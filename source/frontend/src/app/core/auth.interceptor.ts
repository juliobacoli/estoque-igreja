import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  const requisicao = req.clone({ withCredentials: true });

  return next(requisicao).pipe(
    catchError((erro: HttpErrorResponse) => {
      const ehVerificacaoDeSessao = req.url.endsWith('/auth/me');

      if (erro.status === 401 && !ehVerificacaoDeSessao) {
        auth.limparSessao();
        router.navigate(['/login']);
      }

      return throwError(() => erro);
    })
  );
};
