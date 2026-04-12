import { Component, OnInit, OnDestroy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../../../services/notification';
import { AuthService } from '../../../services/auth.service';
import { Subject, interval, takeUntil } from 'rxjs';
import { LayoutComponent } from '../../../shared/layout/layout';



@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatTabsModule, MatProgressSpinnerModule,LayoutComponent
  ],
  templateUrl: './notifications-page.html',
  styleUrls: ['./notifications-page.scss']
})
export class NotificationsPageComponent implements OnInit, OnDestroy {
  private notifService = inject(NotificationService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();

  notifications: any[] = [];
  activityLogs: any[] = [];
  loading = true;
  loadingActivity = true;
  unreadCount = 0;

  ngOnInit() {
    this.loadNotifications();
    this.loadActivity();

    interval(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadNotifications());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadNotifications() {
    this.notifService.getAll().subscribe({
      next: (data: any[]) => {
        this.notifications = data;
        this.unreadCount = data.filter(n => !n.isRead).length;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadActivity() {
    this.notifService.getActivity().subscribe({
      next: (data: any[]) => {
        this.activityLogs = data;
        this.loadingActivity = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingActivity = false;
        this.cdr.detectChanges();
      }
    });
  }

  markRead(id: string) {
    this.notifService.markRead(id).subscribe({
      next: () => {
        const n = this.notifications.find(n => n.id === id);
        if (n) {
          n.isRead = true;
          this.unreadCount = this.notifications.filter(n => !n.isRead).length;
          this.cdr.detectChanges();
        }
      }
    });
  }

  markAllRead() {
    this.notifService.markAllRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('All marked as read')
        );
      }
    });
  }

  navigateToTicket(notification: any) {
    this.markRead(notification.id);
    if (notification.ticketId) {
      this.router.navigate(['/tickets', notification.ticketId]);
    }
  }

  getTypeIcon(type: string): string {
    const icons: any = {
      'info': 'ℹ',
      'success': '✓',
      'warning': '⚠',
      'error': '✗'
    };
    return icons[type] || 'ℹ';
  }

  getTypeColor(type: string): string {
    const colors: any = {
      'info': '#2196f3',
      'success': '#4caf50',
      'warning': '#ff9800',
      'error': '#f44336'
    };
    return colors[type] || '#2196f3';
  }

  getActionIcon(action: string): string {
    const icons: any = {
      'Created': '✚',
      'StatusChanged': '↻',
      'Commented': '💬',
      'Invited': '👤',
      'Updated': '✎',
      'Assigned': '→',
      'TimeLogged': '⏱',
      'BulkUpdate': '⊞'
    };
    return icons[action] || '•';
  }

  logout() {
    this.authService.logout();
  }
}