import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, of, throwError } from 'rxjs';
import { ItensService } from '../../core/itens.service';
import { Item } from '../../core/models';
import { ConfirmacaoDialog } from '../../shared/confirmacao-dialog';
import { CadastroItem } from './cadastro-item';

const ITENS: Item[] = [
  { id: '1', nome: 'Sabão', unidade: 'unidade', estoqueAtual: 3 },
  { id: '2', nome: 'Esponja', unidade: 'pacote', estoqueAtual: 0 }
];

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('CadastroItem', () => {
  const itensService = { listar: vi.fn(), criar: vi.fn(), remover: vi.fn() };
  let fixture: ComponentFixture<CadastroItem>;
  let el: HTMLElement;
  let avisar: ReturnType<typeof vi.spyOn>;
  let abrirDialog: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    itensService.listar.mockReset().mockReturnValue(of(ITENS));
    itensService.criar.mockReset();
    itensService.remover.mockReset();

    TestBed.configureTestingModule({
      imports: [CadastroItem],
      providers: [provideNoopAnimations(), { provide: ItensService, useValue: itensService }]
    });

    fixture = TestBed.createComponent(CadastroItem);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    abrirDialog = vi.spyOn(fixture.debugElement.injector.get(MatDialog), 'open');
    fixture.detectChanges();
  });

  const componente = () => fixture.componentInstance;

  function cadastrar(nome: string, unidade: string) {
    componente()['nome'] = nome;
    componente()['unidade'] = unidade;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  function confirmacaoFechandoCom(valor: boolean | undefined) {
    abrirDialog.mockReturnValue({ afterClosed: () => of(valor) } as unknown as MatDialogRef<unknown>);
  }

  async function remover(item: Item) {
    await componente()['remover'](item);
    fixture.detectChanges();
  }

  it('lista os itens com nome, unidade e estoque', () => {
    const linhas = el.querySelectorAll('.linha');

    expect(linhas).toHaveLength(2);
    expect(texto(linhas[0].querySelector('.linha__nome'))).toBe('Sabão');
    expect(texto(linhas[0].querySelector('.linha__unidade'))).toBe('unidade · 3 em estoque');
  });

  it('oferece exatamente as unidades rolo, pacote e unidade', () => {
    expect(componente()['unidades']).toEqual(['rolo', 'pacote', 'unidade']);
  });

  it('cadastro com sucesso avisa, limpa o formulário e recarrega a lista', () => {
    itensService.criar.mockReturnValue(of({ id: '3', nome: 'Rodo', unidade: 'unidade', estoqueAtual: 0 }));

    cadastrar('Rodo', 'unidade');

    expect(itensService.criar).toHaveBeenCalledWith('Rodo', 'unidade');
    expect(avisar).toHaveBeenCalledWith('"Rodo" cadastrado.', 'Fechar', { duration: 4000 });
    expect(componente()['nome']).toBe('');
    expect(componente()['unidade']).toBe('');
    expect(itensService.listar).toHaveBeenCalledTimes(2);
  });

  it('cadastro com erro mostra a mensagem da API e sacode o formulário', () => {
    itensService.criar.mockReturnValue(throwError(() => ({ error: { error: 'Já existe um item com esse nome' } })));

    cadastrar('Sabão', 'unidade');

    expect(texto(el.querySelector('.erro'))).toBe('Já existe um item com esse nome');
    expect(componente()['shakeState']()).toBe('shake');
    expect((el.querySelector('button.cadastrar') as HTMLButtonElement).disabled).toBe(false);
  });

  it('cadastro com erro sem mensagem mostra a mensagem padrão', () => {
    itensService.criar.mockReturnValue(throwError(() => ({ status: 500 })));

    cadastrar('Sabão', 'unidade');

    expect(texto(el.querySelector('.erro'))).toBe('Não foi possível cadastrar o item.');
  });

  it.each([false, undefined])('remover com confirmação fechada (%s) não chama a API', async (fechamento) => {
    confirmacaoFechandoCom(fechamento);

    await remover(ITENS[0]);

    expect(abrirDialog.mock.calls[0][0]).toBe(ConfirmacaoDialog);
    expect(itensService.remover).not.toHaveBeenCalled();
  });

  it.each([
    [true, 'Item excluído.'],
    [false, 'Item removido da lista — o histórico foi mantido.']
  ])('remoção confirmada com removido=%s avisa e recarrega a lista', async (removido, mensagem) => {
    confirmacaoFechandoCom(true);
    itensService.remover.mockReturnValue(of({ removido }));

    await remover(ITENS[0]);

    expect(itensService.remover).toHaveBeenCalledWith('1');
    expect(avisar).toHaveBeenCalledWith(mensagem, 'Fechar', { duration: 4000 });
    expect(itensService.listar).toHaveBeenCalledTimes(2);
  });

  it('durante a remoção mostra o spinner no item e desabilita todos os botões de remover', async () => {
    confirmacaoFechandoCom(true);
    itensService.remover.mockReturnValue(new Subject());

    await remover(ITENS[0]);

    const botoes = Array.from(el.querySelectorAll('button.remover')) as HTMLButtonElement[];
    expect(botoes.every((b) => b.disabled)).toBe(true);
    expect(botoes[0].querySelector('mat-progress-spinner')).not.toBeNull();
    expect(botoes[1].querySelector('mat-progress-spinner')).toBeNull();
  });

  it('erro na remoção avisa e libera os botões', async () => {
    confirmacaoFechandoCom(true);
    itensService.remover.mockReturnValue(throwError(() => ({ error: { error: 'Item não encontrado' } })));

    await remover(ITENS[0]);

    expect(avisar).toHaveBeenCalledWith('Item não encontrado', 'Fechar', { duration: 4000 });
    const botoes = Array.from(el.querySelectorAll('button.remover')) as HTMLButtonElement[];
    expect(botoes.some((b) => b.disabled)).toBe(false);
  });
});
