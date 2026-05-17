import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  contrasena = '';
  error = signal('');
  cargando = signal(false);

  login(): void {
    this.error.set('');
    this.cargando.set(true);

    this.authService.login(this.email, this.contrasena).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/']);
      },
      error: () => {
        this.cargando.set(false);
        this.error.set('Credenciales incorrectas');
      },
    });
  }
}
