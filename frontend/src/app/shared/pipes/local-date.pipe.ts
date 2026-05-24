import { Pipe, PipeTransform, LOCALE_ID, inject } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'localDate',
  standalone: true
})
export class LocalDatePipe implements PipeTransform {
  private readonly locale = inject(LOCALE_ID);
  private readonly datePipe = new DatePipe(this.locale);

  transform(
    value: string | Date | null | undefined,
    format: string = 'EEE, dd MMM yyyy, hh:mm a',
    timezone: string = 'Asia/Kolkata'
  ): string {
    if (!value) return '';
    return this.datePipe.transform(value, format, timezone) ?? '';
  }
}