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
  rangeDays = 7;
  readonly ranges = [{ label: 'Today', days: 1 }, { label: '7 days', days: 7 }, { label: '30 days', days: 30 }];
  overview: any = { trend: [], priority: [], status: [], teams: [], recentTickets: [] };

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
      stats: this.http.get<any>(`${API_BASE}/Dashboard/overview?rangeDays=${this.rangeDays}`).pipe(
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
        if (stats) { this.overview = stats; this.stats = stats; }
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

  setRange(days: number): void {
    if (this.rangeDays === days) return;
    this.rangeDays = days;
    this.loading = true;
    this.loadAll();
  }

  get totalChange(): number {
    const previous = Number(this.overview.previousTotal || 0);
    const current = Number(this.overview.totalTickets || 0);
    return previous ? Math.round(((current - previous) / previous) * 100) : 0;
  }

  abs(value: number): number { return Math.abs(value); }

  formatMinutes(value: number): string {
    const minutes = Number(value || 0);
    if (!minutes) return '—';
    return minutes < 60 ? `${Math.round(minutes)}m` : `${(minutes / 60).toFixed(1)}h`;
  }

  barHeight(value: number): number {
    const rows = this.overview.trend || [];
    const max = Math.max(1, ...rows.flatMap((x: any) => [Number(x.created || 0), Number(x.resolved || 0)]));
    return Math.max(value ? 8 : 0, Math.round((value / max) * 100));
  }

  share(value: number): number {
    return this.overview.totalTickets ? Math.round((value / this.overview.totalTickets) * 100) : 0;
  }

  get priorityItems(): Array<{ label: string; count: number; color: string }> {
    const colors: Record<string, string> = { Low: '#91a9d6', Medium: 'var(--ui-color-primary)', High: '#5d7fd1', Critical: '#dc3d3d' };
    return ['Low', 'Medium', 'High', 'Critical'].map(label => ({ label, count: this.countFor(this.overview.priority, 'priority', label), color: colors[label] }));
  }

  get statusItems(): Array<{ label: string; count: number; color: string }> {
    const colors: Record<string, string> = { Open: '#4166c9', InProgress: '#6c8bd1', Pending: '#b4c4e5', Resolved: '#91a9d6' };
    return [{ label: 'Open', key: 'Open' }, { label: 'In progress', key: 'InProgress' }, { label: 'Pending', key: 'Pending' }, { label: 'Resolved', key: 'Resolved' }]
      .map(x => ({ label: x.label, count: x.key === 'Resolved'
        ? this.countFor(this.overview.status, 'status', 'Resolved') + this.countFor(this.overview.status, 'status', 'Closed') + this.countFor(this.overview.status, 'status', 'ResolvedOnBeta')
        : this.countFor(this.overview.status, 'status', x.key), color: colors[x.key] }));
  }

  get statusDonut(): string {
    const items = this.statusItems; const total = Math.max(1, items.reduce((sum, x) => sum + x.count, 0)); let cursor = 0;
    const stops = items.map(item => { const start = cursor; cursor += item.count / total * 100; return `${item.color} ${start}% ${cursor}%`; });
    return `conic-gradient(${stops.join(', ')})`;
  }

  private countFor(rows: any[], key: string, value: string): number {
    return Number((rows || []).find(x => x[key] === value)?.count || 0);
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
