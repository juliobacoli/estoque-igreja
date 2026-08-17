import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';
import { shakeAnimation } from '../../shared/animations';
import { DicaSenhaDialog } from './dica-senha-dialog';

/** Tentativas erradas seguidas antes de oferecer a dica. */
const TENTATIVAS_ATE_A_DICA = 3;

@Component({
  selector: 'app-login',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  animations: [shakeAnimation]
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  protected login = '';
  protected senha = '';

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mostrarSenha = signal(false);
  protected readonly shakeState = signal<'idle' | 'shake'>('idle');
  protected readonly destacarOlho = signal(false);

  private tentativasErradas = 0;

  protected alternarSenha() {
    this.mostrarSenha.update((mostrando) => !mostrando);
    this.destacarOlho.set(false);
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
        this.contarErro();
      }
    });
  }

  /**
   * A contagem vive só nesta tela e some ao recarregar. É ajuda ao usuário, não
   * proteção contra força bruta — isso teria que ser feito no servidor.
   */
  private contarErro() {
    this.tentativasErradas += 1;

    if (this.tentativasErradas === TENTATIVAS_ATE_A_DICA && !this.mostrarSenha()) {
      this.dialog
        .open(DicaSenhaDialog, { width: '320px' })
        .afterClosed()
        .subscribe(() => this.destacarOlho.set(true));
    }
  }

  private sacudir() {
    this.shakeState.set('shake');
    setTimeout(() => this.shakeState.set('idle'), 450);
  }
}
