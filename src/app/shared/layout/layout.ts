import { Component, OnInit, OnDestroy, Input, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subject, interval, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { GlobalSearchComponent } from '../global-search/global-search';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, GlobalSearchComponent],
  templateUrl: './layout.html',
  styleUrls: ['./layout.scss']
})
export class LayoutComponent implements OnInit, OnDestroy {
  @Input() activeRoute = '';

  private authService = inject(AuthService);
  public router = inject(Router);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();

  userName = '';
  userEmail = '';
  userRole = '';
  userInitials = '';
  userPhotoUrl = ''; // Added photo URL variable
  unreadCount = 0;
  showNotifDropdown = false;
  notifications: any[] = [];

    ngOnInit() {
      this.loadUserInfo();
      this.loadNotifications();
      // this.startNotifPolling();
    }

    loadUserInfo() {
      const token = this.authService.getToken();
      if (!token) return;

      try {
        const payload = JSON.parse(
          atob(token.split('.')[1]));
        this.userName = payload[
          'http://schemas.xmlsoap.org/ws/2005/' +
          '05/identity/claims/name'
        ] || payload.email?.split('@')[0] || 'User';
        this.userEmail = payload.email || '';
        this.userRole = payload[
          'http://schemas.microsoft.com/ws/2008/' +
          '06/identity/claims/role'
        ] || payload.role || '';
        this.userInitials = this.userName
          .split(' ')
          .map((n: string) => n[0] || '')
          .join('')
          .toUpperCase()
          .slice(0, 2);
      } catch {}

      // Load photo from localStorage first (instant)
      const saved = localStorage.getItem('im3_photo');
      if (saved) {
        this.userPhotoUrl = saved.startsWith('http')
          ? saved
          : 'https://localhost:7071' + saved;
        this.cdr.detectChanges();
      }

      // Then load fresh from API
      this.http.get<any>(
        'https://localhost:7071/api/Profile',
        { headers: this.getHeaders() }
      ).subscribe({
        next: (data) => {
          if (data.photoUrl) {
            this.userPhotoUrl =
              'https://localhost:7071' + data.photoUrl;
            localStorage.setItem(
              'im3_photo', data.photoUrl);
            this.cdr.detectChanges();
          }
        }
      });
    }

    private getHeaders() {
      return new HttpHeaders({
        'Authorization':
          `Bearer ${this.authService.getToken()}`
      });
    }


  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUserPhoto() {
    this.http.get<any>(
      'https://localhost:7071/api/Profile',
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        if (data.photoUrl) {
          this.userPhotoUrl = 'https://localhost:7071' + data.photoUrl;
          localStorage.setItem('im3_photo', data.photoUrl);
          this.cdr.detectChanges();
        }
      }
    });
  }

  loadUnreadCount() {
    this.http.get<any>(
      'https://localhost:7071/api/Notifications/unread-count',
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.unreadCount = data.count || 0;
        this.cdr.detectChanges();
      }
    });
  }

  toggleNotifDropdown() {
    this.showNotifDropdown = !this.showNotifDropdown;
    if (this.showNotifDropdown) this.loadNotifications();
  }

  loadNotifications() {
    this.http.get<any[]>(
      'https://localhost:7071/api/Notifications',
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.notifications = data.slice(0, 8);
        this.cdr.detectChanges();
      }
    });
  }

    goToNotification(n: any) {
      this.showNotifDropdown = false;

      // Mark as read
      this.http.put(
        `https://localhost:7071/api/Notifications/${n.id}/read`,
        {}, { headers: this.getHeaders() }
      ).subscribe({
        next: () => {
          const notif = this.notifications
            .find(x => x.id === n.id);
          if (notif) notif.isRead = true;
          this.unreadCount = Math.max(0,
            this.unreadCount - 1);
          this.cdr.detectChanges();
        }
      });

      // Smart redirect
      Promise.resolve().then(() => {
        if (n.ticketId) {
          this.router.navigate(['/tickets', n.ticketId]);
          return;
        }

        const title = (n.title || '').toLowerCase();
        const msg = (n.message || '').toLowerCase();

        if (title.includes('ticket') ||
            msg.includes('ticket')) {
          this.router.navigate(['/tickets']);
        } else if (title.includes('agent') ||
            title.includes('invited')) {
          this.router.navigate(['/agents']);
        } else if (title.includes('sla') ||
            title.includes('escalat') ||
            title.includes('breach')) {
          this.router.navigate(['/tickets']);
        } else if (title.includes('kb') ||
            title.includes('article')) {
          this.router.navigate(['/kb']);
        } else {
          this.router.navigate(['/notifications']);
        }
      });
    }

  viewAllNotifications() {
    this.showNotifDropdown = false;
    this.router.navigate(['/notifications']);
  }

  getTypeColor(type: string): string {
    const c: any = {
      'info': '#3b82f6', 'success': '#22c55e',
      'warning': '#f59e0b', 'error': '#ef4444'
    };
    return c[type] || '#3b82f6';
  }

  isAdmin(): boolean {
    return this.userRole === 'CompanyAdmin' || this.userRole === 'SuperAdmin';
  }

  logout() {
    localStorage.removeItem('im3_photo'); // Cleanup photo on logout
    this.authService.logout();
  }
}