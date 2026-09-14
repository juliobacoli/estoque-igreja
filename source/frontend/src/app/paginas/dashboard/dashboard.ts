import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { EstoqueService } from '../../core/estoque.service';
import { ExportService } from '../../core/export.service';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';

const UM_DIA_EM_MS = 86_400_000;

// Até 7 dias o estoque está em dia; de 8 a 14 pede atenção; acima disso está atrasado.
const LIMITE_EM_DIA = 7;
const LIMITE_ATENCAO = 14;

type NivelContagem = 'em-dia' | 'atencao' | 'atrasado' | 'sem-contagem';

interface StatusContagem {
  nivel: NivelContagem;
  icone: string;
  titulo: string;
  mensagem: string;
}

function haDias(dias: number) {
  if (dias <= 0) {
    return 'hoje';
  }

  return `há ${dias} ${dias === 1 ? 'dia' : 'dias'}`;
}

/**
 * Conta dias de calendário no fuso do aparelho, e não 24h corridas: uma contagem
 * às 23h vista à 1h do dia seguinte já é "há 1 dia".
 */
function diasDesde(dataIso: string) {
  const inicioDoDia = (data: Date) =>
    new Date(data.getFullYear(), data.getMonth(), data.getDate()).getTime();

  // Math.round absorve os dias de 23h/25h da troca de horário de verão.
  return Math.round((inicioDoDia(new Date()) - inicioDoDia(new Date(dataIso))) / UM_DIA_EM_MS);
}

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  private readonly itensService = inject(ItensService);
  private readonly exportService = inject(ExportService);
  private readonly estoqueService = inject(EstoqueService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  protected readonly itens = signal<Item[]>([]);
  protected readonly carregando = signal(true);
  protected readonly falhaAoCarregar = signal(false);
  protected readonly exportando = signal(false);

  // undefined enquanto carrega ou se a busca falhar: nesses casos a linha não aparece.
  private readonly ultimaAtualizacao = signal<string | null | undefined>(undefined);

  protected readonly statusContagem = computed<StatusContagem | null>(() => {
    const data = this.ultimaAtualizacao();

    if (data === undefined) {
      return null;
    }

    if (data === null) {
      return {
        nivel: 'sem-contagem',
        icone: 'inventory_2',
        titulo: 'Nenhuma contagem registrada',
        mensagem: 'Faça a primeira contagem para acompanhar o estoque da igreja.'
      };
    }

    const dias = diasDesde(data);
    const dataCurta = new Date(data).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });

    if (dias <= LIMITE_EM_DIA) {
      return {
        nivel: 'em-dia',
        icone: 'check_circle',
        titulo: 'Estoque em dia',
        mensagem: dias <= 0 ? 'Contado hoje.' : `Última contagem ${haDias(dias)} (${dataCurta}).`
      };
    }

    if (dias <= LIMITE_ATENCAO) {
      return {
        nivel: 'atencao',
        icone: 'schedule',
        titulo: `Estoque sem contagem ${haDias(dias)}`,
        mensagem: `Última contagem em ${dataCurta}. Quem estiver na igreja pode dar uma passada no depósito?`
      };
    }

    return {
      nivel: 'atrasado',
      icone: 'warning',
      titulo: `Estoque sem contagem ${haDias(dias)}`,
      mensagem: `Última contagem em ${dataCurta}. As quantidades abaixo podem estar desatualizadas.`
    };
  });

  ngOnInit(): void {
    this.carregar();
    this.carregarUltimaAtualizacao();
  }

  private carregarUltimaAtualizacao() {
    this.estoqueService.ultimaAtualizacao().subscribe({
      next: (resposta) => this.ultimaAtualizacao.set(resposta.data),
      error: () => undefined
    });
  }

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
