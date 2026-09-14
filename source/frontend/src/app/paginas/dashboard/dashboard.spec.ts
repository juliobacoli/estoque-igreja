import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, of, throwError } from 'rxjs';
import { EstoqueService } from '../../core/estoque.service';
import { ExportService } from '../../core/export.service';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';
import { Dashboard } from './dashboard';

const ITENS: Item[] = [
  { id: '1', nome: 'Sabão', unidade: 'unidade', estoqueAtual: 5 },
  { id: '2', nome: 'Esponja', unidade: 'pacote', estoqueAtual: 2 }
];

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('Dashboard', () => {
  const itensService = { listar: vi.fn() };
  const exportService = { gerarECompartilhar: vi.fn() };
  const estoqueService = { ultimaAtualizacao: vi.fn() };
  let fixture: ComponentFixture<Dashboard>;
  let el: HTMLElement;
  let navegar: ReturnType<typeof vi.spyOn>;
  let avisar: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    itensService.listar.mockReset().mockReturnValue(of(ITENS));
    exportService.gerarECompartilhar.mockReset();
    estoqueService.ultimaAtualizacao.mockReset().mockReturnValue(of({ data: null }));

    TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideRouter([]),
        { provide: ItensService, useValue: itensService },
        { provide: ExportService, useValue: exportService },
        { provide: EstoqueService, useValue: estoqueService }
      ]
    });

    navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  afterEach(() => vi.useRealTimers());

  function criar() {
    fixture = TestBed.createComponent(Dashboard);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  }

  describe('lista de itens', () => {
    it('mostra o spinner enquanto carrega e depois a lista', () => {
      const itens$ = new Subject<Item[]>();
      itensService.listar.mockReturnValue(itens$);

      criar();
      expect(el.querySelector('.centro mat-progress-spinner')).not.toBeNull();

      itens$.next(ITENS);
      fixture.detectChanges();

      const linhas = el.querySelectorAll('.linha');
      expect(el.querySelector('.centro')).toBeNull();
      expect(linhas).toHaveLength(2);
      expect(texto(linhas[0].querySelector('.linha__nome'))).toBe('Sabão');
      expect(texto(linhas[0].querySelector('.linha__qtd'))).toBe('5 unidade');
    });

    it('erro ao carregar mostra a mensagem e avisa', () => {
      itensService.listar.mockReturnValue(throwError(() => new Error('falhou')));

      criar();

      expect(texto(el.querySelector('.erro'))).toBe('Não foi possível carregar os itens.');
      expect(avisar).toHaveBeenCalledWith('Não foi possível carregar os itens.', 'Fechar', { duration: 4000 });
    });

    it('lista vazia mostra a mensagem própria', () => {
      itensService.listar.mockReturnValue(of([]));

      criar();

      expect(el.textContent).toContain('Nenhum item cadastrado ainda.');
    });

    it('"Atualizar estoque" vai para a tela de contagem', () => {
      criar();

      (el.querySelector('.acao--principal') as HTMLButtonElement).click();

      expect(navegar).toHaveBeenCalledWith(['/atualizar']);
    });
  });

  describe('exportar PDF', () => {
    const botaoPdf = () => el.querySelectorAll('.acao')[1] as HTMLButtonElement;

    it('avisa quando o PDF foi baixado em vez de compartilhado', async () => {
      exportService.gerarECompartilhar.mockResolvedValue('baixado');
      criar();

      await fixture.componentInstance['exportarPdf']();

      expect(avisar).toHaveBeenCalledWith(
        'PDF baixado — este aparelho não permite compartilhar arquivos.',
        'Fechar',
        { duration: 4000 }
      );
    });

    it('não avisa nada quando compartilhou', async () => {
      exportService.gerarECompartilhar.mockResolvedValue('compartilhado');
      criar();

      await fixture.componentInstance['exportarPdf']();

      expect(avisar).not.toHaveBeenCalled();
    });

    it('avisa quando falha ao gerar', async () => {
      exportService.gerarECompartilhar.mockRejectedValue(new Error('falhou'));
      criar();

      await fixture.componentInstance['exportarPdf']();

      expect(avisar).toHaveBeenCalledWith('Não foi possível gerar o PDF.', 'Fechar', { duration: 4000 });
    });

    it('fica desabilitado com spinner durante a exportação e volta ao normal no fim', async () => {
      let concluir!: (valor: string) => void;
      exportService.gerarECompartilhar.mockReturnValue(new Promise((resolve) => (concluir = resolve)));
      criar();

      const exportacao = fixture.componentInstance['exportarPdf']();
      fixture.detectChanges();

      expect(botaoPdf().disabled).toBe(true);
      expect(botaoPdf().querySelector('mat-progress-spinner')).not.toBeNull();

      concluir('compartilhado');
      await exportacao;
      fixture.detectChanges();

      expect(botaoPdf().disabled).toBe(false);
    });
  });

  describe('card de status da contagem', () => {
    // Datas locais: o card conta dias de calendário no fuso do aparelho.
    const AGORA = new Date(2026, 8, 14, 10, 0);
    const diasAtras = (dias: number, hora = 10) => new Date(2026, 8, 14 - dias, hora, 0).toISOString();

    function criarComData(data: string | null, agora = AGORA) {
      vi.useFakeTimers({ toFake: ['Date'] });
      vi.setSystemTime(agora);
      estoqueService.ultimaAtualizacao.mockReturnValue(of({ data }));
      criar();
    }

    const card = () => el.querySelector('section.status[role="status"]');
    const nivel = () => card()?.getAttribute('data-nivel');
    const titulo = () => texto(el.querySelector('.status__titulo'));
    const linhas = () => Array.from(el.querySelectorAll('.status__linha')).map(texto);
    const botaoContar = () => el.querySelector('.status__acao') as HTMLButtonElement | null;

    it('nunca contado: sem linha de data, com aviso e botão', () => {
      criarComData(null);

      expect(nivel()).toBe('sem-contagem');
      expect(titulo()).toBe('Nenhuma contagem registrada');
      expect(el.querySelector('.status__destaque')).toBeNull();
      expect(linhas()).toEqual(['Faça a primeira contagem para acompanhar o estoque da igreja.']);
      expect(botaoContar()).not.toBeNull();
    });

    it('contado hoje: em dia, sem botão', () => {
      criarComData(diasAtras(0, 8));

      expect(nivel()).toBe('em-dia');
      expect(titulo()).toBe('Estoque em dia');
      expect(linhas()).toEqual(['Contado hoje.']);
      expect(texto(el.querySelector('.status__destaque'))).toBe('hoje');
      expect(botaoContar()).toBeNull();
    });

    it('há 1 dia usa o singular', () => {
      criarComData(diasAtras(1));

      expect(linhas()).toEqual(['Última contagem há 1 dia, em 13/09.']);
    });

    it.each([
      [7, 'em-dia'],
      [8, 'atencao'],
      [14, 'atencao'],
      [15, 'atrasado']
    ])('%i dias sem contagem é nível %s', (dias, esperado) => {
      criarComData(diasAtras(dias));

      expect(nivel()).toBe(esperado);
    });

    it('atenção mostra título, data em destaque, aviso e botão', () => {
      criarComData(diasAtras(10));

      expect(titulo()).toBe('Estoque sem contagem há 10 dias');
      expect(texto(el.querySelector('.status__destaque'))).toBe('04/09');
      expect(linhas()).toEqual([
        'Última contagem em 04/09.',
        'Quem estiver na igreja pode dar uma passada no depósito?'
      ]);
      expect(botaoContar()).not.toBeNull();
    });

    it('atrasado mostra título, data em destaque, aviso e botão', () => {
      criarComData(diasAtras(29));

      expect(nivel()).toBe('atrasado');
      expect(titulo()).toBe('Estoque sem contagem há 29 dias');
      expect(linhas()).toEqual(['Última contagem em 16/08.', 'As quantidades abaixo podem estar desatualizadas.']);
      expect(botaoContar()).not.toBeNull();
    });

    it('"Contar agora" vai para a tela de contagem', () => {
      criarComData(diasAtras(29));

      botaoContar()!.click();

      expect(navegar).toHaveBeenCalledWith(['/atualizar']);
    });

    it('conta dias de calendário: 23h de ontem vista à 1h de hoje é "há 1 dia"', () => {
      criarComData(new Date(2026, 8, 13, 23, 0).toISOString(), new Date(2026, 8, 14, 1, 0));

      expect(linhas()).toEqual(['Última contagem há 1 dia, em 13/09.']);
    });

    it('erro na busca esconde o card sem afetar a lista', () => {
      estoqueService.ultimaAtualizacao.mockReturnValue(throwError(() => new Error('falhou')));

      criar();

      expect(card()).toBeNull();
      expect(el.querySelectorAll('.linha')).toHaveLength(2);
    });
  });
});
