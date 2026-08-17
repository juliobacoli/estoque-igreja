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

export interface ItemRemovido {
  removido: boolean;
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

export interface ErroApi {
  error: string;
}
