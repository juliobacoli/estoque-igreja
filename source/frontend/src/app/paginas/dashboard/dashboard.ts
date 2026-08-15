import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ExportService } from '../../core/export.service';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  imports: [],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly exportService = inject(ExportService);
  private readonly router = inject(Router);

  protected readonly itens = signal<Item[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly exportando = signal(false);
  protected readonly avisoExport = signal<string | null>(null);

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
        this.erro.set('Não foi possível carregar os itens.');
        this.carregando.set(false);
      }
    });
  }

  protected irParaAtualizar() {
    this.router.navigate(['/atualizar']);
  }

  protected async exportarPdf() {
    this.avisoExport.set(null);
    this.exportando.set(true);

    try {
      const resultado = await this.exportService.gerarECompartilhar();

      if (resultado === 'baixado') {
        this.avisoExport.set('PDF baixado — este aparelho não permite compartilhar arquivos.');
      }
    } catch {
      this.avisoExport.set('Não foi possível gerar o PDF.');
    } finally {
      this.exportando.set(false);
    }
  }
}
