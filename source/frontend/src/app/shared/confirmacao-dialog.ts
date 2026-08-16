import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmacaoDados {
  titulo: string;
  mensagem: string;
  confirmar: string;
}

@Component({
  selector: 'app-confirmacao-dialog',
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ dados.titulo }}</h2>

    <mat-dialog-content>
      <p>{{ dados.mensagem }}</p>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <!-- Cancelar é o botão neutro e fica antes: a ação destrutiva não deve ser
           a mais fácil de acertar sem querer. -->
      <button mat-button (click)="dialogRef.close(false)">Cancelar</button>
      <button mat-flat-button class="perigo" (click)="dialogRef.close(true)">
        {{ dados.confirmar }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .perigo {
      --mdc-filled-button-container-color: #b3261e;
      --mdc-filled-button-label-text-color: #fff;
    }
  `
})
export class ConfirmacaoDialog {
  protected readonly dialogRef = inject(MatDialogRef<ConfirmacaoDialog, boolean>);
  protected readonly dados = inject<ConfirmacaoDados>(MAT_DIALOG_DATA);
}
