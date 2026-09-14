import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, of, throwError } from 'rxjs';
import { HistoricoService } from '../../core/historico.service';
import { ItensService } from '../../core/itens.service';
import { HistoricoPaginado, Item, RegistroHistorico } from '../../core/models';
import { Historico } from './historico';

const ITENS: Item[] = [
  { id: '1', nome: 'Sabão', unidade: 'unidade', estoqueAtual: 5 },
  { id: '2', nome: 'Esponja', unidade: 'pacote', estoqueAtual: 0 }
];

// Data criada no fuso local, para o formato exibido não depender do fuso da máquina.
const registro = (itemNome: string, anterior: number, nova: number): RegistroHistorico => ({
  itemNome,
  quantidadeAnterior: anterior,
  quantidadeNova: nova,
  perfil: 'Voluntario',
  data: new Date(2026, 8, 10, 13, 30).toISOString()
});

const pagina = (registros: RegistroHistorico[], temMaisPaginas = false): HistoricoPaginado => ({
  registros,
  temMaisPaginas
});

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('Historico', () => {
  const historicoService = { listar: vi.fn() };
  const itensService = { listar: vi.fn() };
  let fixture: ComponentFixture<Historico>;
  let el: HTMLElement;
  let avisar: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    historicoService.listar.mockReset().mockReturnValue(of(pagina([registro('Sabão', 3, 5)])));
    itensService.listar.mockReset().mockReturnValue(of(ITENS));

    TestBed.configureTestingModule({
      imports: [Historico],
      providers: [
        { provide: HistoricoService, useValue: historicoService },
        { provide: ItensService, useValue: itensService }
      ]
    });
  });

  function criar() {
    fixture = TestBed.createComponent(Historico);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  }

  const registrosNaTela = () => Array.from(el.querySelectorAll('.registro__item')).map(texto);
  const botaoCarregarMais = () => el.querySelector('button.carregar-mais') as HTMLButtonElement | null;

  it('ao abrir carrega os itens do filtro (com inativos) e a primeira página', () => {
    criar();

    expect(itensService.listar).toHaveBeenCalledWith(true);
    expect(historicoService.listar).toHaveBeenCalledWith(1, 20, undefined);
  });

  it('mostra item, quantidade anterior → nova, perfil e data', () => {
    criar();

    expect(texto(el.querySelector('.registro__item'))).toBe('Sabão');
    expect(texto(el.querySelector('.registro__qtd'))).toBe('3 → 5');
    expect(texto(el.querySelector('.registro__meta'))).toBe('Voluntario · 10/09/2026 13:30');
  });

  it('sem registros mostra a mensagem própria', () => {
    historicoService.listar.mockReturnValue(of(pagina([])));

    criar();

    expect(el.textContent).toContain('Nenhuma atualização registrada.');
  });

  it('trocar o filtro volta para a página 1 e substitui a lista', () => {
    historicoService.listar
      .mockReturnValueOnce(of(pagina([registro('Sabão', 0, 1), registro('Esponja', 0, 2)], true)))
      .mockReturnValueOnce(of(pagina([registro('Esponja', 0, 2)])));
    criar();

    fixture.componentInstance['itemSelecionado'] = '2';
    fixture.componentInstance['filtrar']();
    fixture.detectChanges();

    expect(historicoService.listar).toHaveBeenLastCalledWith(1, 20, '2');
    expect(registrosNaTela()).toEqual(['Esponja']);
  });

  it('"Carregar mais" pede a próxima página e acrescenta à lista', () => {
    historicoService.listar
      .mockReturnValueOnce(of(pagina([registro('Sabão', 0, 1)], true)))
      .mockReturnValueOnce(of(pagina([registro('Esponja', 0, 2)])));
    criar();

    botaoCarregarMais()!.click();
    fixture.detectChanges();

    expect(historicoService.listar).toHaveBeenLastCalledWith(2, 20, undefined);
    expect(registrosNaTela()).toEqual(['Sabão', 'Esponja']);
  });

  it('"Carregar mais" só aparece quando há mais páginas', () => {
    criar();

    expect(botaoCarregarMais()).toBeNull();
  });

  it('"Carregar mais" some enquanto a próxima página carrega', () => {
    const proxima$ = new Subject<HistoricoPaginado>();
    historicoService.listar.mockReturnValueOnce(of(pagina([registro('Sabão', 0, 1)], true))).mockReturnValueOnce(proxima$);
    criar();

    botaoCarregarMais()!.click();
    fixture.detectChanges();

    expect(botaoCarregarMais()).toBeNull();
    expect(el.querySelector('.centro mat-progress-spinner')).not.toBeNull();
  });

  it('erro ao carregar o histórico avisa', () => {
    historicoService.listar.mockReturnValue(throwError(() => new Error('falhou')));

    criar();

    expect(avisar).toHaveBeenCalledWith('Não foi possível carregar o histórico.', 'Fechar', { duration: 4000 });
  });

  it('falha ao carregar os itens do filtro não quebra a tela', () => {
    itensService.listar.mockReturnValue(throwError(() => new Error('falhou')));

    criar();

    expect(registrosNaTela()).toEqual(['Sabão']);
    expect(avisar).not.toHaveBeenCalled();
  });
});
