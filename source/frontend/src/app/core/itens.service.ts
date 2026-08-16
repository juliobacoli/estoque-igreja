import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { EstoqueAtualizado, Item, ItemRemovido } from './models';

@Injectable({ providedIn: 'root' })
export class ItensService {
  private readonly http = inject(HttpClient);

  /**
   * Por padrão só itens ativos. O filtro do Histórico pede os inativos também,
   * senão o histórico de um item removido fica inalcançável.
   */
  listar(incluirInativos = false) {
    const params = incluirInativos ? new HttpParams().set('incluirInativos', true) : undefined;

    return this.http.get<Item[]>('/api/itens', { params });
  }

  criar(nome: string, unidade: string) {
    return this.http.post<Item>('/api/itens', { nome, unidade });
  }

  atualizarEstoque(id: string, novaQuantidade: number) {
    return this.http.put<EstoqueAtualizado>(`/api/itens/${id}/estoque`, { novaQuantidade });
  }

  remover(id: string) {
    return this.http.delete<ItemRemovido>(`/api/itens/${id}`);
  }
}
