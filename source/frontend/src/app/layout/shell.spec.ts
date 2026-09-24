import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { Modulo } from '../core/models';
import { Shell } from './shell';

@Component({ template: '' })
class Vazio {}

describe('Shell (menu)', () => {
  let fixture: ComponentFixture<Shell>;
  let el: HTMLElement;
  let modulos: Modulo[];
  const auth = {
    ehAdmin: signal(true),
    usuario: signal<string | null>('obreiros'),
    temModulo: (modulo: Modulo) => modulos.includes(modulo),
    logout: vi.fn()
  };

  function criar(liberados: Modulo[] = ['Obreiros']) {
    modulos = liberados;

    TestBed.configureTestingModule({
      imports: [Shell],
      providers: [
        provideRouter([{ path: '**', component: Vazio }]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: auth }
      ]
    });

    fixture = TestBed.createComponent(Shell);
    el = fixture.nativeElement;
    fixture.detectChanges();
  }

  const menuAberto = () => fixture.componentInstance['menuAberto']();
  const texto = (seletor: string) => el.querySelector(seletor)?.textContent?.trim();

  it('admin vê o link de cadastro', () => {
    criar();

    expect(el.querySelector('a[href="/cadastro"]')).not.toBeNull();
  });

  it('mostra com qual login está conectado e a versão do app', () => {
    criar();

    expect(texto('.menu__perfil')).toBe('Conectado como obreiros');
    expect(texto('.menu__versao')).toBe('Versão dev');
  });

  it('usuário só da Ação Social não vê os links dos Obreiros', () => {
    criar(['AcaoSocial']);

    expect(el.querySelector('a[href="/acao-social"]')).not.toBeNull();
    expect(el.querySelector('a[href="/dashboard"]')).toBeNull();
    expect(el.querySelector('a[href="/atualizar"]')).toBeNull();
    expect(el.querySelector('a[href="/cadastro"]')).toBeNull();
    expect(el.querySelector('a[href="/historico"]')).toBeNull();
  });

  it('usuário só dos Obreiros não vê o link da Ação Social', () => {
    criar(['Obreiros']);

    expect(el.querySelector('a[href="/dashboard"]')).not.toBeNull();
    expect(el.querySelector('a[href="/acao-social"]')).toBeNull();
  });

  it('o cabeçalho mostra o módulo da tela atual', async () => {
    criar(['Obreiros', 'AcaoSocial']);
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/dashboard');
    fixture.detectChanges();
    expect(texto('.modulo')).toBe('· Obreiros');

    await router.navigateByUrl('/acao-social');
    fixture.detectChanges();
    expect(texto('.modulo')).toBe('· Ação Social');
  });

  it('o botão de menu abre e fecha a gaveta, e clicar num link fecha', async () => {
    criar();
    const botaoMenu = el.querySelector('button[aria-label="Abrir menu"]') as HTMLButtonElement;

    botaoMenu.click();
    expect(menuAberto()).toBe(true);

    botaoMenu.click();
    expect(menuAberto()).toBe(false);

    botaoMenu.click();
    (el.querySelector('a[href="/historico"]') as HTMLAnchorElement).click();
    await fixture.whenStable();

    expect(menuAberto()).toBe(false);
  });

  it.each([
    ['sucesso', () => of({})],
    ['erro', () => throwError(() => new Error('falhou'))]
  ])('sair com %s no logout fecha o menu e vai para /login', (_, resposta) => {
    criar();
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    auth.logout.mockReturnValue(resposta());
    fixture.componentInstance['alternarMenu']();

    (el.querySelector('button.menu__sair') as HTMLButtonElement).click();

    expect(auth.logout).toHaveBeenCalled();
    expect(navegar).toHaveBeenCalledWith(['/login']);
    expect(menuAberto()).toBe(false);
  });
});
