import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';

/**
 * Aparece depois de algumas tentativas erradas. A graça é o cadeado sacudindo,
 * mas a mensagem resolve a causa real no celular: senha digitada errada sem que
 * a pessoa consiga ver o que escreveu.
 */
@Component({
  selector: 'app-dica-senha-dialog',
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <div class="dica">
      <div class="cadeado" aria-hidden="true">
        <svg viewBox="0 0 24 24">
          <rect x="4" y="10.5" width="16" height="10" rx="2.5" />
          <path d="M8 10.5V7.5a4 4 0 0 1 8 0v3" />
          <circle cx="12" cy="15" r="1.4" />
        </svg>
      </div>

      <h2 mat-dialog-title class="titulo">Opa, calma lá!</h2>

      <mat-dialog-content>
        <p>
          Já foram três tentativas. Toca no
          <strong class="dourado">olhinho</strong> ao lado da senha pra ver o que você
          digitou — às vezes é só um espaço sobrando no fim.
        </p>
      </mat-dialog-content>

      <mat-dialog-actions>
        <button mat-flat-button class="tentar" (click)="dialogRef.close()">
          Vou tentar de novo
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: `
    .dica {
      text-align: center;
      padding: 0.5rem 0.25rem;
    }

    .titulo {
      padding: 0;
      justify-content: center;
    }

    .cadeado {
      width: 72px;
      height: 72px;
      margin: 0 auto 0.5rem;
      color: var(--dourado);
      animation: chacoalha 900ms cubic-bezier(0.36, 0.07, 0.19, 0.97) 200ms both;
    }

    .cadeado svg {
      width: 100%;
      height: 100%;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.6;
      stroke-linecap: round;
      stroke-linejoin: round;
    }

    mat-dialog-content p {
      margin: 0;
      line-height: 1.5;
    }

    mat-dialog-actions {
      justify-content: center;
      padding-top: 1rem;
    }

    .tentar {
      min-height: 48px;
      --mdc-filled-button-container-color: var(--roxo-primario);
      --mdc-filled-button-label-text-color: #fff;
    }

    /* Balança e gira um pouco: o cadeado "não abre" de jeito nenhum. */
    @keyframes chacoalha {
      0% { transform: rotate(0) translateX(0); }
      15% { transform: rotate(-9deg) translateX(-5px); }
      30% { transform: rotate(9deg) translateX(5px); }
      45% { transform: rotate(-7deg) translateX(-4px); }
      60% { transform: rotate(7deg) translateX(4px); }
      75% { transform: rotate(-4deg) translateX(-2px); }
      100% { transform: rotate(0) translateX(0); }
    }

    @media (prefers-reduced-motion: reduce) {
      .cadeado {
        animation: none;
      }
    }
  `
})
export class DicaSenhaDialog {
  protected readonly dialogRef = inject(MatDialogRef<DicaSenhaDialog>);
}
