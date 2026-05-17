import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FiltroRegistrosDto } from '../models/filtro-registros.dto';
import { OrigenDatosDto } from '../models/origen-datos.dto';
import { RegistroDetalleDto } from '../models/registro-detalle.dto';
import { RegistroListadoDto } from '../models/registro-listado.dto';
import { ResultadoPaginado } from '../models/resultado-paginado';
import { RegistrosPorTipoTraficoDto } from '../models/registros-por-tipo-trafico.dto';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RegistroService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/registros`;

  seleccionarTotalRegistrosPorTipoTrafico(): Observable<RegistrosPorTipoTraficoDto[]> {
    return this.http.get<RegistrosPorTipoTraficoDto[]>(`${this.baseUrl}/totales-por-tipo-trafico`);
  }

  buscarRegistros(
    filtro: FiltroRegistrosDto = {},
  ): Observable<ResultadoPaginado<RegistroListadoDto>> {
    const params: Record<string, string | number> = {};
    for (const [key, value] of Object.entries(filtro)) {
      if (value != null && value !== '') {
        params[key] = value;
      }
    }

    return this.http.get<ResultadoPaginado<RegistroListadoDto>>(this.baseUrl, { params });
  }

  obtenerPorId(id: number): Observable<RegistroDetalleDto> {
    return this.http.get<RegistroDetalleDto>(`${this.baseUrl}/${id}`);
  }

  seleccionarTotalRegistrosPendientes(): Observable<{ totalRegistrosPendientes: number }> {
    return this.http.get<{ totalRegistrosPendientes: number }>(`${this.baseUrl}/pendientes`);
  }

  seleccionarOrigenDatos(): Observable<OrigenDatosDto[]> {
    return this.http.get<OrigenDatosDto[]>(`${this.baseUrl}/origen-datos`);
  }

  clasificarPendientes() {
    return this.http.post(`${this.baseUrl}/clasificar`, {});
  }
}

