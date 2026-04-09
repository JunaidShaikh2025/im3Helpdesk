import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'activeFilter',
})
export class ActiveFilterPipe implements PipeTransform {
  transform(value: unknown, ...args: unknown[]): unknown {
    return null;
  }
}
