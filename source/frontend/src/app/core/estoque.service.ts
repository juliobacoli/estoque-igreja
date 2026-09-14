import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UltimaAtualizacao } from './models';

@Injectable({ providedIn: 'root' })
export class EstoqueService {
  private readonly http = inject(HttpClient);

  ultimaAtualizacao() {
    return this.http.get<UltimaAtualizacao>('/api/estoque/ultima-atualizacao');
  }
}
