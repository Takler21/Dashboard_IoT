import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { RegistroService } from '../../core/services/registro.service';
import { RegistroDetalleDto } from '../../core/models/registro-detalle.dto';
import { TipoTraficoLabelPipe } from '../../shared/pipes/tipo-trafico-label.pipe';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-registro-detalle',
  standalone: true,
  imports: [RouterLink, DatePipe, TipoTraficoLabelPipe, SpinnerComponent],
  templateUrl: './registro-detalle.component.html',
})
export class RegistroDetalleComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly registroService = inject(RegistroService);

  registro = signal<RegistroDetalleDto | null>(null);
  cargando = signal(true);
  error = signal<string | null>(null);
  queryParams = signal<Record<string, string>>({});

  datosJsonFormateados = computed(() => {
    const reg = this.registro();
    if (!reg) return '';
    return JSON.stringify(JSON.parse(reg.datosJson), null, 2);
  });

  ngOnInit(): void {
    const parametros = this.route.snapshot.queryParams;
    this.queryParams.set(parametros);

    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.registroService.obtenerPorId(id).subscribe({
      next: (data) => {
        this.registro.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el registro.');
        this.cargando.set(false);
      },
    });
  }
}
