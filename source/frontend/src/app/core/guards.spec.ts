import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, CanActivateFn, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Observable, firstValueFrom, of } from 'rxjs';
import { alteracoesNaoSalvasGuard, PodeTerAlteracoes } from './alteracoes-nao-salvas.guard';
import { adminGuard, authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { ConfirmacaoDialog } from '../shared/confirmacao-dialog';

describe('authGuard e adminGuard', () => {
  const auth = { carregarSessao: vi.fn(), ehAdmin: vi.fn() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }]
    });
  });

  function executar(guard: CanActivateFn) {
    const resultado = TestBed.runInInjectionContext(() =>
      guard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );

    return firstValueFrom(resultado as Observable<boolean | UrlTree>);
  }

  function destino(resultado: boolean | UrlTree) {
    return TestBed.inject(Router).serializeUrl(resultado as UrlTree);
  }

  it('authGuard libera quem está autenticado', async () => {
    auth.carregarSessao.mockReturnValue(of(true));

    expect(await executar(authGuard)).toBe(true);
  });

  it('authGuard manda para /login quem não está autenticado', async () => {
    auth.carregarSessao.mockReturnValue(of(false));

    expect(destino(await executar(authGuard))).toBe('/login');
  });

  it('adminGuard libera admin', async () => {
    auth.carregarSessao.mockReturnValue(of(true));
    auth.ehAdmin.mockReturnValue(true);

    expect(await executar(adminGuard)).toBe(true);
  });

  it('adminGuard manda voluntário para /dashboard', async () => {
    auth.carregarSessao.mockReturnValue(of(true));
    auth.ehAdmin.mockReturnValue(false);

    expect(destino(await executar(adminGuard))).toBe('/dashboard');
  });
});

describe('alteracoesNaoSalvasGuard', () => {
  const dialog = { open: vi.fn() };

  beforeEach(() => {
    dialog.open.mockReset();

    TestBed.configureTestingModule({
      providers: [{ provide: MatDialog, useValue: dialog }]
    });
  });

  function executar(temAlteracao: boolean) {
    const componente: PodeTerAlteracoes = { temAlteracaoPendente: () => temAlteracao };

    return TestBed.runInInjectionContext(() =>
      alteracoesNaoSalvasGuard(componente, {} as ActivatedRouteSnapshot, {} as RouterStateSnapshot, {} as RouterStateSnapshot)
    );
  }

  function dialogFechandoCom(valor: unknown) {
    dialog.open.mockReturnValue({ afterClosed: () => of(valor) } as unknown as MatDialogRef<unknown>);
  }

  it('sem alteração pendente libera a saída sem perguntar', () => {
    expect(executar(false)).toBe(true);
    expect(dialog.open).not.toHaveBeenCalled();
  });

  it('com alteração pendente abre a confirmação', async () => {
    dialogFechandoCom(true);

    await firstValueFrom(executar(true) as Observable<boolean>);

    expect(dialog.open).toHaveBeenCalledWith(ConfirmacaoDialog, {
      data: expect.objectContaining({ titulo: 'Sair sem salvar?', confirmar: 'Sair sem salvar' })
    });
  });

  it.each([
    [true, true],
    [false, false],
    [undefined, false]
  ])('diálogo fechado com %s resulta em %s', async (fechamento, esperado) => {
    dialogFechandoCom(fechamento);

    expect(await firstValueFrom(executar(true) as Observable<boolean>)).toBe(esperado);
  });
});
