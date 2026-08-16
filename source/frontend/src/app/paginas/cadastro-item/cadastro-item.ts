import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ItensService } from '../../core/itens.service';
import { shakeAnimation } from '../../shared/animations';

@Component({
  selector: 'app-cadastro-item',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    // MatSnackBarModule é obrigatório: o MatSnackBar não é providedIn root,
    // quem o registra é o módulo.
    MatSnackBarModule
  ],
  templateUrl: './cadastro-item.html',
  styleUrl: './cadastro-item.css',
  animations: [shakeAnimation]
})
export class CadastroItem {
  private readonly itensService = inject(ItensService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly unidades = ['rolo', 'pacote', 'unidade'];

  protected nome = '';
  protected unidade = '';

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly shakeState = signal<'idle' | 'shake'>('idle');

  protected cadastrar() {
    this.erro.set(null);
    this.salvando.set(true);

    this.itensService.criar(this.nome, this.unidade).subscribe({
      next: (item) => {
        this.snackBar.open(`"${item.nome}" cadastrado.`, 'Fechar', { duration: 4000 });
        this.nome = '';
        this.unidade = '';
        this.salvando.set(false);
      },
      error: (resposta) => {
        // 400 com "Já existe um item com esse nome" vem do índice único
        // normalizado — pega duplicata com acento ou caixa diferente.
        this.erro.set(resposta.error?.error ?? 'Não foi possível cadastrar o item.');
        this.salvando.set(false);
        this.sacudir();
      }
    });
  }

  private sacudir() {
    this.shakeState.set('shake');
    setTimeout(() => this.shakeState.set('idle'), 450);
  }
}
