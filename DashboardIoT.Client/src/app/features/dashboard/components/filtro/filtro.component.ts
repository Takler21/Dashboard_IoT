import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FiltroRegistrosDto } from '../../../../core/models/filtro-registros.dto';
import { OrigenDatosDto } from '../../../../core/models/origen-datos.dto';
import { TipoTrafico, TipoTraficoLabels } from '../../../../core/models/tipo-trafico.enum';
import { RegistroService } from '../../../../core/services/registro.service';

@Component({
  selector: 'app-filtro',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './filtro.component.html',
})
export class FiltroComponent implements OnInit {
  private readonly registroService = inject(RegistroService);

  filtrar = output<FiltroRegistrosDto>();
  filtroActualizado = input<FiltroRegistrosDto>({});

  filtro: FiltroRegistrosDto = {};
  origenesDatos = signal<OrigenDatosDto[]>([]);

  tiposTrafico = Object.values(TipoTrafico)
    .filter((v): v is TipoTrafico => typeof v === 'number')
    .map((valor) => ({ valor, etiqueta: TipoTraficoLabels[valor] }));

  ngOnInit(): void {
    const filtroInput = this.filtroActualizado();
    this.filtro = {
      fechaDesde: filtroInput.fechaDesde,
      fechaHasta: filtroInput.fechaHasta,
      direccionIp: filtroInput.direccionIp,
      tipoTrafico: filtroInput.tipoTrafico,
      idOrigenDatos: filtroInput.idOrigenDatos,
    };
    this.registroService
      .seleccionarOrigenDatos()
      .subscribe((datos) => this.origenesDatos.set(datos));
  }

  aplicarFiltro(): void {
    this.filtrar.emit(this.filtro);
  }
}
