import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

const TOKEN_KEY = 'jwt_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  estaAutenticado(): boolean {
    return this.token() !== null;
  }

  login(email: string, contrasena: string): Observable<{ token: string }> {
    return this.http
      .post<{
        token: string;
      }>(`${environment.apiBaseUrl}/api/usuarios/login`, { email, contrasena })
      .pipe(
        tap((response) => {
          localStorage.setItem(TOKEN_KEY, response.token);
          this.token.set(response.token);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.token.set(null);
    this.router.navigate(['/login']);
  }
}
