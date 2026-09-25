const PLURAIS: Record<string, string> = {
  pacote: 'pacotes',
  unidade: 'unidades',
  lata: 'latas',
  garrafa: 'garrafas'
};

/**
 * Plural só acima de 1 ("0 pacote", "1 pacote", "8 pacotes"), inclusive com o
 * campo vazio. Unidade fora da lista, como kg, fica como está para não inventar
 * plural errado.
 */
export function unidadePara(quantidade: number | null, unidade: string): string {
  return quantidade !== null && quantidade > 1 ? (PLURAIS[unidade] ?? unidade) : unidade;
}
