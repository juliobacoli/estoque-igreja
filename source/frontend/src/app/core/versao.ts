// Preenchida no build da imagem Docker com o hash do commit (--define). Fora
// dele (ng serve, testes) fica "dev".
declare const VERSAO_APP: string | undefined;

export const versaoApp = typeof VERSAO_APP === 'undefined' ? 'dev' : VERSAO_APP;
