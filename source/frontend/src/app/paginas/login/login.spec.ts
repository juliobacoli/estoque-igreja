import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Subject, of, throwError } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { DicaSenhaDialog } from './dica-senha-dialog';
import { Login } from './login';

describe('Login', () => {
  const auth = { login: vi.fn(), rotaInicial: vi.fn() };
  let fixture: ComponentFixture<Login>;
  let el: HTMLElement;
  let navegar: ReturnType<typeof vi.spyOn>;
  let abrirDialog: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    auth.login.mockReset();

    TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([]), provideNoopAnimations(), { provide: AuthService, useValue: auth }]
    });

    navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(Login);
    el = fixture.nativeElement;
    abrirDialog = vi
      .spyOn(fixture.debugElement.injector.get(MatDialog), 'open')
      .mockReturnValue({ afterClosed: () => of(undefined) } as unknown as MatDialogRef<unknown>);
    fixture.detectChanges();
  });

  function enviar(login = 'admin', senha = 'senha') {
    fixture.componentInstance['login'] = login;
    fixture.componentInstance['senha'] = senha;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  function errar(vezes = 1) {
    auth.login.mockReturnValue(throwError(() => ({ error: { error: 'Login ou senha inválidos' } })));

    for (let i = 0; i < vezes; i++) {
      enviar();
    }
  }

  const botaoOlho = () => el.querySelector('button[matsuffix]') as HTMLButtonElement;
  const botaoEntrar = () => el.querySelector('button.entrar') as HTMLButtonElement;

  it('login com sucesso envia as credenciais e vai para a área do usuário', () => {
    auth.login.mockReturnValue(of({ perfil: 'Admin', login: 'social', modulos: ['AcaoSocial'] }));
    auth.rotaInicial.mockReturnValue('/acao-social');

    enviar('admin', 'senha-certa');

    expect(auth.login).toHaveBeenCalledWith('admin', 'senha-certa');
    expect(navegar).toHaveBeenCalledWith(['/acao-social']);
  });

  it('login com erro mostra a mensagem da API, sacode e reabilita o botão', () => {
    errar();

    expect(el.querySelector('.erro')?.textContent?.trim()).toBe('Login ou senha inválidos');
    expect(fixture.componentInstance['shakeState']()).toBe('shake');
    expect(botaoEntrar().disabled).toBe(false);
    expect(navegar).not.toHaveBeenCalled();
  });

  it('erro sem mensagem da API mostra a mensagem padrão', () => {
    auth.login.mockReturnValue(throwError(() => ({ status: 0 })));

    enviar();

    expect(el.querySelector('.erro')?.textContent?.trim()).toBe('Login ou senha inválidos');
  });

  it('durante o envio desabilita campos e botão e mostra o spinner', async () => {
    auth.login.mockReturnValue(new Subject());

    enviar();
    await fixture.whenStable();

    expect(botaoEntrar().disabled).toBe(true);
    expect(botaoEntrar().querySelector('mat-progress-spinner')).not.toBeNull();
    expect((el.querySelector('input[name="login"]') as HTMLInputElement).disabled).toBe(true);
    expect((el.querySelector('input[name="senha"]') as HTMLInputElement).disabled).toBe(true);
  });

  it('o olho alterna a senha entre visível e oculta', () => {
    const senha = () => el.querySelector('input[name="senha"]') as HTMLInputElement;
    expect(senha().type).toBe('password');
    expect(botaoOlho().getAttribute('aria-label')).toBe('Mostrar senha');

    botaoOlho().click();
    fixture.detectChanges();

    expect(senha().type).toBe('text');
    expect(botaoOlho().getAttribute('aria-label')).toBe('Ocultar senha');
    expect(botaoOlho().getAttribute('aria-pressed')).toBe('true');

    botaoOlho().click();
    fixture.detectChanges();

    expect(senha().type).toBe('password');
  });

  it('na terceira falha abre a dica e destaca o olho ao fechar', () => {
    errar(2);
    expect(abrirDialog).not.toHaveBeenCalled();

    errar(1);

    expect(abrirDialog).toHaveBeenCalledOnce();
    expect(abrirDialog.mock.calls[0][0]).toBe(DicaSenhaDialog);
    expect(botaoOlho().classList).toContain('destacado');
  });

  it('a dica não abre se a senha já estiver visível', () => {
    botaoOlho().click();
    fixture.detectChanges();

    errar(3);

    expect(abrirDialog).not.toHaveBeenCalled();
  });

  it('a dica não abre de novo na quarta falha', () => {
    errar(4);

    expect(abrirDialog).toHaveBeenCalledOnce();
  });

  it('tocar no olho remove o destaque', () => {
    errar(3);

    botaoOlho().click();
    fixture.detectChanges();

    expect(botaoOlho().classList).not.toContain('destacado');
  });
});
