import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Garante o envio do cookie de sessão e trata 401 num lugar só: sessão expirada
 * volta para o login em vez de deixar a tela quebrada.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  const requisicao = req.clone({ withCredentials: true });

  return next(requisicao).pipe(
    catchError((erro: HttpErrorResponse) => {
      // /auth/me responde 401 de propósito quando não há sessão — quem chama
      // já trata, não faz sentido redirecionar.
      const ehVerificacaoDeSessao = req.url.endsWith('/auth/me');

      if (erro.status === 401 && !ehVerificacaoDeSessao) {
        auth.limparSessao();
        router.navigate(['/login']);
      }

      return throwError(() => erro);
    })
  );
};
