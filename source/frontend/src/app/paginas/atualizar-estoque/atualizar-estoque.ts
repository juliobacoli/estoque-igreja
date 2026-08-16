import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';

interface LinhaContagem {
  item: Item;
  quantidade: number;
}

@Component({
  selector: 'app-atualizar-estoque',
  imports: [FormsModule],
  templateUrl: './atualizar-estoque.html',
  styleUrl: './atualizar-estoque.css'
})
export class AtualizarEstoque implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly router = inject(Router);

  protected readonly linhas = signal<LinhaContagem[]>([]);
  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);

  ngOnInit(): void {
    this.itensService.listar().subscribe({
      next: (itens) => {
        this.linhas.set(itens.map((item) => ({ item, quantidade: item.estoqueAtual })));
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Não foi possível carregar os itens.');
        this.carregando.set(false);
      }
    });
  }

  protected alterar(id: string, valor: string) {
    const numero = Number(valor);

    this.linhas.update((linhas) =>
      linhas.map((linha) =>
        linha.item.id === id
          ? { ...linha, quantidade: Number.isFinite(numero) ? numero : linha.quantidade }
          : linha
      )
    );
  }

  protected temAlteracao() {
    return this.linhas().some((linha) => linha.quantidade !== linha.item.estoqueAtual);
  }

  /**
   * Envia só os itens cuja contagem mudou. Cada um vira um PUT próprio, que é o
   * contrato do Capítulo 3 — e um registro de histórico no servidor.
   */
  protected salvar() {
    const alterados = this.linhas().filter((linha) => linha.quantidade !== linha.item.estoqueAtual);

    if (alterados.length === 0) {
      return;
    }

    if (alterados.some((linha) => linha.quantidade < 0)) {
      this.erro.set('A quantidade não pode ser negativa.');
      return;
    }

    this.erro.set(null);
    this.salvando.set(true);

    forkJoin(
      alterados.map((linha) => this.itensService.atualizarEstoque(linha.item.id, linha.quantidade))
    ).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (resposta) => {
        this.erro.set(resposta.error?.error ?? 'Não foi possível salvar a contagem.');
        this.salvando.set(false);
      }
    });
  }
}
