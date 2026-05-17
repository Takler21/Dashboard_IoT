import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RegistroService } from '../../core/services/registro.service';
import { RegistroListadoDto } from '../../core/models/registro-listado.dto';
import { FiltroRegistrosDto } from '../../core/models/filtro-registros.dto';
import { EstadisticasComponent } from './components/estadisticas/estadisticas.component';
import { FiltroComponent } from './components/filtro/filtro.component';
import { TablaRegistrosComponent } from './components/tabla-registros/tabla-registros.component';
import { ModalImportarRegistrosComponent } from './components/modal-importar-registros/modal-importar-registros.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    FiltroComponent,
    TablaRegistrosComponent,
    EstadisticasComponent,
    ModalImportarRegistrosComponent,
  ],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  private readonly registroService = inject(RegistroService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  registros = signal<RegistroListadoDto[]>([]);
  totalCount = signal(0);
  paginaActual = signal(0);
  tamanoPagina = signal(20);
  cargando = signal(false);
  registrosPendientes = signal(0);
  mostrarModalImportacion = signal(false);
  filtroActualizado: FiltroRegistrosDto = {};

  private filtroActual: FiltroRegistrosDto = {};
  private intervaloAviso = 0;

  ngOnInit(): void {
    const parametros = this.route.snapshot.queryParamMap;
    this.filtroActualizado = {
      fechaDesde: parametros.get('fechaDesde') ?? undefined,
      fechaHasta: parametros.get('fechaHasta') ?? undefined,
      direccionIp: parametros.get('direccionIp') ?? undefined,
      tipoTrafico: parametros.get('tipoTrafico')
        ? Number(parametros.get('tipoTrafico'))
        : undefined,
      idOrigenDatos: parametros.get('idOrigenDatos')
        ? Number(parametros.get('idOrigenDatos'))
        : undefined,
    };
    const paginaParam = parametros.get('pagina');
    const paginaInicial = paginaParam ? Number(paginaParam) : undefined;
    this.buscarRegistros(this.filtroActualizado, paginaInicial);

    this.seleccionarRegistrosPendientes();
    this.intervaloAviso = setInterval(() => {
      this.seleccionarRegistrosPendientes();
    }, 5000);
  }

  ngOnDestroy(): void {
    clearInterval(this.intervaloAviso);
  }

  buscarRegistros(filtro: FiltroRegistrosDto, paginaInicial?: number): void {
    this.filtroActual = filtro;
    this.cargarPagina(paginaInicial);
  }

  cambiarPagina(pagina: number): void {
    this.cargarPagina(pagina);
  }

  private cargarPagina(pagina?: number): void {
    this.cargando.set(true);
    const filtro: FiltroRegistrosDto = {
      ...this.filtroActual,
      ...(pagina != null ? { pagina } : {}),
    };
    this.registroService.buscarRegistros(filtro).subscribe({
      next: (resultado) => {
        this.registros.set(resultado.items);
        this.totalCount.set(resultado.totalCount);
        this.paginaActual.set(resultado.pagina);
        this.tamanoPagina.set(resultado.tamanoPagina);
        this.cargando.set(false);

        this.router.navigate([], {
          queryParams: {
            fechaDesde: this.filtroActual.fechaDesde ?? null,
            fechaHasta: this.filtroActual.fechaHasta ?? null,
            direccionIp: this.filtroActual.direccionIp ?? null,
            tipoTrafico: this.filtroActual.tipoTrafico ?? null,
            idOrigenDatos: this.filtroActual.idOrigenDatos ?? null,
            pagina: resultado.pagina,
          },
          queryParamsHandling: 'merge',
          replaceUrl: true,
        });
      },
      error: () => this.cargando.set(false),
    });
  }

  private seleccionarRegistrosPendientes(): void {
    this.registroService
      .seleccionarTotalRegistrosPendientes()
      .subscribe((result) => this.registrosPendientes.set(result.totalRegistrosPendientes));
  }

  clasificarPendientes() {
    this.registroService.clasificarPendientes().subscribe();
  }
}
