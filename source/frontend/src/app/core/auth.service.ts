import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of, tap } from 'rxjs';
import { Modulo, Sessao } from './models';

/** Tela inicial de cada módulo, na ordem em que o login escolhe para onde ir. */
const INICIO_DO_MODULO: Record<Modulo, string> = {
  Obreiros: '/dashboard',
  AcaoSocial: '/acao-social'
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _sessao = signal<Sessao | null>(null);

  readonly perfil = computed(() => this._sessao()?.perfil ?? null);
  readonly usuario = computed(() => this._sessao()?.login ?? null);
  readonly modulos = computed(() => this._sessao()?.modulos ?? []);
  readonly autenticado = computed(() => this._sessao() !== null);
  readonly ehAdmin = computed(() => this.perfil() === 'Admin');

  login(login: string, senha: string) {
    return this.http
      .post<Sessao>('/auth/login', { login, senha })
      .pipe(tap((sessao) => this._sessao.set(sessao)));
  }

  logout() {
    return this.http
      .post('/auth/logout', {})
      .pipe(tap(() => this._sessao.set(null)));
  }

  carregarSessao() {
    if (this._sessao() !== null) {
      return of(true);
    }

    return this.http.get<Sessao>('/auth/me').pipe(
      tap((sessao) => this._sessao.set(sessao)),
      map(() => true),
      catchError(() => of(false))
    );
  }

  limparSessao() {
    this._sessao.set(null);
  }

  temModulo(modulo: Modulo) {
    return this.modulos().includes(modulo);
  }

  /** Primeira área que o usuário acessa. */
  rotaInicial() {
    const modulo = (Object.keys(INICIO_DO_MODULO) as Modulo[]).find((m) => this.temModulo(m));

    return modulo ? INICIO_DO_MODULO[modulo] : '/login';
  }
}
