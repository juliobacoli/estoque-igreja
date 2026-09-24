import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';
import { Modulo } from './models';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.carregarSessao().pipe(
    map((autenticado) => (autenticado ? true : router.createUrlTree(['/login'])))
  );
};

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.carregarSessao().pipe(
    map(() => (auth.ehAdmin() ? true : router.createUrlTree(['/dashboard'])))
  );
};

/** Quem não tem o módulo vai para a própria área inicial. A barreira real é o 403 da API. */
export function moduloGuard(modulo: Modulo): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return auth.carregarSessao().pipe(
      map(() => (auth.temModulo(modulo) ? true : router.createUrlTree([auth.rotaInicial()])))
    );
  };
}
