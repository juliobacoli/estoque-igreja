export type Perfil = 'Admin';

export type Modulo = 'Obreiros' | 'AcaoSocial';

export interface Sessao {
  perfil: Perfil;
  login: string;
  modulos: Modulo[];
}

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

export interface UltimaAtualizacao {
  data: string | null;
}

export interface ErroApi {
  error: string;
}

export interface ItemSocial {
  id: string;
  nome: string;
  unidade: string;
  estoqueAtual: number;
}

export interface EstoqueSocialAlterado {
  itemId: string;
  quantidadeAnterior: number;
  quantidadeNova: number;
}
