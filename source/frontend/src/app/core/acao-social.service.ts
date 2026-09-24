import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EstoqueSocialAlterado, ItemRemovido, ItemSocial } from './models';

const API = '/api/acao-social/itens';

@Injectable({ providedIn: 'root' })
export class AcaoSocialService {
  private readonly http = inject(HttpClient);

  listar() {
    return this.http.get<ItemSocial[]>(API);
  }

  criar(nome: string, unidade: string) {
    return this.http.post<ItemSocial>(API, { nome, unidade });
  }

  remover(id: string) {
    return this.http.delete<ItemRemovido>(`${API}/${id}`);
  }

  registrarEntrada(id: string, quantidade: number, doador: string | null) {
    return this.http.post<EstoqueSocialAlterado>(`${API}/${id}/entradas`, { quantidade, doador });
  }

  ajustar(id: string, novaQuantidade: number, motivo: string) {
    return this.http.post<EstoqueSocialAlterado>(`${API}/${id}/ajustes`, { novaQuantidade, motivo });
  }
}
