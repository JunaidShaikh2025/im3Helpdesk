import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({ providedIn: 'root' })
export class ToastService {
  constructor(private toastr: ToastrService) {}

  success(msg = 'Saved successfully!') {
    Promise.resolve().then(() =>
      this.toastr.success(msg, 'Success', {
        timeOut: 2500,
        positionClass: 'toast-top-right'
      })
    );
  }

  error(msg = 'Something went wrong. Please try again.') {
    Promise.resolve().then(() =>
      this.toastr.error(msg, 'Error', {
        timeOut: 3500
      })
    );
  }

  info(msg: string) {
    Promise.resolve().then(() =>
      this.toastr.info(msg, '', { timeOut: 2000 })
    );
  }
}