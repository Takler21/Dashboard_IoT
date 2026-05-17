import { Component, computed, inject, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { RegistroListadoDto } from '../../../../core/models/registro-listado.dto';
import { TipoTraficoLabelPipe } from '../../../../shared/pipes/tipo-trafico-label.pipe';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-tabla-registros',
  standalone: true,
  imports: [DatePipe, TipoTraficoLabelPipe, SpinnerComponent],
  templateUrl: './tabla-registros.component.html',
})
export class TablaRegistrosComponent {
  private readonly router = inject(Router);

  registros = input<RegistroListadoDto[]>([]);
  totalCount = input(0);
  paginaActual = input(1);
  tamanoPagina = input(20);
  cargando = input(false);

  cambiarPagina = output<number>();

  totalPaginas = computed(() => {
    if (this.totalCount() === 0) return 1;
    return Math.ceil(this.totalCount() / this.tamanoPagina());
  });

  paginas = computed(() => {
    const total = this.totalPaginas();
    const actual = this.paginaActual();
    const rango = 2;
    const inicio = Math.max(1, actual - rango);
    const fin = Math.min(total, actual + rango);

    const paginas: number[] = [];
    for (let i = inicio; i <= fin; i++) {
      paginas.push(i);
    }
    return paginas;
  });

  irAPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas() && pagina !== this.paginaActual()) {
      this.cambiarPagina.emit(pagina);
    }
  }

  mostrarDetalleRegistro(idRegistro: number): void {
    this.router.navigate(['/registros', idRegistro], {
      queryParamsHandling: 'preserve',
    });
  }
}
