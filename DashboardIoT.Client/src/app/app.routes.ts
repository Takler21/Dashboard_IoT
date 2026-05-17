import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LoginComponent } from './features/login/login.component';
import { RegistroDetalleComponent } from './features/registro-detalle/registro-detalle.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent, title: 'Iniciar sesión' },
  { path: '', component: DashboardComponent, canActivate: [authGuard], title: 'Dashboard' },
  { path: 'registros/:id', component: RegistroDetalleComponent, canActivate: [authGuard], title: 'Detalle registro' },
  { path: '**', redirectTo: 'login' },
];
