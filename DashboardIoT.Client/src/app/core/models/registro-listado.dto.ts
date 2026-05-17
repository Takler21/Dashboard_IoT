export interface RegistroListadoDto {
  idRegistro: number;
  ipOrigen: string;
  ipDestino: string | null;
  tipoTrafico: string;
  fecha: string;
}
