import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, of, throwError } from 'rxjs';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';
import { AtualizarEstoque } from './atualizar-estoque';

const ITENS: Item[] = [
  { id: '1', nome: 'Sabão', unidade: 'unidade', estoqueAtual: 3 },
  { id: '2', nome: 'Esponja', unidade: 'pacote', estoqueAtual: 0 }
];

describe('AtualizarEstoque', () => {
  const itensService = { listar: vi.fn(), atualizarEstoque: vi.fn() };
  let fixture: ComponentFixture<AtualizarEstoque>;
  let el: HTMLElement;
  let navegar: ReturnType<typeof vi.spyOn>;
  let avisar: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    itensService.listar.mockReset().mockReturnValue(of(ITENS));
    itensService.atualizarEstoque.mockReset().mockReturnValue(of({}));

    TestBed.configureTestingModule({
      imports: [AtualizarEstoque],
      providers: [provideRouter([]), { provide: ItensService, useValue: itensService }]
    });

    navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  function criar() {
    fixture = TestBed.createComponent(AtualizarEstoque);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  }

  const campo = (id: string) => el.querySelector(`#qtd-${id}`) as HTMLInputElement;
  const botao = (rotulo: string) => el.querySelector(`button[aria-label="${rotulo}"]`) as HTMLButtonElement;
  const botaoSalvar = () => el.querySelector('button.salvar') as HTMLButtonElement;

  function clicar(rotulo: string) {
    botao(rotulo).click();
    fixture.detectChanges();
  }

  function digitar(id: string, valor: string) {
    campo(id).value = valor;
    campo(id).dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function salvar() {
    botaoSalvar().click();
    fixture.detectChanges();
  }

  it('carrega os itens com a quantidade inicial igual ao estoque atual', () => {
    criar();

    expect(campo('1').value).toBe('3');
    expect(campo('2').value).toBe('0');
  });

  it('erro ao carregar mostra a mensagem e avisa', () => {
    itensService.listar.mockReturnValue(throwError(() => new Error('falhou')));

    criar();

    expect(el.querySelector('.erro')?.textContent?.trim()).toBe('Não foi possível carregar os itens.');
    expect(avisar).toHaveBeenCalledWith('Não foi possível carregar os itens.', 'Fechar', { duration: 4000 });
  });

  it('lista vazia mostra a mensagem própria', () => {
    itensService.listar.mockReturnValue(of([]));

    criar();

    expect(el.textContent).toContain('Nenhum item cadastrado ainda.');
  });

  it('+ soma e − subtrai uma unidade; − fica desabilitado em zero', () => {
    criar();

    clicar('Aumentar Sabão');
    expect(campo('1').value).toBe('4');

    clicar('Diminuir Sabão');
    expect(campo('1').value).toBe('3');

    expect(botao('Diminuir Esponja').disabled).toBe(true);
  });

  it('valor negativo digitado vira zero', () => {
    criar();

    digitar('1', '-5');

    expect(campo('1').value).toBe('0');
  });

  it('valor não numérico mantém a quantidade anterior', () => {
    criar();

    fixture.componentInstance['alterar']('1', 'abc');
    fixture.detectChanges();

    expect(campo('1').value).toBe('3');
  });

  it('salvar só fica habilitado quando há alteração', () => {
    criar();
    expect(botaoSalvar().disabled).toBe(true);

    clicar('Aumentar Sabão');

    expect(botaoSalvar().disabled).toBe(false);
  });

  it('salva só os itens alterados, com as quantidades certas', () => {
    criar();
    clicar('Aumentar Sabão');

    salvar();

    expect(itensService.atualizarEstoque).toHaveBeenCalledOnce();
    expect(itensService.atualizarEstoque).toHaveBeenCalledWith('1', 4);
  });

  it('salvar um item avisa "Contagem salva." e volta para o dashboard', () => {
    criar();
    clicar('Aumentar Sabão');

    salvar();

    expect(avisar).toHaveBeenCalledWith('Contagem salva.', 'Fechar', { duration: 4000 });
    expect(navegar).toHaveBeenCalledWith(['/dashboard']);
  });

  it('salvar vários itens avisa quantos foram atualizados', () => {
    criar();
    clicar('Aumentar Sabão');
    clicar('Aumentar Esponja');

    salvar();

    expect(itensService.atualizarEstoque).toHaveBeenCalledTimes(2);
    expect(avisar).toHaveBeenCalledWith('2 itens atualizados.', 'Fechar', { duration: 4000 });
  });

  it('com alteração pendente o guard pergunta; depois de salvar, não', () => {
    criar();
    clicar('Aumentar Sabão');
    expect(fixture.componentInstance.temAlteracaoPendente()).toBe(true);

    salvar();

    expect(fixture.componentInstance.temAlteracaoPendente()).toBe(false);
  });

  it('erro 404 avisa que um item foi removido e recarrega a lista', () => {
    itensService.atualizarEstoque.mockReturnValue(throwError(() => ({ status: 404 })));
    criar();
    clicar('Aumentar Sabão');

    salvar();

    expect(avisar).toHaveBeenCalledWith(
      'Um item foi removido por um administrador. A lista foi atualizada.',
      'Fechar',
      { duration: 4000 }
    );
    expect(itensService.listar).toHaveBeenCalledTimes(2);
    expect(navegar).not.toHaveBeenCalled();
  });

  it.each([
    ['com mensagem da API', { status: 400, error: { error: 'Mensagem da API' } }, 'Mensagem da API'],
    ['sem mensagem da API', { status: 500 }, 'Não foi possível salvar a contagem.']
  ])('outro erro %s avisa e libera o botão', (_, erro, mensagem) => {
    itensService.atualizarEstoque.mockReturnValue(throwError(() => erro));
    criar();
    clicar('Aumentar Sabão');

    salvar();

    expect(avisar).toHaveBeenCalledWith(mensagem, 'Fechar', { duration: 4000 });
    expect(botaoSalvar().disabled).toBe(false);
    expect(navegar).not.toHaveBeenCalled();
  });

  it('durante o salvamento desabilita contadores e botão', () => {
    itensService.atualizarEstoque.mockReturnValue(new Subject());
    criar();
    clicar('Aumentar Sabão');

    salvar();

    expect(botaoSalvar().disabled).toBe(true);
    expect(botaoSalvar().querySelector('mat-progress-spinner')).not.toBeNull();
    expect(botao('Aumentar Sabão').disabled).toBe(true);
    expect(campo('1').disabled).toBe(true);
  });
});
