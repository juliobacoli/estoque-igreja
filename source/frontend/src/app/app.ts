import { Component, inject, signal, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface HealthResponse {
  status: string;
  timestamp: string;
}

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly http = inject(HttpClient);

  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly health = signal<HealthResponse | null>(null);

  ngOnInit(): void {
    this.http.get<HealthResponse>('/health').subscribe({
      next: (resposta) => {
        this.health.set(resposta);
        this.carregando.set(false);
      },
      error: (err) => {
        this.erro.set(`${err.status} — ${err.message}`);
        this.carregando.set(false);
      }
    });
  }
}
