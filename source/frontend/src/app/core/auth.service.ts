import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of, tap } from 'rxjs';
import { Perfil } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _perfil = signal<Perfil | null>(null);

  readonly perfil = this._perfil.asReadonly();
  readonly autenticado = computed(() => this._perfil() !== null);
  readonly ehAdmin = computed(() => this._perfil() === 'Admin');

  login(login: string, senha: string) {
    return this.http
      .post<{ perfil: Perfil }>('/auth/login', { login, senha })
      .pipe(tap((resposta) => this._perfil.set(resposta.perfil)));
  }

  logout() {
    return this.http
      .post('/auth/logout', {})
      .pipe(tap(() => this._perfil.set(null)));
  }

  carregarSessao() {
    if (this._perfil() !== null) {
      return of(true);
    }

    return this.http.get<{ perfil: Perfil }>('/auth/me').pipe(
      tap((resposta) => this._perfil.set(resposta.perfil)),
      map(() => true),
      catchError(() => of(false))
    );
  }

  limparSessao() {
    this._perfil.set(null);
  }
}
