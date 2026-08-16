import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExportService } from '../../core/export.service';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  // MatSnackBarModule é obrigatório: o MatSnackBar não é providedIn root,
  // quem o registra é o módulo.
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly exportService = inject(ExportService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  protected readonly itens = signal<Item[]>([]);
  protected readonly carregando = signal(true);
  protected readonly falhaAoCarregar = signal(false);
  protected readonly exportando = signal(false);

  ngOnInit(): void {
    this.carregar();
  }

  /**
   * Busca sempre do servidor ao entrar na tela — o critério de aceite 2.4 exige
   * que a quantidade reflita o banco logo após uma atualização.
   */
  private carregar() {
    this.carregando.set(true);

    this.itensService.listar().subscribe({
      next: (itens) => {
        this.itens.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.falhaAoCarregar.set(true);
        this.carregando.set(false);
        this.avisar('Não foi possível carregar os itens.');
      }
    });
  }

  protected irParaAtualizar() {
    this.router.navigate(['/atualizar']);
  }

  protected async exportarPdf() {
    this.exportando.set(true);

    try {
      const resultado = await this.exportService.gerarECompartilhar();

      if (resultado === 'baixado') {
        this.avisar('PDF baixado — este aparelho não permite compartilhar arquivos.');
      }
    } catch {
      this.avisar('Não foi possível gerar o PDF.');
    } finally {
      this.exportando.set(false);
    }
  }

  private avisar(mensagem: string) {
    this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
  }
}
