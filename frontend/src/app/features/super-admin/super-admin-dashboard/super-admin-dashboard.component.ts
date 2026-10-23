import { Component, OnInit, ChangeDetectorRef, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SuperAdminService } from '../../../core/services/super-admin.service';
import { LayoutComponent } from '../../../layouts/main-layout/layout.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';


@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LayoutComponent
  ],
  templateUrl: './super-admin-dashboard.html',
  styleUrls: ['./super-admin-dashboard.scss']
})
export class SuperAdminDashboardComponent implements OnInit {
  trackById = (_: number, item: any): any =>
    item?.id ?? item?.ticketNumber ?? item?.key ?? item?.name ?? item?.code ?? item?.value ?? item ?? _;
  trackByIndex = (_: number): number => _;
  private destroyRef = inject(DestroyRef);
  private superAdminService = inject(SuperAdminService);
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
    this.superAdminService.getStats().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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
}