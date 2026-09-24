import { TestBed } from '@angular/core/testing';
import { Route } from '@angular/router';
import { routes } from './app.routes';
import { alteracoesNaoSalvasGuard } from './core/alteracoes-nao-salvas.guard';
import { adminGuard, authGuard } from './core/auth.guard';
import { AuthService } from './core/auth.service';
import { Shell } from './layout/shell';
import { AcaoSocial } from './paginas/acao-social/acao-social';
import { CadastroItemSocial } from './paginas/cadastro-item-social/cadastro-item-social';
import { AtualizarEstoque } from './paginas/atualizar-estoque/atualizar-estoque';
import { CadastroItem } from './paginas/cadastro-item/cadastro-item';
import { Dashboard } from './paginas/dashboard/dashboard';
import { Historico } from './paginas/historico/historico';
import { Login } from './paginas/login/login';

describe('Rotas', () => {
  const shell = routes.find((r) => r.path === '')!;
  const filha = (path: string) => shell.children!.find((r) => r.path === path)!;

  it('rota desconhecida volta para o início, e o início abre a área do usuário', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: AuthService, useValue: { rotaInicial: () => '/acao-social' } }]
    });
    const inicio = filha('');

    expect(routes.find((r) => r.path === '**')?.redirectTo).toBe('');
    expect(inicio.pathMatch).toBe('full');
    expect(TestBed.runInInjectionContext(() => (inicio.redirectTo as () => string)())).toBe('/acao-social');
  });

  it('login é público e o restante exige autenticação', () => {
    expect(routes.find((r) => r.path === 'login')?.canActivate).toBeUndefined();
    expect(shell.canActivate).toContain(authGuard);
  });

  it.each(['dashboard', 'atualizar', 'cadastro', 'historico', 'acao-social', 'acao-social/itens'])(
    'rota %s exige o módulo dela',
    (path) => {
      expect(filha(path).canActivate?.length).toBeGreaterThan(0);
    }
  );

  it.each(['cadastro', 'acao-social/itens'])('%s exige admin', (path) => {
    expect(filha(path).canActivate).toContain(adminGuard);
  });

  it('atualizar estoque avisa sobre alterações não salvas', () => {
    expect(filha('atualizar').canDeactivate).toContain(alteracoesNaoSalvasGuard);
  });

  it.each<[string, () => Route, unknown]>([
    ['login', () => routes.find((r) => r.path === 'login')!, Login],
    ['shell', () => shell, Shell],
    ['dashboard', () => filha('dashboard'), Dashboard],
    ['atualizar', () => filha('atualizar'), AtualizarEstoque],
    ['cadastro', () => filha('cadastro'), CadastroItem],
    ['historico', () => filha('historico'), Historico],
    ['acao-social', () => filha('acao-social'), AcaoSocial],
    ['acao-social/itens', () => filha('acao-social/itens'), CadastroItemSocial]
  ])('rota %s carrega o componente certo', async (_, rota, componente) => {
    expect(await rota().loadComponent!()).toBe(componente);
  });
});
