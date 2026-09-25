import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Observable, firstValueFrom } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { AuthService } from '../../core/auth.service';
import { EstoqueSocialAlterado, ItemSocial } from '../../core/models';
import { MovimentacaoDados, MovimentacaoDialog, MovimentacaoInformada } from './movimentacao-dialog';
import { unidadePara } from '../../core/unidade';

@Component({
  selector: 'app-acao-social',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatDialogModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './acao-social.html',
  styleUrl: './acao-social.css'
})
export class AcaoSocial implements OnInit {
  protected readonly unidadePara = unidadePara;
  private readonly acaoSocial = inject(AcaoSocialService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly ehAdmin = inject(AuthService).ehAdmin;
  protected readonly itens = signal<ItemSocial[]>([]);
  protected readonly carregando = signal(true);
  protected readonly falhaAoCarregar = signal(false);
  // Por item: o backend trava só o item alterado, então os outros seguem liberados.
  protected readonly salvando = signal<ReadonlySet<string>>(new Set());

  ngOnInit(): void {
    this.carregar();
  }

  private carregar() {
    this.acaoSocial.listar().subscribe({
      next: (itens) => {
        this.itens.set(itens);
        this.falhaAoCarregar.set(false);
        this.carregando.set(false);
      },
      error: () => {
        this.falhaAoCarregar.set(true);
        this.carregando.set(false);
      }
    });
  }

  protected registrarDoacao(item: ItemSocial) {
    this.movimentar({ tipo: 'entrada', item }, (informado) =>
      this.acaoSocial.registrarEntrada(item.id, informado.quantidade)
    );
  }

  protected ajustar(item: ItemSocial) {
    this.movimentar({ tipo: 'ajuste', item }, (informado) =>
      this.acaoSocial.ajustar(item.id, informado.quantidade, informado.texto)
    );
  }

  private async movimentar(
    dados: MovimentacaoDados,
    enviar: (informado: MovimentacaoInformada) => Observable<EstoqueSocialAlterado>
  ) {
    const informado = await firstValueFrom(
      this.dialog.open(MovimentacaoDialog, { data: dados, width: '340px' }).afterClosed()
    );

    if (!informado) {
      return;
    }

    const id = dados.item.id;
    this.salvando.update((ids) => new Set(ids).add(id));

    enviar(informado).subscribe({
      next: (resultado) => {
        this.avisar(`${dados.item.nome}: agora ${resultado.quantidadeNova} ${unidadePara(resultado.quantidadeNova, dados.item.unidade)}.`);
        // A resposta já traz o estoque novo: atualiza a linha sem buscar a lista de novo.
        this.itens.update((itens) =>
          itens.map((i) => (i.id === id ? { ...i, estoqueAtual: resultado.quantidadeNova } : i))
        );
        this.pararDeSalvar(id);
      },
      error: (resposta) => {
        this.avisar(resposta.error?.error ?? 'Não foi possível salvar.');
        this.pararDeSalvar(id);
      }
    });
  }

  private pararDeSalvar(id: string) {
    this.salvando.update((ids) => {
      const restantes = new Set(ids);
      restantes.delete(id);
      return restantes;
    });
  }

  private avisar(mensagem: string) {
    this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
  }
}
