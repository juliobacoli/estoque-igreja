import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ItemSocial } from '../../core/models';

export interface MovimentacaoDados {
  tipo: 'entrada' | 'ajuste';
  item: ItemSocial;
}

/** Na entrada, "texto" é o doador (opcional); no ajuste, é o motivo (obrigatório). */
export interface MovimentacaoInformada {
  quantidade: number;
  texto: string;
}

@Component({
  selector: 'app-movimentacao-dialog',
  imports: [FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <form #form="ngForm" (ngSubmit)="confirmar()">
      <h2 mat-dialog-title>{{ entrada ? 'Registrar doação' : 'Ajustar estoque' }}</h2>

      <mat-dialog-content>
        <p class="muted">
          {{ dados.item.nome }} · hoje: {{ dados.item.estoqueAtual }} {{ dados.item.unidade }}
        </p>

        <mat-form-field appearance="outline" class="campo-mat">
          <mat-label>{{ entrada ? 'Quantidade recebida' : 'Quantidade que existe agora' }}</mat-label>
          <input
            matInput
            name="quantidade"
            type="number"
            inputmode="numeric"
            [min]="entrada ? 1 : 0"
            [(ngModel)]="quantidade"
            required>
          <span matTextSuffix>{{ dados.item.unidade }}</span>
        </mat-form-field>

        <mat-form-field appearance="outline" class="campo-mat">
          <mat-label>{{ entrada ? 'Quem doou (opcional)' : 'Motivo' }}</mat-label>
          <input matInput name="texto" type="text" [(ngModel)]="texto" [required]="!entrada">
          @if (!entrada) {
            <mat-hint>Ex.: vencido, perdido</mat-hint>
          }
        </mat-form-field>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="dialogRef.close()">Cancelar</button>
        <button mat-flat-button class="salvar" type="submit" [disabled]="form.invalid">Salvar</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .campo-mat {
      width: 100%;
    }

    .salvar {
      --mat-button-filled-container-color: var(--roxo-primario);
      --mat-button-filled-label-text-color: #fff;
    }
  `
})
export class MovimentacaoDialog {
  protected readonly dialogRef = inject(MatDialogRef<MovimentacaoDialog, MovimentacaoInformada>);
  protected readonly dados = inject<MovimentacaoDados>(MAT_DIALOG_DATA);

  protected readonly entrada = this.dados.tipo === 'entrada';
  protected quantidade: number | null = this.entrada ? null : this.dados.item.estoqueAtual;
  protected texto = '';

  protected confirmar() {
    if (this.quantidade === null) {
      return;
    }

    this.dialogRef.close({ quantidade: this.quantidade, texto: this.texto.trim() });
  }
}
