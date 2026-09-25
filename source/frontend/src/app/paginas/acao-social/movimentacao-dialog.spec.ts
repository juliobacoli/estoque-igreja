import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MovimentacaoDados, MovimentacaoDialog } from './movimentacao-dialog';

describe('MovimentacaoDialog', () => {
  const fechar = vi.fn();

  function criar(tipo: MovimentacaoDados['tipo'], unidade = 'kg') {
    fechar.mockReset();

    TestBed.configureTestingModule({
      imports: [MovimentacaoDialog],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: { close: fechar } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: { tipo, item: { id: '1', nome: 'Arroz', unidade, estoqueAtual: 10 } }
        }
      ]
    });

    const fixture = TestBed.createComponent(MovimentacaoDialog);
    fixture.detectChanges();
    return fixture;
  }

  const campo = (fixture: ReturnType<typeof criar>, nome: string) =>
    fixture.nativeElement.querySelector(`input[name="${nome}"]`);

  it('doação em kg pede pacotes e kg de cada um e devolve o total', () => {
    const fixture = criar('entrada');
    const componente = fixture.componentInstance;

    expect(fixture.nativeElement.textContent).toContain('Registrar doação');
    expect(campo(fixture, 'pacotes')).not.toBeNull();
    expect(campo(fixture, 'kgPorPacote')).not.toBeNull();
    expect(campo(fixture, 'quantidade')).toBeNull();

    componente['pacotes'] = 2;
    componente['kgPorPacote'] = 5;
    componente['confirmar']();

    expect(fechar).toHaveBeenCalledWith({ quantidade: 10, texto: '' });
  });

  it('doação de item fora de kg pede só a quantidade', () => {
    const fixture = criar('entrada', 'pacote');
    const componente = fixture.componentInstance;

    expect(campo(fixture, 'pacotes')).toBeNull();
    expect(campo(fixture, 'quantidade')).not.toBeNull();

    componente['quantidade'] = 8;
    componente['confirmar']();

    expect(fechar).toHaveBeenCalledWith({ quantidade: 8, texto: '' });
  });

  it('doação não tem campo de doador', () => {
    const fixture = criar('entrada', 'pacote');

    expect(campo(fixture, 'texto')).toBeNull();
    expect(fixture.nativeElement.textContent).not.toContain('Quem doou');
  });

  it('ajuste em kg usa a quantidade total, começa com a atual e pede motivo', () => {
    const fixture = criar('ajuste');

    expect(fixture.nativeElement.textContent).toContain('Ajustar estoque');
    expect(campo(fixture, 'pacotes')).toBeNull();
    expect(campo(fixture, 'texto')).not.toBeNull();
    expect(fixture.componentInstance['quantidade']).toBe(10);
  });

  it('doação em kg sem pacotes ou sem peso não fecha', () => {
    const fixture = criar('entrada');
    const componente = fixture.componentInstance;

    componente['confirmar']();
    componente['pacotes'] = 2;
    componente['confirmar']();

    expect(fechar).not.toHaveBeenCalled();
  });

  it('sem quantidade não fecha', () => {
    const fixture = criar('entrada', 'pacote');

    fixture.componentInstance['confirmar']();

    expect(fechar).not.toHaveBeenCalled();
  });
});
