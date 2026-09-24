import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { alteracoesNaoSalvasGuard } from './core/alteracoes-nao-salvas.guard';
import { adminGuard, authGuard, moduloGuard } from './core/auth.guard';
import { AuthService } from './core/auth.service';

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
      { path: '', pathMatch: 'full', redirectTo: () => inject(AuthService).rotaInicial() },
      {
        path: 'dashboard',
        canActivate: [moduloGuard('Obreiros')],
        loadComponent: () => import('./paginas/dashboard/dashboard').then((m) => m.Dashboard)
      },
      {
        path: 'atualizar',
        canActivate: [moduloGuard('Obreiros')],
        canDeactivate: [alteracoesNaoSalvasGuard],
        loadComponent: () =>
          import('./paginas/atualizar-estoque/atualizar-estoque').then((m) => m.AtualizarEstoque)
      },
      {
        path: 'cadastro',
        canActivate: [moduloGuard('Obreiros'), adminGuard],
        loadComponent: () =>
          import('./paginas/cadastro-item/cadastro-item').then((m) => m.CadastroItem)
      },
      {
        path: 'historico',
        canActivate: [moduloGuard('Obreiros')],
        loadComponent: () => import('./paginas/historico/historico').then((m) => m.Historico)
      },
      {
        path: 'acao-social',
        canActivate: [moduloGuard('AcaoSocial')],
        loadComponent: () =>
          import('./paginas/acao-social/acao-social').then((m) => m.AcaoSocial)
      },
      {
        path: 'acao-social/itens',
        canActivate: [moduloGuard('AcaoSocial'), adminGuard],
        loadComponent: () =>
          import('./paginas/cadastro-item-social/cadastro-item-social').then((m) => m.CadastroItemSocial)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
