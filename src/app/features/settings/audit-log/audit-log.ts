import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatButtonModule, MatToolbarModule, MatCardModule,
    MatProgressSpinnerModule, MatSelectModule, 
    MatFormFieldModule,LayoutComponent
  ],
  templateUrl: './audit-log.html',
  styleUrls: ['./audit-log.scss']
})
export class AuditLogComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  logs: any[] = [];
  loading = true;
  page = 1;
  pageSize = 20;
  total = 0;
  totalPages = 0;
  selectedType = '';

  entityTypes = ['', 'Ticket', 'Agent', 'Profile'];

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.loading = true;
    const params = new URLSearchParams();
    params.set('page', this.page.toString());
    params.set('pageSize', this.pageSize.toString());
    if (this.selectedType) params.set('entityType', this.selectedType);

    this.http.get<any>(
      `https://localhost:7071/api/Audit?${params.toString()}`,
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.logs = data.logs;
        this.total = data.total;
        this.totalPages = data.totalPages;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.loadLogs();
    }
  }

  nextPage() {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadLogs();
    }
  }

  getActionColor(action: string): string {
    const colors: any = {
      'Created': '#4caf50', 'StatusChanged': '#ff9800',
      'Commented': '#2196f3', 'Invited': '#9c27b0',
      'Updated': '#009688', 'BulkUpdate': '#ff5722',
      'Assigned': '#3f51b5', 'Deleted': '#f44336'
    };
    return colors[action] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}