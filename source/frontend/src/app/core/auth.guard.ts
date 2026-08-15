import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.carregarSessao().pipe(
    map((autenticado) => (autenticado ? true : router.createUrlTree(['/login'])))
  );
};

/** Cadastro de Itens é só do Admin (Capítulo 2, item 2.6). */
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.carregarSessao().pipe(
    map(() => (auth.ehAdmin() ? true : router.createUrlTree(['/dashboard'])))
  );
};
