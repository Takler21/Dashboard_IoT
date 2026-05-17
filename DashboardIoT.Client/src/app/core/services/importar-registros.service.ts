import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResumenImportacion } from '../models/resumen-importacion';

export enum EstadosImportacion {
  Inactivo = 'inactivo',
  Subiendo = 'subiendo',
  Error = 'error',
}

@Injectable({ providedIn: 'root' })
export class ImportarRegistrosService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/registros`;
  private subscription: Subscription | null = null;

  status = signal(EstadosImportacion.Inactivo);
  fileName = signal('');
  error = signal('');
  mensaje = signal('');

  iniciarCarga(file: File): void {
    if (this.status() === EstadosImportacion.Subiendo) return;

    const formData = new FormData();
    formData.append('archivo', file);
    this.status.set(EstadosImportacion.Subiendo);
    this.fileName.set(file.name);
    this.error.set('');
    this.mensaje.set('');

    this.subscription = this.http.post<ResumenImportacion>(`${this.baseUrl}/importarcsv`, formData).subscribe({
      next: (resumen) => {
        this.status.set(EstadosImportacion.Inactivo);
        this.fileName.set('');
        this.mensaje.set(
          `${resumen.importadas} de ${resumen.totalFilas} importados (${resumen.duplicadas} duplicados, ${resumen.rechazadas} rechazados)`,
        );
      },
      error: (error) => {
        this.status.set(EstadosImportacion.Error);
        this.error.set(error.error);
      },
    });
  }

  cancelar(): void {
    this.subscription?.unsubscribe();
    this.status.set(EstadosImportacion.Inactivo);
    this.fileName.set('');
    this.error.set('');
  }
}
