import { Component, OnInit, OnDestroy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../auth/auth.service';
import { SubscriptionService } from '../../core/services/subscription';
import { ToastrService } from 'ngx-toastr';
import { Subject, interval, takeUntil, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardChartsComponent } from './dashboard-charts/dashboard-charts';
import { DashboardTrendComponent } from './dashboard-trend/dashboard-trend';
import { LayoutComponent } from '../../layouts/main-layout/layout';
import { environment } from '../../../environments/environment';

const API_BASE = environment.apiUrl;
const REFRESH_INTERVAL_MS = 60_000;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatCardModule,
    MatToolbarModule, MatProgressSpinnerModule,
    DashboardChartsComponent,
    DashboardTrendComponent,
    LayoutComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {

  private authService = inject(AuthService);
  public  sub         = inject(SubscriptionService);
  public  router      = inject(Router);
  private http        = inject(HttpClient);
  private toastr      = inject(ToastrService);
  private cdr         = inject(ChangeDetectorRef);
  private destroy$    = new Subject<void>();

  userName    = '';
  userEmail   = '';
  userRole    = '';
  userInitials = '';
  loading     = true;
  error       = false;

  widgetData: any = null;

  stats: any = {
    totalTickets: 0, openTickets: 0,
    inProgressTickets: 0, resolvedTickets: 0,
    closedTickets: 0,
    totalAgents: 0, newTicketsToday: 0,
    newTicketsThisWeek: 0, avgResolutionHours: '0.0',
    lowPriority: 0, mediumPriority: 0,
    highPriority: 0, criticalPriority: 0,
    trialDaysLeft: 30, organizationName: '',
    recentTickets: []
  };

  // Today/tomorrow events surfaced as an animated strip above the trial banner.
  events: Array<{
    kind: 'holiday' | 'birthday';
    title: string;
    subtitle: string;
    when: 'today' | 'tomorrow';
    icon: string;
    color: string;
  }> = [];
  eventsLoaded = false;

  ngOnInit(): void {
    this.initUserFromToken();
    this.loadAll();
    this.loadEvents();

    interval(REFRESH_INTERVAL_MS)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.loadAll(); this.loadEvents(); });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Loads today/tomorrow holidays + birthdays for the dashboard event strip. */
  loadEvents(): void {
    const holidays$ = this.http.get<any>(`${API_BASE}/Holidays/reminders`).pipe(
      catchError(() => of(null))
    );
    const birthdays$ = this.http.get<any>(`${API_BASE}/Birthdays/reminders`).pipe(
      catchError(() => of(null))
    );

    forkJoin({ holidays: holidays$, birthdays: birthdays$ })
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ holidays, birthdays }) => {
        const out: typeof this.events = [];

        const hItems = Array.isArray(holidays?.items) ? holidays.items : [];
        for (const h of hItems) {
          out.push({
            kind: 'holiday',
            title: h.occasion,
            subtitle: h.when === 'today' ? 'Holiday today' : 'Holiday tomorrow',
            when: h.when,
            icon: h.isFloating ? '🎊' : '🎉',
            color: h.isFloating ? '#f59e0b' : '#10b981'
          });
        }

        const bItems = Array.isArray(birthdays?.items) ? birthdays.items : [];
        for (const b of bItems) {
          out.push({
            kind: 'birthday',
            title: b.fullName,
            subtitle: b.when === 'today' ? 'Birthday today' : 'Birthday tomorrow',
            when: b.when,
            icon: '🎂',
            color: '#ec4899'
          });
        }

        this.events = out;
        this.eventsLoaded = true;
        this.cdr.detectChanges();
      });
  }

  trackEvent = (_: number, e: { kind: string; title: string }) => `${e.kind}|${e.title}`;

  private initUserFromToken(): void {
    this.userName = this.authService.getUserName() || 'User';
    this.userRole = this.authService.getUserRole() || '';
    this.userEmail = '';
    this.userInitials = this.userName
      .split(' ')
      .filter((n: string) => n.length)
      .map((n: string) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  loadAll(): void {
    forkJoin({
      stats: this.http.get<any>(`${API_BASE}/Dashboard/stats`).pipe(
        catchError(err => {
          console.error('Stats error:', err.status, err.error);
          this.toastr.error('Could not load dashboard stats', 'Error');
          return of(null);
        })
      ),
      widgets: this.http.get<any>(`${API_BASE}/Dashboard/widgets`).pipe(
        catchError(err => {
          console.warn('Widgets error:', err.status);
          return of(null);
        })
      )
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: ({ stats, widgets }) => {
        if (stats)   this.stats      = stats;
        if (widgets) this.widgetData = widgets;
        this.loading = false;
        this.error   = !stats;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.error   = true;
        this.cdr.detectChanges();
      }
    });
  }

  getTimeAgo(date: string): string {
    if (!date) return '';
    const diff = Date.now() - new Date(date).getTime();
    const mins = Math.floor(diff / 60_000);
    if (mins < 1)  return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24)  return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
  }

  getStatusClass(status: any): string {
    const map: Record<string, string> = {
      '0': 'open', '1': 'inprogress', '2': 'resolved', '3': 'closed',
      'Open': 'open', 'InProgress': 'inprogress',
      'Resolved': 'resolved', 'Closed': 'closed'
    };
    return map[String(status)] ?? 'open';
  }

  getStatusLabel(status: any): string {
    const map: Record<string, string> = {
      '0': 'Open', '1': 'In Progress', '2': 'Resolved', '3': 'Closed',
      'Open': 'Open', 'InProgress': 'In Progress',
      'Resolved': 'Resolved', 'Closed': 'Closed'
    };
    return map[String(status)] ?? 'Open';
  }

  getTrialColor(): string {
    const d = this.trialDaysLeft();
    if (d > 7) return '#2563eb';   // info blue — plenty of time
    if (d > 3) return '#f59e0b';   // amber — warning
    return '#dc2626';              // red — urgent
  }

  /** Banner visibility: only CompanyAdmin during Trial. */
  showTrialBanner(): boolean {
    if (this.userRole !== 'CompanyAdmin') return false;
    const s = this.sub.subscription();
    return !!s && s.isTrial === true;
  }

  trialDaysLeft(): number {
    return this.sub.subscription()?.daysRemaining ?? 0;
  }

  goToUpgrade(): void {
    this.router.navigate(['/explore-plans']);
  }

  logout(): void {
    this.authService.logout();
  }
}
