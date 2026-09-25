import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, of, throwError } from 'rxjs';
import { AcaoSocialService } from '../../core/acao-social.service';
import { AuthService } from '../../core/auth.service';
import { EstoqueSocialAlterado, ItemSocial } from '../../core/models';
import { AcaoSocial } from './acao-social';
import { MovimentacaoDialog, MovimentacaoInformada } from './movimentacao-dialog';

const ARROZ: ItemSocial = { id: '1', nome: 'Arroz', unidade: 'kg', estoqueAtual: 10 };
const MACARRAO: ItemSocial = { id: '2', nome: 'Macarrão', unidade: 'pacote', estoqueAtual: 8 };

const texto = (elemento: Element | null | undefined) => (elemento?.textContent ?? '').replace(/\s+/g, ' ').trim();

describe('AcaoSocial (estoque)', () => {
  const servico = { listar: vi.fn(), registrarEntrada: vi.fn(), ajustar: vi.fn() };
  let fixture: ComponentFixture<AcaoSocial>;
  let el: HTMLElement;
  let avisar: ReturnType<typeof vi.spyOn>;
  let abrirDialog: ReturnType<typeof vi.spyOn>;

  function criar(itens: ItemSocial[] = [ARROZ]) {
    servico.listar.mockReset().mockReturnValue(of(itens));
    servico.registrarEntrada
      .mockReset()
      .mockReturnValue(of({ itemId: '1', quantidadeAnterior: 10, quantidadeNova: 15 }));
    servico.ajustar.mockReset().mockReturnValue(of({ itemId: '1', quantidadeAnterior: 10, quantidadeNova: 7 }));

    TestBed.configureTestingModule({
      imports: [AcaoSocial],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AcaoSocialService, useValue: servico },
        { provide: AuthService, useValue: { ehAdmin: signal(true) } }
      ]
    });

    fixture = TestBed.createComponent(AcaoSocial);
    el = fixture.nativeElement;
    avisar = vi.spyOn(fixture.debugElement.injector.get(MatSnackBar), 'open').mockReturnValue(undefined as never);
    abrirDialog = vi.spyOn(fixture.debugElement.injector.get(MatDialog), 'open');
    fixture.detectChanges();
  }

  function dialogoFechandoCom(valor: MovimentacaoInformada | undefined) {
    abrirDialog.mockReturnValue({ afterClosed: () => of(valor) } as unknown as MatDialogRef<unknown>);
  }

  async function clicar(rotulo: string) {
    (el.querySelector('button[aria-label="' + rotulo + '"]') as HTMLButtonElement).click();
    await fixture.whenStable();
  }

  it('lista os itens com a quantidade e a unidade', () => {
    criar();

    expect(texto(el.querySelector('.linha__nome'))).toBe('Arroz');
    expect(texto(el.querySelector('.linha__qtd'))).toBe('10 kg');
  });

  it('sem itens, mostra o aviso e o atalho para o cadastro', () => {
    criar([]);

    expect(el.textContent).toContain('Nenhum item cadastrado ainda.');
    expect(el.querySelector('a[href="/acao-social/itens"]')).not.toBeNull();
  });

  it('doação envia a quantidade e atualiza a linha sem buscar a lista de novo', async () => {
    criar();
    dialogoFechandoCom({ quantidade: 5, texto: '' });

    await clicar('Registrar doação de Arroz');

    expect(abrirDialog).toHaveBeenCalledWith(
      MovimentacaoDialog,
      expect.objectContaining({ data: { tipo: 'entrada', item: ARROZ } })
    );
    expect(servico.registrarEntrada).toHaveBeenCalledWith('1', 5);
    expect(avisar).toHaveBeenCalledWith('Arroz: agora 15 kg.', 'Fechar', expect.anything());
    expect(servico.listar).toHaveBeenCalledTimes(1);
    fixture.detectChanges();
    expect(texto(el.querySelector('.linha__qtd'))).toBe('15 kg');
  });

  it('enquanto um item salva, só os botões dele ficam travados', async () => {
    criar([ARROZ, MACARRAO]);
    const resposta = new Subject<EstoqueSocialAlterado>();
    servico.registrarEntrada.mockReturnValue(resposta);
    dialogoFechandoCom({ quantidade: 5, texto: '' });

    await clicar('Registrar doação de Arroz');
    fixture.detectChanges();

    const botao = (rotulo: string) => el.querySelector('button[aria-label="' + rotulo + '"]') as HTMLButtonElement;
    expect(botao('Registrar doação de Arroz').disabled).toBe(true);
    expect(botao('Ajustar Arroz').disabled).toBe(true);
    expect(botao('Registrar doação de Macarrão').disabled).toBe(false);
    expect(botao('Ajustar Macarrão').disabled).toBe(false);

    resposta.next({ itemId: '1', quantidadeAnterior: 10, quantidadeNova: 15 });
    fixture.detectChanges();
    expect(botao('Registrar doação de Arroz').disabled).toBe(false);
  });

  it('unidade vai para o plural só acima de 1', () => {
    criar([MACARRAO, { ...MACARRAO, id: '3', nome: 'Óleo', unidade: 'garrafa', estoqueAtual: 0 }]);

    const quantidades = [...el.querySelectorAll('.linha__qtd')].map(texto);
    expect(quantidades).toEqual(['8 pacotes', '0 garrafa']);
  });

  it('ajuste envia a nova quantidade e o motivo', async () => {
    criar();
    dialogoFechandoCom({ quantidade: 7, texto: 'Venceu' });

    await clicar('Ajustar Arroz');

    expect(servico.ajustar).toHaveBeenCalledWith('1', 7, 'Venceu');
  });

  it('cancelar o diálogo não chama a API', async () => {
    criar();
    dialogoFechandoCom(undefined);

    await clicar('Registrar doação de Arroz');

    expect(servico.registrarEntrada).not.toHaveBeenCalled();
  });

  it('erro da API mostra a mensagem dela', async () => {
    criar();
    dialogoFechandoCom({ quantidade: 1, texto: '' });
    servico.registrarEntrada.mockReturnValue(throwError(() => ({ error: { error: 'Item não encontrado' } })));

    await clicar('Registrar doação de Arroz');

    expect(avisar).toHaveBeenCalledWith('Item não encontrado', 'Fechar', expect.anything());
  });
});
