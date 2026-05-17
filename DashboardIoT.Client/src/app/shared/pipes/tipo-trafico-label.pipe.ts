import { Pipe, PipeTransform } from '@angular/core';
import { TipoTraficoLabelsByName } from '../../core/models/tipo-trafico.enum';

@Pipe({ name: 'tipoTraficoLabel', standalone: true })
export class TipoTraficoLabelPipe implements PipeTransform {
  transform(value: string): string {
    const label = TipoTraficoLabelsByName[value];
    if (label === undefined) {
      throw new Error(`TipoTrafico desconocido: ${value}`);
    }
    return label;
  }
}
