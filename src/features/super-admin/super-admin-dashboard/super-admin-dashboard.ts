import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { SuperAdminService } from '../../../services/super-admin';
import { AuthService } from '../../../app/services/auth.service';

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatCardModule, MatProgressSpinnerModule
  ],
  templateUrl: './super-admin-dashboard.html',
  styleUrls: ['./super-admin-dashboard.scss']
})
export class SuperAdminDashboardComponent implements OnInit {
  private superAdminService = inject(SuperAdminService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  loading = true;
  stats: any = null;

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading = true;
    this.superAdminService.getStats().subscribe({
      next: (data: any) => {
        this.stats = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load stats');
        this.cdr.detectChanges();
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}