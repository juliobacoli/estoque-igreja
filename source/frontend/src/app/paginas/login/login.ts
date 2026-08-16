import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';
import { shakeAnimation } from '../../shared/animations';

@Component({
  selector: 'app-login',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  animations: [shakeAnimation]
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected login = '';
  protected senha = '';

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mostrarSenha = signal(false);
  protected readonly shakeState = signal<'idle' | 'shake'>('idle');

  protected alternarSenha() {
    this.mostrarSenha.update((mostrando) => !mostrando);
  }

  protected entrar() {
    this.erro.set(null);
    this.enviando.set(true);

    this.auth.login(this.login, this.senha).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (resposta) => {
        // Mensagem única, sem revelar se o errado foi o login ou a senha.
        this.erro.set(resposta.error?.error ?? 'Login ou senha inválidos');
        this.enviando.set(false);
        this.sacudir();
      }
    });
  }

  /**
   * Volta o estado para 'idle' ao fim da animação, senão uma segunda falha
   * seguida não dispararia a transição de novo.
   */
  private sacudir() {
    this.shakeState.set('shake');
    setTimeout(() => this.shakeState.set('idle'), 450);
  }
}
