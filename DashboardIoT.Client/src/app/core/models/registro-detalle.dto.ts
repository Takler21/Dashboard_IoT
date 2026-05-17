export interface RegistroDetalleDto {
  idRegistro: number;
  ipOrigen: string;
  ipDestino: string | null;
  fecha: string;
  tipoTrafico: string;
  justificacion: string;
  datosJson: string;
}
