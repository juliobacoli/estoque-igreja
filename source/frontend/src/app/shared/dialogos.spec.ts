import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DicaSenhaDialog } from '../paginas/login/dica-senha-dialog';
import { ConfirmacaoDados, ConfirmacaoDialog } from './confirmacao-dialog';

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('Diálogos', () => {
  const dialogRef = { close: vi.fn() };

  beforeEach(() => dialogRef.close.mockReset());

  describe('ConfirmacaoDialog', () => {
    const dados: ConfirmacaoDados = {
      titulo: 'Remover item',
      mensagem: '"Sabão" sai da lista de estoque.',
      confirmar: 'Remover'
    };

    function criar() {
      TestBed.configureTestingModule({
        imports: [ConfirmacaoDialog],
        providers: [
          { provide: MAT_DIALOG_DATA, useValue: dados },
          { provide: MatDialogRef, useValue: dialogRef }
        ]
      });

      const fixture = TestBed.createComponent(ConfirmacaoDialog);
      fixture.detectChanges();
      return fixture.nativeElement as HTMLElement;
    }

    it('mostra título, mensagem e o texto do botão de confirmar', () => {
      const el = criar();

      expect(texto(el.querySelector('h2'))).toBe('Remover item');
      expect(texto(el.querySelector('p'))).toBe('"Sabão" sai da lista de estoque.');
      expect(texto(el.querySelector('button.perigo'))).toBe('Remover');
    });

    it('"Cancelar" fecha com false', () => {
      const el = criar();

      (el.querySelectorAll('button')[0] as HTMLButtonElement).click();

      expect(dialogRef.close).toHaveBeenCalledWith(false);
    });

    it('confirmar fecha com true', () => {
      const el = criar();

      (el.querySelector('button.perigo') as HTMLButtonElement).click();

      expect(dialogRef.close).toHaveBeenCalledWith(true);
    });
  });

  describe('DicaSenhaDialog', () => {
    it('"Vou tentar de novo" fecha o diálogo', () => {
      TestBed.configureTestingModule({
        imports: [DicaSenhaDialog],
        providers: [{ provide: MatDialogRef, useValue: dialogRef }]
      });

      const fixture = TestBed.createComponent(DicaSenhaDialog);
      fixture.detectChanges();

      (fixture.nativeElement.querySelector('button.tentar') as HTMLButtonElement).click();

      expect(dialogRef.close).toHaveBeenCalledOnce();
    });
  });
});
