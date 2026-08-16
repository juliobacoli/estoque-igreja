import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';

interface LinhaContagem {
  item: Item;
  quantidade: number;
}

@Component({
  selector: 'app-atualizar-estoque',
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    // MatSnackBarModule é obrigatório: o MatSnackBar não é providedIn root,
    // quem o registra é o módulo.
    MatSnackBarModule
  ],
  templateUrl: './atualizar-estoque.html',
  styleUrl: './atualizar-estoque.css'
})
export class AtualizarEstoque implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly linhas = signal<LinhaContagem[]>([]);
  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly falhaAoCarregar = signal(false);

  ngOnInit(): void {
    this.carregar();
  }

  private carregar() {
    this.carregando.set(true);

    this.itensService.listar().subscribe({
      next: (itens) => {
        this.linhas.set(itens.map((item) => ({ item, quantidade: item.estoqueAtual })));
        this.falhaAoCarregar.set(false);
        this.carregando.set(false);
      },
      error: () => {
        this.falhaAoCarregar.set(true);
        this.carregando.set(false);
        this.avisar('Não foi possível carregar os itens.');
      }
    });
  }

  protected alterar(id: string, valor: string) {
    const numero = Number(valor);

    this.definir(id, Number.isFinite(numero) ? numero : null);
  }

  protected somar(id: string, passo: number) {
    const linha = this.linhas().find((l) => l.item.id === id);

    if (linha) {
      this.definir(id, linha.quantidade + passo);
    }
  }

  /** Nunca abaixo de zero — a mesma regra que o servidor aplica. */
  private definir(id: string, valor: number | null) {
    this.linhas.update((linhas) =>
      linhas.map((linha) =>
        linha.item.id === id
          ? { ...linha, quantidade: valor === null ? linha.quantidade : Math.max(0, valor) }
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

    this.salvando.set(true);

    forkJoin(
      alterados.map((linha) => this.itensService.atualizarEstoque(linha.item.id, linha.quantidade))
    ).subscribe({
      next: () => {
        this.avisar(
          alterados.length === 1 ? 'Contagem salva.' : `${alterados.length} itens atualizados.`
        );
        this.router.navigate(['/dashboard']);
      },
      error: (resposta) => {
        this.salvando.set(false);

        // 404 aqui significa que um item foi removido por um administrador depois
        // que esta tela carregou. A lista em mãos está velha, então recarrega —
        // insistir no mesmo save falharia de novo.
        if (resposta.status === 404) {
          this.avisar('Um item foi removido por um administrador. A lista foi atualizada.');
          this.carregar();
          return;
        }

        this.avisar(resposta.error?.error ?? 'Não foi possível salvar a contagem.');
      }
    });
  }

  private avisar(mensagem: string) {
    this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
  }
}
