import { Route } from '@angular/router';
import { routes } from './app.routes';
import { alteracoesNaoSalvasGuard } from './core/alteracoes-nao-salvas.guard';
import { adminGuard, authGuard } from './core/auth.guard';
import { Shell } from './layout/shell';
import { AtualizarEstoque } from './paginas/atualizar-estoque/atualizar-estoque';
import { CadastroItem } from './paginas/cadastro-item/cadastro-item';
import { Dashboard } from './paginas/dashboard/dashboard';
import { Historico } from './paginas/historico/historico';
import { Login } from './paginas/login/login';

describe('Rotas', () => {
  const shell = routes.find((r) => r.path === '')!;
  const filha = (path: string) => shell.children!.find((r) => r.path === path)!;

  it('rota desconhecida volta para o início, e o início abre o dashboard', () => {
    expect(routes.find((r) => r.path === '**')?.redirectTo).toBe('');
    expect(filha('')).toMatchObject({ pathMatch: 'full', redirectTo: 'dashboard' });
  });

  it('login é público e o restante exige autenticação', () => {
    expect(routes.find((r) => r.path === 'login')?.canActivate).toBeUndefined();
    expect(shell.canActivate).toContain(authGuard);
  });

  it('cadastro exige admin', () => {
    expect(filha('cadastro').canActivate).toContain(adminGuard);
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
    ['historico', () => filha('historico'), Historico]
  ])('rota %s carrega o componente certo', async (_, rota, componente) => {
    expect(await rota().loadComponent!()).toBe(componente);
  });
});
