import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { AuthService } from '../../core/auth.service';
import { CestaResumo } from '../../core/models';
import { Cesta } from './cesta';

const RESUMO: CestaResumo = {
  cestasProntas: 3,
  podeMontar: 2,
  itens: [
    { itemId: 'a', nome: 'Arroz', unidade: 'kg', quantidadePorCesta: 5, estoqueAtual: 12 },
    { itemId: 'o', nome: 'Óleo', unidade: 'garrafa', quantidadePorCesta: 1, estoqueAtual: 7 }
  ],
  faltasParaProxima: [{ nome: 'Arroz', unidade: 'kg', tem: 12, precisa: 15 }]
};

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('Cesta', () => {
  const servico = { obterCesta: vi.fn(), montar: vi.fn() };
  let fixture: ComponentFixture<Cesta>;
  let el: HTMLElement;
  let avisar: ReturnType<typeof vi.spyOn>;

  function criar(resumo: CestaResumo = RESUMO) {
    servico.obterCesta.mockReset().mockReturnValue(of(resumo));
    servico.montar.mockReset();

    TestBed.configureTestingModule({
      imports: [Cesta],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AcaoSocialService, useValue: servico },
        { provide: AuthService, useValue: { ehAdmin: signal(true) } }
      ]
    });

    fixture = TestBed.createComponent(Cesta);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  }

  function montar(quantidade: number) {
    fixture.componentInstance['quantidade'] = quantidade;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('mostra as cestas prontas, quantas dá para montar, o que falta e a composição', () => {
    criar();

    const numeros = Array.from(el.querySelectorAll('.numero')).map((n) => [
      texto(n.querySelector('.numero__valor')),
      texto(n.querySelector('.muted'))
    ]);
    expect(numeros).toEqual([
      ['3', 'cestas prontas'],
      ['2', 'dá para montar']
    ]);
    expect(texto(el.querySelector('.faltas li'))).toBe('Arroz: 3 kg');
    expect(Array.from(el.querySelectorAll('.linha')).map(texto)).toEqual([
      'Arroz 5 kg · tem 12',
      'Óleo 1 garrafa · tem 7'
    ]);
  });

  it('sem cesta definida, mostra o aviso e o atalho para definir', () => {
    criar({ cestasProntas: 0, podeMontar: 0, itens: [], faltasParaProxima: [] });

    expect(el.textContent).toContain('A cesta ainda não foi definida.');
    expect(el.querySelector('form')).toBeNull();
    expect(texto(el.querySelector('a[href="/acao-social/cesta/modelo"]'))).toContain('Definir a cesta');
  });

  it('sem estoque para nenhuma cesta, o botão de montar fica desabilitado', () => {
    criar({ ...RESUMO, podeMontar: 0 });

    expect((el.querySelector('button.montar') as HTMLButtonElement).disabled).toBe(true);
  });

  it('montar envia a quantidade, avisa e recarrega', () => {
    criar();
    servico.montar.mockReturnValue(of({ montadas: 2, cestasProntas: 5 }));

    montar(2);

    expect(servico.montar).toHaveBeenCalledWith(2);
    expect(avisar).toHaveBeenCalledWith('2 cestas montadas. Prontas: 5.', 'Fechar', expect.anything());
    expect(servico.obterCesta).toHaveBeenCalledTimes(2);
  });

  it('falta de estoque mostra a mensagem da API', () => {
    criar();
    servico.montar.mockReturnValue(
      throwError(() => ({ error: { error: 'Não dá para montar 3 cestas. Falta: Arroz (tem 12, precisa de 15). Dá para montar até 2.' } }))
    );

    montar(3);

    expect(texto(el.querySelector('.erro'))).toBe(
      'Não dá para montar 3 cestas. Falta: Arroz (tem 12, precisa de 15). Dá para montar até 2.'
    );
  });
});
