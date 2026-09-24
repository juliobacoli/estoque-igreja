import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MovimentacaoDados, MovimentacaoDialog } from './movimentacao-dialog';

describe('MovimentacaoDialog', () => {
  const fechar = vi.fn();

  function criar(tipo: MovimentacaoDados['tipo']) {
    fechar.mockReset();

    TestBed.configureTestingModule({
      imports: [MovimentacaoDialog],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: { close: fechar } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: { tipo, item: { id: '1', nome: 'Arroz', unidade: 'kg', estoqueAtual: 10 } }
        }
      ]
    });

    const fixture = TestBed.createComponent(MovimentacaoDialog);
    fixture.detectChanges();
    return fixture;
  }

  it('doação começa sem quantidade e devolve o doador sem espaços', () => {
    const fixture = criar('entrada');
    const componente = fixture.componentInstance;

    expect(fixture.nativeElement.textContent).toContain('Registrar doação');
    expect(componente['quantidade']).toBeNull();

    componente['quantidade'] = 5;
    componente['texto'] = '  Mercado ';
    componente['confirmar']();

    expect(fechar).toHaveBeenCalledWith({ quantidade: 5, texto: 'Mercado' });
  });

  it('ajuste começa com a quantidade atual', () => {
    const fixture = criar('ajuste');

    expect(fixture.nativeElement.textContent).toContain('Ajustar estoque');
    expect(fixture.componentInstance['quantidade']).toBe(10);
  });

  it('sem quantidade não fecha', () => {
    const fixture = criar('entrada');

    fixture.componentInstance['confirmar']();

    expect(fechar).not.toHaveBeenCalled();
  });
});
