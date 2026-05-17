import { Component, inject, signal, output } from '@angular/core';
import { EstadosImportacion, ImportarRegistrosService } from '../../../../core/services/importar-registros.service';

@Component({
  selector: 'app-modal-importar-registros',
  standalone: true,
  templateUrl: './modal-importar-registros.component.html',
})
export class ModalImportarRegistrosComponent {
  protected importarRegistrosService = inject(ImportarRegistrosService);
  protected readonly EstadosImportacion = EstadosImportacion;
  closed = output<void>();
  archivoSeleccionado = signal<File | null>(null);

  alSeleccionarArchivo(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.archivoSeleccionado.set(file);
  }
}
