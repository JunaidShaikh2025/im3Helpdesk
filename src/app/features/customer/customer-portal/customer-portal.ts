import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { ToastrService } from 'ngx-toastr';
import { CustomerService } from '../../../services/customer';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-customer-portal',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatButtonModule, MatToolbarModule, MatCardModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatProgressSpinnerModule, MatTabsModule
  ],
  templateUrl: './customer-portal.html',
  styleUrls: ['./customer-portal.scss']
})
export class CustomerPortalComponent implements OnInit {
  private customerService = inject(CustomerService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  tickets: any[] = [];
  loading = true;
  submitting = false;
  submitted = false;

  categories = ['General', 'Technical', 'Billing', 'Sales', 'Network', 'Other'];

  ticketForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    category: ['General', Validators.required]
  });

  ngOnInit() {
    this.loadMyTickets();
  }

  loadMyTickets() {
    this.loading = true;
    this.customerService.getMyTickets().subscribe({
      next: (data: any[]) => {
        this.tickets = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  submitTicket() {
    if (this.ticketForm.invalid) return;
    this.submitting = true;
    this.cdr.detectChanges();

    this.customerService.submitTicket(this.ticketForm.value).subscribe({
      next: () => {
        this.submitting = false;
        this.submitted = true;
        this.ticketForm.reset({ category: 'General' });
        this.cdr.detectChanges();
        this.toastr.success('Ticket submitted! We will get back to you soon.');
        setTimeout(() => {
          this.submitted = false;
          this.loadMyTickets();
          this.cdr.detectChanges();
        }, 3000);
      },
      error: (err: any) => {
        this.submitting = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed to submit ticket');
      }
    });
  }

  viewTicket(id: string) {
    this.router.navigate(['/customer/ticket', id]);
  }

  getStatusColor(status: string): string {
    const colors: any = {
      'Open': '#f44336', 'InProgress': '#ff9800',
      'Resolved': '#4caf50', 'Closed': '#9e9e9e'
    };
    return colors[status] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}