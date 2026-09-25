import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AcaoSocialService } from '../../core/acao-social.service';
import { AuthService } from '../../core/auth.service';
import { CestaResumo } from '../../core/models';
import { unidadePara } from '../../core/unidade';

@Component({
  selector: 'app-cesta',
  imports: [
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './cesta.html',
  styleUrl: './cesta.css'
})
export class Cesta implements OnInit {
  protected readonly unidadePara = unidadePara;
  private readonly acaoSocial = inject(AcaoSocialService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly ehAdmin = inject(AuthService).ehAdmin;
  protected readonly resumo = signal<CestaResumo | null>(null);
  protected readonly falhaAoCarregar = signal(false);
  protected readonly montando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected quantidade: number | null = 1;

  ngOnInit(): void {
    this.carregar();
  }

  private carregar() {
    this.acaoSocial.obterCesta().subscribe({
      next: (resumo) => {
        this.resumo.set(resumo);
        this.falhaAoCarregar.set(false);
      },
      error: () => this.falhaAoCarregar.set(true)
    });
  }

  protected montar() {
    if (!this.quantidade || this.quantidade < 1) {
      return;
    }

    this.erro.set(null);
    this.montando.set(true);

    this.acaoSocial.montar(this.quantidade).subscribe({
      next: (resultado) => {
        const cestas = resultado.montadas === 1 ? '1 cesta montada' : `${resultado.montadas} cestas montadas`;
        this.snackBar.open(`${cestas}. Prontas: ${resultado.cestasProntas}.`, 'Fechar', { duration: 4000 });
        this.quantidade = 1;
        this.montando.set(false);
        this.carregar();
      },
      error: (resposta) => {
        this.erro.set(resposta.error?.error ?? 'Não foi possível montar as cestas.');
        this.montando.set(false);
      }
    });
  }
}
