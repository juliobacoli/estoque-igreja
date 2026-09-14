import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function logarComo(perfil: 'Admin' | 'Voluntario') {
    service.login('usuario', 'senha').subscribe();
    http.expectOne('/auth/login').flush({ perfil });
  }

  it('login com sucesso envia as credenciais e guarda o perfil', () => {
    service.login('admin', 'senha').subscribe();

    const requisicao = http.expectOne('/auth/login');
    expect(requisicao.request.method).toBe('POST');
    expect(requisicao.request.body).toEqual({ login: 'admin', senha: 'senha' });
    requisicao.flush({ perfil: 'Admin' });

    expect(service.perfil()).toBe('Admin');
    expect(service.autenticado()).toBe(true);
    expect(service.ehAdmin()).toBe(true);
  });

  it('login de voluntário autentica sem ser admin', () => {
    logarComo('Voluntario');

    expect(service.autenticado()).toBe(true);
    expect(service.ehAdmin()).toBe(false);
  });

  it('login com erro não altera o perfil', () => {
    service.login('admin', 'errada').subscribe({ error: () => undefined });

    http.expectOne('/auth/login').flush(
      { error: 'Login ou senha inválidos' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(service.perfil()).toBeNull();
    expect(service.autenticado()).toBe(false);
  });

  it('carregarSessao com perfil já carregado retorna true sem chamar a API', () => {
    logarComo('Admin');
    let resultado: boolean | undefined;

    service.carregarSessao().subscribe((valor) => (resultado = valor));

    http.expectNone('/auth/me');
    expect(resultado).toBe(true);
  });

  it('carregarSessao sem perfil busca /auth/me e guarda o perfil', () => {
    let resultado: boolean | undefined;

    service.carregarSessao().subscribe((valor) => (resultado = valor));
    http.expectOne('/auth/me').flush({ perfil: 'Voluntario' });

    expect(resultado).toBe(true);
    expect(service.perfil()).toBe('Voluntario');
  });

  it('carregarSessao com erro em /auth/me retorna false sem lançar', () => {
    let resultado: boolean | undefined;
    let falhou = false;

    service.carregarSessao().subscribe({
      next: (valor) => (resultado = valor),
      error: () => (falhou = true)
    });
    http.expectOne('/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(resultado).toBe(false);
    expect(falhou).toBe(false);
    expect(service.autenticado()).toBe(false);
  });

  it('logout limpa o perfil só depois da resposta', () => {
    logarComo('Admin');

    service.logout().subscribe();
    const requisicao = http.expectOne('/auth/logout');
    expect(service.perfil()).toBe('Admin');

    requisicao.flush({});
    expect(service.perfil()).toBeNull();
  });

  it('limparSessao limpa o perfil imediatamente', () => {
    logarComo('Admin');

    service.limparSessao();

    expect(service.perfil()).toBeNull();
    expect(service.autenticado()).toBe(false);
  });
});
