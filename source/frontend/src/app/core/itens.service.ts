import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EstoqueAtualizado, Item } from './models';

@Injectable({ providedIn: 'root' })
export class ItensService {
  private readonly http = inject(HttpClient);

  listar() {
    return this.http.get<Item[]>('/api/itens');
  }

  criar(nome: string, unidade: string) {
    return this.http.post<Item>('/api/itens', { nome, unidade });
  }

  atualizarEstoque(id: string, novaQuantidade: number) {
    return this.http.put<EstoqueAtualizado>(`/api/itens/${id}/estoque`, { novaQuantidade });
  }
}
