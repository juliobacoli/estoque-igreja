import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HistoricoPaginado } from './models';

@Injectable({ providedIn: 'root' })
export class HistoricoService {
  private readonly http = inject(HttpClient);

  listar(pagina: number, tamanhoPagina: number, itemId?: string) {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);

    if (itemId) {
      params = params.set('itemId', itemId);
    }

    return this.http.get<HistoricoPaginado>('/api/historico', { params });
  }
}
