import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { ModeloCesta } from './modelo-cesta';

describe('ModeloCesta', () => {
  const servico = { listar: vi.fn(), obterCesta: vi.fn(), definirModelo: vi.fn() };
  let fixture: ComponentFixture<ModeloCesta>;
  let navegar: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    servico.listar.mockReset().mockReturnValue(
      of([
        { id: 'a', nome: 'Arroz', unidade: 'kg', estoqueAtual: 10 },
        { id: 'f', nome: 'Feijão', unidade: 'kg', estoqueAtual: 4 }
      ])
    );
    servico.obterCesta.mockReset().mockReturnValue(
      of({
        cestasProntas: 0,
        podeMontar: 2,
        itens: [{ itemId: 'a', nome: 'Arroz', unidade: 'kg', quantidadePorCesta: 5, estoqueAtual: 10 }],
        faltasParaProxima: []
      })
    );
    servico.definirModelo.mockReset();

    TestBed.configureTestingModule({
      imports: [ModeloCesta],
      providers: [provideRouter([]), provideNoopAnimations(), { provide: AcaoSocialService, useValue: servico }]
    });

    navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(ModeloCesta);
    vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  });

  const linhas = () => fixture.componentInstance['linhas']()!;

  function salvar() {
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('lista todos os itens e preenche a quantidade dos que já estão na cesta', () => {
    expect(linhas().map((l) => [l.item.nome, l.quantidade])).toEqual([
      ['Arroz', 5],
      ['Feijão', null]
    ]);
  });

  it('salvar envia só os itens com quantidade e volta para as cestas', () => {
    servico.definirModelo.mockReturnValue(of(undefined));
    linhas()[1].quantidade = 2;

    salvar();

    expect(servico.definirModelo).toHaveBeenCalledWith([
      { itemId: 'a', quantidade: 5 },
      { itemId: 'f', quantidade: 2 }
    ]);
    expect(navegar).toHaveBeenCalledWith(['/acao-social/cesta']);
  });

  it('sem nenhum item com quantidade, avisa e não chama a API', () => {
    linhas()[0].quantidade = 0;

    salvar();

    expect(servico.definirModelo).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('.erro').textContent.trim()).toBe(
      'Escolha pelo menos um item para a cesta.'
    );
  });

  it('erro da API aparece na tela', () => {
    servico.definirModelo.mockReturnValue(throwError(() => ({ error: { error: 'Item não encontrado' } })));

    salvar();

    expect(fixture.nativeElement.querySelector('.erro').textContent.trim()).toBe('Item não encontrado');
    expect(navegar).not.toHaveBeenCalled();
  });
});
