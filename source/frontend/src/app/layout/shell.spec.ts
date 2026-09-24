import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { Perfil } from '../core/models';
import { Shell } from './shell';

@Component({ template: '' })
class Vazio {}

describe('Shell (menu)', () => {
  let fixture: ComponentFixture<Shell>;
  let el: HTMLElement;
  const auth = {
    ehAdmin: signal(true),
    perfil: signal<Perfil | null>('Admin'),
    logout: vi.fn()
  };

  function criar(perfil: Perfil) {
    auth.ehAdmin.set(perfil === 'Admin');
    auth.perfil.set(perfil);

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

  it('admin vê o link de cadastro', () => {
    criar('Admin');

    expect(el.querySelector('a[href="/cadastro"]')).not.toBeNull();
  });

  it('mostra com qual perfil está conectado', () => {
    criar('Admin');

    expect(el.querySelector('.menu__perfil')?.textContent?.trim()).toBe('Conectado como Admin');
  });

  it('o botão de menu abre e fecha a gaveta, e clicar num link fecha', async () => {
    criar('Admin');
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
    criar('Admin');
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    auth.logout.mockReturnValue(resposta());
    fixture.componentInstance['alternarMenu']();

    (el.querySelector('button.menu__sair') as HTMLButtonElement).click();

    expect(auth.logout).toHaveBeenCalled();
    expect(navegar).toHaveBeenCalledWith(['/login']);
    expect(menuAberto()).toBe(false);
  });
});
