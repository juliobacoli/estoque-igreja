import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ExportService {
  private readonly http = inject(HttpClient);

  baixarPdf() {
    return this.http.get('/api/estoque/exportar-pdf', { responseType: 'blob' });
  }

  /**
   * Abre o menu nativo de compartilhamento do celular. Onde a Web Share API não
   * aceita arquivos (desktop, navegadores antigos), cai para download.
   *
   * O share() precisa acontecer perto do gesto de toque — se a geração do PDF
   * demorar, o iOS Safari pode recusar por perda de "user activation".
   */
  async compartilhar(blob: Blob) {
    const arquivo = new File([blob], 'estoque.pdf', { type: 'application/pdf' });

    if (navigator.canShare?.({ files: [arquivo] })) {
      try {
        await navigator.share({ files: [arquivo], title: 'Estoque ICPA' });
        return 'compartilhado' as const;
      } catch (erro) {
        // Cancelar o menu nativo dispara AbortError — não é falha.
        if (erro instanceof DOMException && erro.name === 'AbortError') {
          return 'cancelado' as const;
        }
      }
    }

    this.baixarArquivo(blob, 'estoque.pdf');
    return 'baixado' as const;
  }

  private baixarArquivo(blob: Blob, nome: string) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = url;
    link.download = nome;
    link.click();

    URL.revokeObjectURL(url);
  }

  async gerarECompartilhar() {
    const blob = await firstValueFrom(this.baixarPdf());
    return this.compartilhar(blob);
  }
}
