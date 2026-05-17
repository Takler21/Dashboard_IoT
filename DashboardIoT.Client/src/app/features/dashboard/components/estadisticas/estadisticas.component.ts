import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartOptions } from 'chart.js';
import { RegistroService } from '../../../../core/services/registro.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { RegistrosPorTipoTraficoDto } from '../../../../core/models/registros-por-tipo-trafico.dto';
import { TipoTraficoLabelsByName } from '../../../../core/models/tipo-trafico.enum';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { tap } from 'rxjs';

@Component({
  selector: 'app-estadisticas',
  standalone: true,
  imports: [BaseChartDirective, SpinnerComponent],
  templateUrl: './estadisticas.component.html',
})
export class EstadisticasComponent {
  private readonly registroService = inject(RegistroService);
  private readonly destroyRef = inject(DestroyRef);

  readonly cargando = signal(true);

  readonly registrosPorTipoTrafico = toSignal(
    this.registroService
      .seleccionarTotalRegistrosPorTipoTrafico()
      .pipe(tap(() => this.cargando.set(false))),
    { initialValue: [] as RegistrosPorTipoTraficoDto[] },
  );

  readonly esPantallaPequena = signal(window.innerWidth < 992);

  ngOnInit(): void {
    const onResize = () => this.esPantallaPequena.set(window.innerWidth < 992);
    window.addEventListener('resize', onResize);
    this.destroyRef.onDestroy(() => window.removeEventListener('resize', onResize));
  }

  readonly estadisticasGraficoCircular = computed(() => ({
    labels: this.calcularLabels(),
    datasets: [{ data: this.calcularData() }],
  }));

  private calcularLabels(): string[] {
    const datos = this.registrosPorTipoTrafico();
    const total = datos.reduce((sum, x) => sum + x.numeroRegistros, 0);

    return datos.map((dato) => {
      const nombre = TipoTraficoLabelsByName[dato.tipoTrafico];
      const porcentaje = total > 0 ? ((dato.numeroRegistros / total) * 100).toFixed(1) : '0';
      return `${nombre} (${porcentaje}%)`;
    });
  }

  private calcularData(): number[] {
    return this.registrosPorTipoTrafico().map((dato) => dato.numeroRegistros);
  }

  readonly opcionesGrafico = computed<ChartOptions<'pie'>>(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: this.esPantallaPequena() ? 'bottom' : 'right',
        align: this.esPantallaPequena() ? 'start' : 'center',
      },
    },
  }));
}
