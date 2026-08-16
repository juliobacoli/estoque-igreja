import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.css'
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly menuAberto = signal(false);
  protected readonly ehAdmin = this.auth.ehAdmin;
  protected readonly perfil = this.auth.perfil;

  protected alternarMenu() {
    this.menuAberto.update((aberto) => !aberto);
  }

  protected fecharMenu() {
    this.menuAberto.set(false);
  }

  protected sair() {
    this.fecharMenu();
    this.auth.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])
    });
  }
}
