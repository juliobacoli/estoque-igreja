import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CanDeactivateFn } from '@angular/router';
import { map } from 'rxjs';
import { ConfirmacaoDados, ConfirmacaoDialog } from '../shared/confirmacao-dialog';

export interface PodeTerAlteracoes {
  temAlteracaoPendente(): boolean;
}

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

  return inject(MatDialog)
    .open(ConfirmacaoDialog, { data: dados })
    .afterClosed()
    .pipe(map(Boolean));
};
