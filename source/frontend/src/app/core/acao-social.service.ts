import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CestaResumo, CestasMontadas, EstoqueSocialAlterado, ItemRemovido, ItemSocial } from './models';

const API = '/api/acao-social/itens';
const CESTA = '/api/acao-social/cesta';

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

  obterCesta() {
    return this.http.get<CestaResumo>(CESTA);
  }

  definirModelo(itens: { itemId: string; quantidade: number }[]) {
    return this.http.put<void>(`${CESTA}/modelo`, { itens });
  }

  montar(quantidade: number) {
    return this.http.post<CestasMontadas>(`${CESTA}/montagens`, { quantidade });
  }
}
