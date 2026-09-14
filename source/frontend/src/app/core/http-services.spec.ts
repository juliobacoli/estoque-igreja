import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { EstoqueService } from './estoque.service';
import { HistoricoService } from './historico.service';
import { ItensService } from './itens.service';

describe('Services HTTP', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  describe('ItensService', () => {
    let service: ItensService;

    beforeEach(() => (service = TestBed.inject(ItensService)));

    it('listar() não envia incluirInativos', () => {
      service.listar().subscribe();

      const requisicao = http.expectOne((r) => r.url === '/api/itens');
      expect(requisicao.request.method).toBe('GET');
      expect(requisicao.request.urlWithParams).toBe('/api/itens');
      requisicao.flush([]);
    });

    it('listar(true) envia incluirInativos=true', () => {
      service.listar(true).subscribe();

      const requisicao = http.expectOne((r) => r.url === '/api/itens');
      expect(requisicao.request.urlWithParams).toBe('/api/itens?incluirInativos=true');
      requisicao.flush([]);
    });

    it('criar envia POST com nome e unidade', () => {
      service.criar('Sabão', 'unidade').subscribe();

      const requisicao = http.expectOne('/api/itens');
      expect(requisicao.request.method).toBe('POST');
      expect(requisicao.request.body).toEqual({ nome: 'Sabão', unidade: 'unidade' });
      requisicao.flush({});
    });

    it('atualizarEstoque envia PUT com a nova quantidade', () => {
      service.atualizarEstoque('abc', 5).subscribe();

      const requisicao = http.expectOne('/api/itens/abc/estoque');
      expect(requisicao.request.method).toBe('PUT');
      expect(requisicao.request.body).toEqual({ novaQuantidade: 5 });
      requisicao.flush({});
    });

    it('remover envia DELETE para o item', () => {
      service.remover('abc').subscribe();

      const requisicao = http.expectOne('/api/itens/abc');
      expect(requisicao.request.method).toBe('DELETE');
      requisicao.flush({ removido: true });
    });
  });

  describe('HistoricoService', () => {
    let service: HistoricoService;

    beforeEach(() => (service = TestBed.inject(HistoricoService)));

    it('envia página e tamanho sem itemId quando não informado', () => {
      service.listar(2, 20).subscribe();

      const requisicao = http.expectOne((r) => r.url === '/api/historico');
      expect(requisicao.request.urlWithParams).toBe('/api/historico?pagina=2&tamanhoPagina=20');
      requisicao.flush({ registros: [], temMaisPaginas: false });
    });

    it('envia itemId quando informado', () => {
      service.listar(1, 20, 'abc').subscribe();

      const requisicao = http.expectOne((r) => r.url === '/api/historico');
      expect(requisicao.request.urlWithParams).toBe('/api/historico?pagina=1&tamanhoPagina=20&itemId=abc');
      requisicao.flush({ registros: [], temMaisPaginas: false });
    });
  });

  describe('EstoqueService', () => {
    it('ultimaAtualizacao chama o endpoint da última contagem', () => {
      TestBed.inject(EstoqueService).ultimaAtualizacao().subscribe();

      const requisicao = http.expectOne('/api/estoque/ultima-atualizacao');
      expect(requisicao.request.method).toBe('GET');
      requisicao.flush({ data: null });
    });
  });
});
