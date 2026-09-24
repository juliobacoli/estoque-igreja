import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { forkJoin } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { ItemSocial } from '../../core/models';

interface LinhaDoModelo {
  item: ItemSocial;
  quantidade: number | null;
}

/** Todos os itens aparecem; quem fica em branco ou zero não vai na cesta. */
@Component({
  selector: 'app-modelo-cesta',
  imports: [
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './modelo-cesta.html',
  styleUrl: './modelo-cesta.css'
})
export class ModeloCesta implements OnInit {
  private readonly acaoSocial = inject(AcaoSocialService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly linhas = signal<LinhaDoModelo[] | null>(null);
  protected readonly falhaAoCarregar = signal(false);
  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);

  ngOnInit(): void {
    forkJoin([this.acaoSocial.listar(), this.acaoSocial.obterCesta()]).subscribe({
      next: ([itens, cesta]) => {
        const naCesta = new Map(cesta.itens.map((i) => [i.itemId, i.quantidadePorCesta]));
        this.linhas.set(itens.map((item) => ({ item, quantidade: naCesta.get(item.id) ?? null })));
      },
      error: () => this.falhaAoCarregar.set(true)
    });
  }

  protected salvar() {
    const itens = (this.linhas() ?? [])
      .filter((l) => (l.quantidade ?? 0) > 0)
      .map((l) => ({ itemId: l.item.id, quantidade: l.quantidade! }));

    if (itens.length === 0) {
      this.erro.set('Escolha pelo menos um item para a cesta.');
      return;
    }

    this.erro.set(null);
    this.salvando.set(true);

    this.acaoSocial.definirModelo(itens).subscribe({
      next: () => {
        this.snackBar.open('Cesta salva.', 'Fechar', { duration: 4000 });
        this.router.navigate(['/acao-social/cesta']);
      },
      error: (resposta) => {
        this.erro.set(resposta.error?.error ?? 'Não foi possível salvar a cesta.');
        this.salvando.set(false);
      }
    });
  }
}
