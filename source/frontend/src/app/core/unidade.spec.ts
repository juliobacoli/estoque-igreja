import { unidadePara } from './unidade';

describe('unidadePara', () => {
  it('fica no singular com vazio, 0 e 1', () => {
    expect(unidadePara(null, 'pacote')).toBe('pacote');
    expect(unidadePara(0, 'pacote')).toBe('pacote');
    expect(unidadePara(1, 'pacote')).toBe('pacote');
  });

  it('vai para o plural acima de 1', () => {
    expect(unidadePara(2, 'pacote')).toBe('pacotes');
    expect(unidadePara(8, 'unidade')).toBe('unidades');
    expect(unidadePara(3, 'lata')).toBe('latas');
    expect(unidadePara(4, 'garrafa')).toBe('garrafas');
  });

  it('kg e unidades fora da lista não mudam', () => {
    expect(unidadePara(50, 'kg')).toBe('kg');
    expect(unidadePara(5, 'barra')).toBe('barra');
  });
});
