import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatCardModule,
    MatToolbarModule, MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  public router = inject(Router);
  private http = inject(HttpClient);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  userName = '';
  userRole = '';
  loading = true;
  stats: any = {
    totalTickets: 0,
    openTickets: 0,
    inProgressTickets: 0,
    resolvedTickets: 0,
    totalAgents: 0,
    recentTickets: []
  };

  getStatusClass(status: any): string {
  const map: any = {
    0: 'open', 1: 'inprogress', 2: 'resolved', 3: 'closed',
    'Open': 'open', 'InProgress': 'inprogress',
    'Resolved': 'resolved', 'Closed': 'closed'
  };
  return map[status] || 'open';
}

getStatusLabel(status: any): string {
  const map: any = {
    0: 'Open', 1: 'InProgress', 2: 'Resolved', 3: 'Closed',
    'Open': 'Open', 'InProgress': 'InProgress',
    'Resolved': 'Resolved', 'Closed': 'Closed'
  };
  return map[status] || 'Open';
}

  ngOnInit() {
    const token = this.authService.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
        || payload.email || 'User';
      this.userRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        || payload.role || '';
    }
    this.loadStats();
  }

  loadStats() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });

    this.http.get<any>('https://localhost:7071/api/Dashboard/stats', { headers })
      .subscribe({
        next: (data) => {
          this.stats = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
  }

  logout() {
    this.authService.logout();
  }
}