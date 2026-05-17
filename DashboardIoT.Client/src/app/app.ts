import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { ImportarRegistrosService } from './core/services/importar-registros.service';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LayoutComponent, RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  protected importarRegistrosService = inject(ImportarRegistrosService);
  protected authService = inject(AuthService);
}
