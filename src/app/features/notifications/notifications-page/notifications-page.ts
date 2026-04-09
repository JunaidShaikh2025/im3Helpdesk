import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../../../services/notification';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatCardModule, MatTabsModule,
    MatProgressSpinnerModule, MatBadgeModule
  ],
  templateUrl: './notifications-page.html',
  styleUrls: ['./notifications-page.scss']
})
export class NotificationsPageComponent implements OnInit {
  private notifService = inject(NotificationService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  notifications: any[] = [];
  activityLogs: any[] = [];
  loading = true;
  loadingActivity = true;
  unreadCount = 0;

  ngOnInit() {
    this.loadNotifications();
    this.loadActivity();
  }

  loadNotifications() {
    this.loading = true;
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
    this.loadingActivity = true;
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
        this.toastr.success('All notifications marked as read');
      }
    });
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
      'Updated': '✎'
    };
    return icons[action] || '•';
  }

  logout() {
    this.authService.logout();
  }
}