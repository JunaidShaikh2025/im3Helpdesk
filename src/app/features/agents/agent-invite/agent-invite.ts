import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { ToastrService } from 'ngx-toastr';
import { AgentService } from '../../../services/agent';

@Component({
  selector: 'app-agent-invite',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatToolbarModule, MatProgressSpinnerModule, MatCardModule
  ],
  templateUrl: './agent-invite.html',
  styleUrls: ['./agent-invite.scss']
})
export class AgentInviteComponent {
  private agentService = inject(AgentService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  loading = false;
  success = false;
  tempPassword = '';

  form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['']
  });

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.cdr.detectChanges();

    this.agentService.invite(this.form.value).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.success = true;
        this.tempPassword = res.tempPassword;
        this.cdr.detectChanges();
        this.toastr.success('Agent invited successfully!');
      },
      error: (err: any) => {
        this.loading = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed to invite agent');
      }
    });
  }
}