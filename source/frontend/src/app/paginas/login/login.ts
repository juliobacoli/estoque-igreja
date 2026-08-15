import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected login = '';
  protected senha = '';

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected entrar() {
    this.erro.set(null);
    this.enviando.set(true);

    this.auth.login(this.login, this.senha).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (resposta) => {
        // A API já devolve mensagem genérica, sem dizer qual campo está errado.
        this.erro.set(resposta.error?.error ?? 'Não foi possível entrar. Tente de novo.');
        this.enviando.set(false);
      }
    });
  }
}
