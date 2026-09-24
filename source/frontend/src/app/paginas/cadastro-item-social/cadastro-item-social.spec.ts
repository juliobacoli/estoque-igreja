import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { CadastroItemSocial } from './cadastro-item-social';

describe('CadastroItemSocial', () => {
  const servico = { listar: vi.fn(), criar: vi.fn(), remover: vi.fn() };
  let fixture: ComponentFixture<CadastroItemSocial>;
  let el: HTMLElement;
  let avisar: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    servico.listar.mockReset().mockReturnValue(of([{ id: '1', nome: 'Arroz', unidade: 'kg', estoqueAtual: 4 }]));
    servico.criar.mockReset();
    servico.remover.mockReset();

    TestBed.configureTestingModule({
      imports: [CadastroItemSocial],
      providers: [provideNoopAnimations(), { provide: AcaoSocialService, useValue: servico }]
    });

    fixture = TestBed.createComponent(CadastroItemSocial);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    fixture.detectChanges();
  });

  const componente = () => fixture.componentInstance;

  function cadastrar(nome: string, unidade: string) {
    componente()['nome'] = nome;
    componente()['unidade'] = unidade;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('lista os itens da Ação Social', () => {
    expect(el.querySelector('h1')?.textContent).toBe('Cadastro de itens da Ação Social');
    expect(el.querySelector('.linha__unidade')?.textContent?.trim()).toBe('kg · 4 em estoque');
  });

  it('oferece unidades de alimento', () => {
    expect(componente()['unidades']).toEqual(['kg', 'pacote', 'unidade', 'lata', 'garrafa']);
  });

  it('cadastrar envia para a API da Ação Social e recarrega', () => {
    servico.criar.mockReturnValue(of({ id: '2', nome: 'Feijão', unidade: 'kg', estoqueAtual: 0 }));

    cadastrar('Feijão', 'kg');

    expect(servico.criar).toHaveBeenCalledWith('Feijão', 'kg');
    expect(avisar).toHaveBeenCalledWith('"Feijão" cadastrado.', 'Fechar', expect.anything());
    expect(servico.listar).toHaveBeenCalledTimes(2);
  });

  it('nome repetido mostra o erro da API', () => {
    servico.criar.mockReturnValue(throwError(() => ({ error: { error: 'Já existe um item com esse nome' } })));

    cadastrar('Arroz', 'kg');

    expect(el.querySelector('.erro')?.textContent?.trim()).toBe('Já existe um item com esse nome');
  });

  it('remover confirma e chama a API', async () => {
    vi.spyOn(fixture.debugElement.injector.get(MatDialog), 'open').mockReturnValue({
      afterClosed: () => of(true)
    } as unknown as MatDialogRef<unknown>);
    servico.remover.mockReturnValue(of({ removido: false }));

    await componente()['remover']({ id: '1', nome: 'Arroz', unidade: 'kg', estoqueAtual: 4 });

    expect(servico.remover).toHaveBeenCalledWith('1');
    expect(avisar).toHaveBeenCalledWith(
      'Item removido da lista — o histórico foi mantido.',
      'Fechar',
      expect.anything()
    );
  });
});
