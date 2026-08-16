import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HistoricoService } from '../../core/historico.service';
import { ItensService } from '../../core/itens.service';
import { Item, RegistroHistorico } from '../../core/models';

const TAMANHO_PAGINA = 20;

@Component({
  selector: 'app-historico',
  imports: [
    FormsModule,
    DatePipe,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    // MatSnackBarModule é obrigatório: o MatSnackBar não é providedIn root,
    // quem o registra é o módulo.
    MatSnackBarModule
  ],
  templateUrl: './historico.html',
  styleUrl: './historico.css'
})
export class Historico implements OnInit {
  private readonly historicoService = inject(HistoricoService);
  private readonly itensService = inject(ItensService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly registros = signal<RegistroHistorico[]>([]);
  protected readonly itens = signal<Item[]>([]);
  protected readonly temMais = signal(false);
  protected readonly carregando = signal(true);

  protected itemSelecionado = '';

  private pagina = 1;

  ngOnInit(): void {
    this.itensService.listar().subscribe({
      next: (itens) => this.itens.set(itens),
      error: () => undefined
    });

    this.buscar(true);
  }

  protected filtrar() {
    this.pagina = 1;
    this.buscar(true);
  }

  protected carregarMais() {
    this.pagina += 1;
    this.buscar(false);
  }

  private buscar(reiniciar: boolean) {
    this.carregando.set(true);

    this.historicoService
      .listar(this.pagina, TAMANHO_PAGINA, this.itemSelecionado || undefined)
      .subscribe({
        next: (resposta) => {
          this.registros.update((atuais) =>
            reiniciar ? resposta.registros : [...atuais, ...resposta.registros]
          );
          this.temMais.set(resposta.temMaisPaginas);
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.snackBar.open('Não foi possível carregar o histórico.', 'Fechar', {
            duration: 4000
          });
        }
      });
  }
}
