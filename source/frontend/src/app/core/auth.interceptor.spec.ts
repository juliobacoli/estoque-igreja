import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  const auth = { limparSessao: vi.fn() };
  const router = { navigate: vi.fn() };
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    auth.limparSessao.mockReset();
    router.navigate.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router }
      ]
    });

    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  function falhar(url: string, status: number) {
    let erro: HttpErrorResponse | undefined;

    http.get(url).subscribe({ error: (e: HttpErrorResponse) => (erro = e) });
    controller.expectOne(url).flush(null, { status, statusText: 'Erro' });

    return erro;
  }

  it('envia toda requisição com credenciais (cookie)', () => {
    http.get('/api/itens').subscribe();

    const requisicao = controller.expectOne('/api/itens');
    expect(requisicao.request.withCredentials).toBe(true);
    requisicao.flush([]);
  });

  it('401 limpa a sessão, manda para /login e repassa o erro', () => {
    const erro = falhar('/api/itens', 401);

    expect(auth.limparSessao).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
    expect(erro?.status).toBe(401);
  });

  it('401 na verificação de sessão (/auth/me) não redireciona', () => {
    const erro = falhar('/auth/me', 401);

    expect(auth.limparSessao).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
    expect(erro?.status).toBe(401);
  });

  it.each([400, 403, 404, 500])('erro %i só é repassado', (status) => {
    const erro = falhar('/api/itens', status);

    expect(auth.limparSessao).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
    expect(erro?.status).toBe(status);
  });
});
