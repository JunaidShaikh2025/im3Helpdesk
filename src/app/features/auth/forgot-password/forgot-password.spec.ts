import { Component, ChangeDetectorRef } from '@angular/core'; // ChangeDetectorRef add karein
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatFormFieldModule, MatInputModule,
    MatButtonModule, MatProgressSpinnerModule
  ],
  templateUrl: './forgot-password.html',
  styleUrls: ['./forgot-password.scss']
})
export class ForgotPasswordComponent {
  loading = false;
  submitted = false;
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef // Isko inject karein
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.cdr.detectChanges(); // Change detect karwayein

    this.authService.forgotPassword(this.form.value).subscribe({
      next: (res) => {
        this.loading = false;
        this.submitted = true;
        this.toastr.success('Reset link sent to your email!');
        this.cdr.detectChanges(); // Success ke baad update karein
      },
      error: (err) => {
        this.loading = false;
        this.toastr.error(err.error?.message || 'Something went wrong');
        this.cdr.detectChanges(); // Error ke baad update karein
      }
    });
  }
}