import { Component, OnInit, OnDestroy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Subject, interval, takeUntil } from 'rxjs';
import { DashboardChartsComponent } from '../dashboard-charts/dashboard-charts';
import { GlobalSearchComponent } from '../../../shared/global-search/global-search';
import { DashboardTrendComponent } from '../dashboard-trend/dashboard-trend';

// imports array mein:


// imports array mein:


@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatCardModule,
    MatToolbarModule, MatProgressSpinnerModule,
    DashboardChartsComponent,GlobalSearchComponent,
    DashboardTrendComponent
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  public router = inject(Router);
  private http = inject(HttpClient);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();

  userName = '';
  userRole = '';
  loading = true;
  widgetData: any = null;
  stats: any = {
    totalTickets: 0, openTickets: 0,
    inProgressTickets: 0, resolvedTickets: 0,
    totalAgents: 0, newTicketsToday: 0,
    newTicketsThisWeek: 0, avgResolutionHours: 0,
    trialDaysLeft: 0, organizationName: '',
    recentTickets: []
  };

  ngOnInit() {
    const token = this.authService.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userName = payload[
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
      ] || payload.email || 'User';
      this.userRole = payload[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ] || payload.role || '';
    }
    this.loadStats();

    // Auto refresh every 60 seconds
    interval(60000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadStats());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

private getHeaders() {
  return new HttpHeaders({
    'Authorization': `Bearer ${this.authService.getToken()}`
  });
}



  loadStats() {
    const headers = this.getHeaders();
    this.http.get<any>(
      'https://localhost:7071/api/Dashboard/stats', { headers }
    ).subscribe({
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

  this.http.get<any>(
    'https://localhost:7071/api/Dashboard/widgets', { headers }
  ).subscribe({
    next: (data) => {
      this.widgetData = data;
      this.cdr.detectChanges();
    }
  });
}

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

  getTrialColor(): string {
    if (this.stats.trialDaysLeft > 15) return '#4caf50';
    if (this.stats.trialDaysLeft > 5) return '#ff9800';
    return '#f44336';
  }

  logout() {
    this.authService.logout();
  }
}