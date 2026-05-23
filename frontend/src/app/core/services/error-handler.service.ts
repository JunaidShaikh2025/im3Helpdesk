import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  constructor() {}

  handleHttpError(error: HttpErrorResponse): void {
    console.error('HTTP error intercepted', {
      status: error.status,
      message: error.message,
      url: error.url
    });
  }
}
