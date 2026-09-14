import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ExportService } from './export.service';

describe('ExportService', () => {
  let service: ExportService;
  let clicarLink: ReturnType<typeof vi.spyOn>;
  const pdf = new Blob(['%PDF'], { type: 'application/pdf' });

  // O jsdom não implementa compartilhamento nem URLs de blob: cada teste define o que precisa.
  function definirNavigator(canShare: unknown, share: unknown) {
    Object.defineProperty(navigator, 'canShare', { value: canShare, configurable: true, writable: true });
    Object.defineProperty(navigator, 'share', { value: share, configurable: true, writable: true });
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(ExportService);

    Object.defineProperty(URL, 'createObjectURL', { value: vi.fn(() => 'blob:pdf'), configurable: true, writable: true });
    Object.defineProperty(URL, 'revokeObjectURL', { value: vi.fn(), configurable: true, writable: true });
    clicarLink = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);
  });

  afterEach(() => {
    definirNavigator(undefined, undefined);
    vi.restoreAllMocks();
  });

  it('baixarPdf pede o PDF como blob', () => {
    const http = TestBed.inject(HttpTestingController);

    service.baixarPdf().subscribe();

    const requisicao = http.expectOne('/api/estoque/exportar-pdf');
    expect(requisicao.request.responseType).toBe('blob');
    requisicao.flush(pdf);
    http.verify();
  });

  it('compartilha o arquivo quando o aparelho suporta', async () => {
    const share = vi.fn().mockResolvedValue(undefined);
    definirNavigator(vi.fn(() => true), share);

    const resultado = await service.compartilhar(pdf);

    expect(resultado).toBe('compartilhado');
    const dados = share.mock.calls[0][0] as ShareData;
    expect(dados.title).toBe('Estoque ICPA');
    expect(dados.files?.[0].name).toBe('estoque.pdf');
    expect(clicarLink).not.toHaveBeenCalled();
  });

  it('retorna cancelado quando o usuário fecha o compartilhamento', async () => {
    definirNavigator(vi.fn(() => true), vi.fn().mockRejectedValue(new DOMException('fechou', 'AbortError')));

    const resultado = await service.compartilhar(pdf);

    expect(resultado).toBe('cancelado');
    expect(clicarLink).not.toHaveBeenCalled();
  });

  it('baixa o arquivo quando o compartilhamento falha por outro motivo', async () => {
    definirNavigator(vi.fn(() => true), vi.fn().mockRejectedValue(new Error('falhou')));

    const resultado = await service.compartilhar(pdf);

    expect(resultado).toBe('baixado');
    expect(clicarLink).toHaveBeenCalledOnce();
  });

  it('baixa o arquivo quando o aparelho não suporta compartilhar', async () => {
    const resultado = await service.compartilhar(pdf);

    expect(resultado).toBe('baixado');
    expect(URL.createObjectURL).toHaveBeenCalledWith(pdf);
    expect(clicarLink).toHaveBeenCalledOnce();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:pdf');
  });
});
