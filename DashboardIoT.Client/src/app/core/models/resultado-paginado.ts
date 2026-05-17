export interface ResultadoPaginado<T> {
  items: T[];
  totalCount: number;
  pagina: number;
  tamanoPagina: number;
}
