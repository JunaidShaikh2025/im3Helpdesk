import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SuperAdminService } from '../../../core/services/super-admin';
import { LayoutComponent } from '../../../layouts/main-layout/layout';

@Component({
  selector: 'app-email-queue-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, LayoutComponent],
  templateUrl: './email-queue-dashboard.html',
  styleUrls: ['./email-queue-dashboard.scss']
})
export class EmailQueueDashboardComponent implements OnInit {
  private readonly superAdminService = inject(SuperAdminService);
  private readonly toastr = inject(ToastrService);
  private readonly cdr = inject(ChangeDetectorRef);

  loading = true;
  actionBusyId = '';

  selectedStatus = '';
  statusOptions = ['Queued', 'Processing', 'Sent', 'Delivered', 'Opened', 'Failed', 'Bounced', 'Cancelled'];

  page = 1;
  pageSize = 20;
  webhookPage = 1;
  webhookPageSize = 10;

  kpis = {
    totalQueued: 0,
    totalSent: 0,
    totalFailed: 0,
    totalBounced: 0
  };

  emailLogs: any[] = [];
  emailTotalItems = 0;
  emailTotalPages = 1;

  webhookLogs: any[] = [];
  webhookTotalItems = 0;
  webhookTotalPages = 1;

  ngOnInit(): void {
    setTimeout(() => this.loadData());
  }

  loadData(): void {
    this.loading = true;
    this.superAdminService.getEmailQueueOverview({
      page: this.page,
      pageSize: this.pageSize,
      status: this.selectedStatus || undefined,
      webhookPage: this.webhookPage,
      webhookPageSize: this.webhookPageSize
    }).subscribe({
      next: (res) => {
        queueMicrotask(() => {
          this.kpis = res?.kpis || this.kpis;

          this.emailLogs = res?.emailLogs?.items || [];
          this.emailTotalItems = res?.emailLogs?.totalItems || 0;
          this.emailTotalPages = Math.max(1, res?.emailLogs?.totalPages || 1);

          this.webhookLogs = res?.webhookLogs?.items || [];
          this.webhookTotalItems = res?.webhookLogs?.totalItems || 0;
          this.webhookTotalPages = Math.max(1, res?.webhookLogs?.totalPages || 1);

          this.loading = false;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        queueMicrotask(() => {
          this.loading = false;
          this.toastr.error('Failed to load email queue dashboard');
          this.cdr.detectChanges();
        });
      }
    });
  }

  applyStatusFilter(): void {
    this.page = 1;
    this.loadData();
  }

  retryEmail(item: any): void {
    this.actionBusyId = item.id;
    this.superAdminService.retryQueuedEmail(item.id).subscribe({
      next: () => {
        this.toastr.success('Email queued for retry');
        this.actionBusyId = '';
        this.loadData();
      },
      error: () => {
        this.actionBusyId = '';
        this.toastr.error('Failed to retry email');
      }
    });
  }

  cancelEmail(item: any): void {
    this.actionBusyId = item.id;
    this.superAdminService.cancelQueuedEmail(item.id).subscribe({
      next: () => {
        this.toastr.success('Queued email cancelled');
        this.actionBusyId = '';
        this.loadData();
      },
      error: () => {
        this.actionBusyId = '';
        this.toastr.error('Failed to cancel queued email');
      }
    });
  }

  nextEmailPage(): void {
    if (this.page >= this.emailTotalPages) return;
    this.page++;
    this.loadData();
  }

  prevEmailPage(): void {
    if (this.page <= 1) return;
    this.page--;
    this.loadData();
  }

  nextWebhookPage(): void {
    if (this.webhookPage >= this.webhookTotalPages) return;
    this.webhookPage++;
    this.loadData();
  }

  prevWebhookPage(): void {
    if (this.webhookPage <= 1) return;
    this.webhookPage--;
    this.loadData();
  }

  statusClass(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'queued' || s === 'processing') return 's-queued';
    if (s === 'sent' || s === 'delivered' || s === 'opened') return 's-sent';
    if (s === 'failed') return 's-failed';
    if (s === 'bounced') return 's-bounced';
    if (s === 'cancelled') return 's-cancelled';
    return 's-default';
  }

  canRetry(status: string): boolean {
    return ['Failed', 'Bounced', 'Cancelled'].includes(status);
  }

  canCancel(status: string): boolean {
    return ['Queued', 'Processing'].includes(status);
  }
}
