import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CanDeactivateFn } from '@angular/router';
import { map } from 'rxjs';
import { ConfirmacaoDados, ConfirmacaoDialog } from '../shared/confirmacao-dialog';

/** Telas que podem segurar a saída implementam isto. */
export interface PodeTerAlteracoes {
  temAlteracaoPendente(): boolean;
}

/**
 * Pergunta antes de sair de uma tela com edição pendente.
 *
 * Vive num arquivo próprio, sem importar componente nenhum: se o guard viesse
 * junto da tela, o `app.routes.ts` teria que importá-la no topo e o
 * `loadComponent` deixaria de ser lazy.
 */
export const alteracoesNaoSalvasGuard: CanDeactivateFn<PodeTerAlteracoes> = (componente) => {
  if (!componente.temAlteracaoPendente()) {
    return true;
  }

  const dados: ConfirmacaoDados = {
    titulo: 'Sair sem salvar?',
    mensagem:
      'Você alterou quantidades que ainda não foram salvas. Ao sair desta tela, a contagem será perdida.',
    confirmar: 'Sair sem salvar'
  };

  // Fechar no ESC ou clicando fora devolve undefined — que vira false e mantém
  // o usuário na tela. O silêncio nunca descarta a contagem.
  return inject(MatDialog)
    .open(ConfirmacaoDialog, { data: dados })
    .afterClosed()
    .pipe(map(Boolean));
};
