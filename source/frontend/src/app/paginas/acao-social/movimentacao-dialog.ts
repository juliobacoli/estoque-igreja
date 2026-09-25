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

/** "texto" é o motivo do ajuste; na entrada fica vazio. */
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

        @if (porPacote) {
          <mat-form-field appearance="outline" class="campo-mat">
            <mat-label>Quantos pacotes?</mat-label>
            <input matInput name="pacotes" type="number" inputmode="numeric" min="1" [(ngModel)]="pacotes" required>
            <span matTextSuffix>pacotes</span>
          </mat-form-field>

          <mat-form-field appearance="outline" class="campo-mat">
            <mat-label>Peso de cada pacote</mat-label>
            <input matInput name="kgPorPacote" type="number" inputmode="numeric" min="1" [(ngModel)]="kgPorPacote" required>
            <span matTextSuffix>kg</span>
          </mat-form-field>

          @if (totalKg(); as total) {
            <p class="total">Entra +{{ total }} kg</p>
          }
        } @else {
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
        }

        @if (!entrada) {
          <mat-form-field appearance="outline" class="campo-mat">
            <mat-label>Motivo</mat-label>
            <input matInput name="texto" type="text" [(ngModel)]="texto" required>
            <mat-hint>Ex.: vencido, perdido</mat-hint>
          </mat-form-field>
        }
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

    /* As setas nativas do campo numérico cobrem o sufixo; no celular o teclado já é numérico. */
    input[type="number"] {
      appearance: textfield;
    }

    input[type="number"]::-webkit-outer-spin-button,
    input[type="number"]::-webkit-inner-spin-button {
      -webkit-appearance: none;
      margin: 0;
    }

    .total {
      margin: 0 0 1rem;
      padding: 0.5rem 0.75rem;
      border-radius: 8px;
      background: color-mix(in srgb, var(--dourado) 15%, transparent);
      color: var(--dourado);
      font-weight: 600;
      text-align: center;
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

  // Doação em kg chega em pacotes de tamanhos variados: a tela soma o peso pela pessoa.
  protected readonly porPacote = this.entrada && this.dados.item.unidade === 'kg';
  protected pacotes: number | null = null;
  protected kgPorPacote: number | null = null;

  protected totalKg(): number | null {
    return this.pacotes && this.kgPorPacote ? this.pacotes * this.kgPorPacote : null;
  }

  protected confirmar() {
    const quantidade = this.porPacote ? this.totalKg() : this.quantidade;

    if (quantidade === null) {
      return;
    }

    this.dialogRef.close({ quantidade, texto: this.texto.trim() });
  }
}
