import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';
import { FormControl, FormGroup } from '@angular/forms';


@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatToolbarModule, MatCardModule, MatProgressSpinnerModule,
    MatDatepickerModule, MatNativeDateModule
  ],
  templateUrl: './reports-page.html',
  styleUrls: ['./reports-page.scss']
})
export class ReportsPageComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  private apiUrl = 'https://localhost:7071/api/Reports';
  loading = false;
  report: any = null;

  dateForm = new FormGroup({
    from: new FormControl<Date>(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)),
    to: new FormControl<Date>(new Date())
  });

  ngOnInit() {
    this.loadReport();
  }
  

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  loadReport() {
    this.loading = true;
    this.cdr.detectChanges();

    const from = this.dateForm.value.from?.toISOString();
    const to = this.dateForm.value.to?.toISOString();

    this.http.get<any>(
      `${this.apiUrl}/summary?from=${from}&to=${to}`,
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.report = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load report');
        this.cdr.detectChanges();
      }
    });
  }

  exportCsv() {
    const from = this.dateForm.value.from?.toISOString();
    const to = this.dateForm.value.to?.toISOString();

    this.http.get(
      `${this.apiUrl}/export-csv?from=${from}&to=${to}`,
      { headers: this.getHeaders(), responseType: 'blob' }
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `tickets-${new Date().toISOString().split('T')[0]}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastr.success('CSV exported!');
      },
      error: () => {
        this.toastr.error('Export failed');
      }
    });
  }

  getBarWidth(count: number): string {
    if (!this.report) return '0%';
    const max = this.report.totalTickets || 1;
    return Math.round((count / max) * 100) + '%';
  }

  getStatusColor(status: string): string {
    const colors: any = {
      'Open': '#f44336', 'InProgress': '#ff9800',
      'Resolved': '#4caf50', 'Closed': '#9e9e9e'
    };
    return colors[status] || '#2196f3';
  }

  getPriorityColor(priority: string): string {
    const colors: any = {
      'Critical': '#f44336', 'High': '#ff9800',
      'Medium': '#2196f3', 'Low': '#4caf50'
    };
    return colors[priority] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}