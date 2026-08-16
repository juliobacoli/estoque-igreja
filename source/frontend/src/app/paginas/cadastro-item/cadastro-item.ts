import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ItensService } from '../../core/itens.service';

@Component({
  selector: 'app-cadastro-item',
  imports: [FormsModule],
  templateUrl: './cadastro-item.html',
  styleUrl: './cadastro-item.css'
})
export class CadastroItem {
  private readonly itensService = inject(ItensService);

  protected readonly unidades = ['rolo', 'pacote', 'unidade'];

  protected nome = '';
  protected unidade = '';

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal<string | null>(null);

  protected cadastrar() {
    this.erro.set(null);
    this.sucesso.set(null);
    this.salvando.set(true);

    this.itensService.criar(this.nome, this.unidade).subscribe({
      next: (item) => {
        this.sucesso.set(`"${item.nome}" cadastrado.`);
        this.nome = '';
        this.unidade = '';
        this.salvando.set(false);
      },
      error: (resposta) => {
        // 400 com "Já existe um item com esse nome" vem do índice único
        // normalizado — pega duplicata com acento ou caixa diferente.
        this.erro.set(resposta.error?.error ?? 'Não foi possível cadastrar o item.');
        this.salvando.set(false);
      }
    });
  }
}
