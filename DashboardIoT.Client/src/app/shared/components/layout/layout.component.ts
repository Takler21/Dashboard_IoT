import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterLink],
  template: `
    <nav class="navbar bg-white shadow-sm px-3 py-2">
      <a routerLink="/" class="inicio navbar-brand fw-bold text-dark mb-0 text-decoration-none"
        >Inicio</a
      >
      <div class="dropdown">
        <button
          class="btn btn-link text-dark p-0"
          (click)="menuAbierto.set(!menuAbierto())"
          aria-label="Usuario">
          <i class="bi bi-person-circle" style="font-size: 28px"></i>
        </button>
        @if (menuAbierto()) {
          <ul class="dropdown-menu show menu-usuario">
            <li>
              <button class="dropdown-item" (click)="authService.logout()">Cerrar sesión</button>
            </li>
          </ul>
        }
      </div>
    </nav>
    <main class="layout-main">
      <div class="container py-4">
        <ng-content></ng-content>
      </div>
    </main>
  `,
})
export class LayoutComponent {
  protected authService = inject(AuthService);
  menuAbierto = signal(false);
}
