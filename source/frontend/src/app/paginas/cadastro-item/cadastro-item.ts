import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';
import { ConfirmacaoDados, ConfirmacaoDialog } from '../../shared/confirmacao-dialog';
import { shakeAnimation } from '../../shared/animations';

@Component({
  selector: 'app-cadastro-item',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './cadastro-item.html',
  styleUrl: './cadastro-item.css',
  animations: [shakeAnimation]
})
export class CadastroItem implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly unidades = ['rolo', 'pacote', 'unidade'];

  protected nome = '';
  protected unidade = '';

  protected readonly itens = signal<Item[]>([]);
  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly removendo = signal<string | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly shakeState = signal<'idle' | 'shake'>('idle');

  ngOnInit(): void {
    this.carregarItens();
  }

  private carregarItens() {
    this.carregando.set(true);

    this.itensService.listar().subscribe({
      next: (itens) => {
        this.itens.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.carregando.set(false);
        this.avisar('Não foi possível carregar os itens.');
      }
    });
  }

  protected cadastrar() {
    this.erro.set(null);
    this.salvando.set(true);

    this.itensService.criar(this.nome, this.unidade).subscribe({
      next: (item) => {
        this.avisar(`"${item.nome}" cadastrado.`);
        this.nome = '';
        this.unidade = '';
        this.salvando.set(false);
        this.carregarItens();
      },
      error: (resposta) => {
        this.erro.set(resposta.error?.error ?? 'Não foi possível cadastrar o item.');
        this.salvando.set(false);
        this.sacudir();
      }
    });
  }

  protected async remover(item: Item) {
    const dados: ConfirmacaoDados = {
      titulo: 'Remover item',
      mensagem: `"${item.nome}" sai da lista de estoque. Se ele já foi contado alguma vez, o histórico é mantido.`,
      confirmar: 'Remover'
    };

    const confirmou = await firstValueFrom(
      this.dialog.open(ConfirmacaoDialog, { data: dados }).afterClosed()
    );

    if (!confirmou) {
      return;
    }

    this.removendo.set(item.id);

    this.itensService.remover(item.id).subscribe({
      next: (resultado) => {
        this.avisar(
          resultado.removido
            ? 'Item excluído.'
            : 'Item removido da lista — o histórico foi mantido.'
        );
        this.removendo.set(null);
        this.carregarItens();
      },
      error: (resposta) => {
        this.avisar(resposta.error?.error ?? 'Não foi possível remover o item.');
        this.removendo.set(null);
      }
    });
  }

  private sacudir() {
    this.shakeState.set('shake');
    setTimeout(() => this.shakeState.set('idle'), 450);
  }

  private avisar(mensagem: string) {
    this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
  }
}
