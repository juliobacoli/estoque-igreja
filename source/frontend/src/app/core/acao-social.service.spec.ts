import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AcaoSocialService } from './acao-social.service';

describe('AcaoSocialService', () => {
  let service: AcaoSocialService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(AcaoSocialService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function esperar(url: string, metodo: string, corpo?: unknown) {
    const requisicao = http.expectOne(url);
    expect(requisicao.request.method).toBe(metodo);

    if (corpo !== undefined) {
      expect(requisicao.request.body).toEqual(corpo);
    }

    requisicao.flush({});
  }

  it('listar busca os itens da Ação Social', () => {
    service.listar().subscribe();
    esperar('/api/acao-social/itens', 'GET');
  });

  it('criar envia nome e unidade', () => {
    service.criar('Arroz', 'kg').subscribe();
    esperar('/api/acao-social/itens', 'POST', { nome: 'Arroz', unidade: 'kg' });
  });

  it('remover envia DELETE do item', () => {
    service.remover('abc').subscribe();
    esperar('/api/acao-social/itens/abc', 'DELETE');
  });

  it('registrarEntrada envia só a quantidade', () => {
    service.registrarEntrada('abc', 5).subscribe();
    esperar('/api/acao-social/itens/abc/entradas', 'POST', { quantidade: 5 });
  });

  it('ajustar envia a nova quantidade e o motivo', () => {
    service.ajustar('abc', 2, 'Venceu').subscribe();
    esperar('/api/acao-social/itens/abc/ajustes', 'POST', { novaQuantidade: 2, motivo: 'Venceu' });
  });

  it('obterCesta busca o resumo da cesta', () => {
    service.obterCesta().subscribe();
    esperar('/api/acao-social/cesta', 'GET');
  });

  it('definirModelo envia os itens da cesta', () => {
    service.definirModelo([{ itemId: 'a', quantidade: 5 }]).subscribe();
    esperar('/api/acao-social/cesta/modelo', 'PUT', { itens: [{ itemId: 'a', quantidade: 5 }] });
  });

  it('montar envia a quantidade de cestas', () => {
    service.montar(3).subscribe();
    esperar('/api/acao-social/cesta/montagens', 'POST', { quantidade: 3 });
  });
});
