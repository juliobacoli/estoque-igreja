import { Routes } from '@angular/router';
import { alteracoesNaoSalvasGuard } from './core/alteracoes-nao-salvas.guard';
import { adminGuard, authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./paginas/login/login').then((m) => m.Login)
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./paginas/dashboard/dashboard').then((m) => m.Dashboard)
      },
      {
        path: 'atualizar',
        canDeactivate: [alteracoesNaoSalvasGuard],
        loadComponent: () =>
          import('./paginas/atualizar-estoque/atualizar-estoque').then((m) => m.AtualizarEstoque)
      },
      {
        path: 'cadastro',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./paginas/cadastro-item/cadastro-item').then((m) => m.CadastroItem)
      },
      {
        path: 'historico',
        loadComponent: () => import('./paginas/historico/historico').then((m) => m.Historico)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
