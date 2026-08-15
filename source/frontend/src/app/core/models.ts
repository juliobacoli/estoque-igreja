export type Perfil = 'Admin' | 'Voluntario';

export interface Item {
  id: string;
  nome: string;
  unidade: string;
  estoqueAtual: number;
}

export interface EstoqueAtualizado {
  itemId: string;
  quantidadeAnterior: number;
  quantidadeNova: number;
}

export interface RegistroHistorico {
  itemNome: string;
  quantidadeAnterior: number;
  quantidadeNova: number;
  perfil: Perfil;
  data: string;
}

export interface HistoricoPaginado {
  registros: RegistroHistorico[];
  temMaisPaginas: boolean;
}

/** Formato de erro usado por toda a API (Capítulo 3, item 3.5). */
export interface ErroApi {
  error: string;
}
