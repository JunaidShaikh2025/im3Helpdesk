import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LayoutComponent } from '../../../layouts/main-layout/layout';
import { environment } from '../../../../environments/environment';

interface AdminPayment {
  id: string;
  organizationId: string;
  orgName: string;
  planId: string;
  planName: string;
  billingCycle: string;
  agentSeats: number;
  amount: number;
  currency: string;
  status: string;
  cardLast4?: string;
  cardBrand?: string;
  billingName?: string;
  billingEmail?: string;
  notes?: string;
  submittedAt: string;
  reviewedAt?: string;
  reviewNotes?: string;
}

@Component({
  selector: 'app-pending-payments',
  standalone: true,
  imports: [CommonModule, FormsModule, LayoutComponent],
  templateUrl: './pending-payments.html',
  styleUrls: ['./pending-payments.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PendingPaymentsComponent {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/superadmin/subscription`;

  readonly statusFilter = signal<string>('Pending');
  readonly payments = signal<AdminPayment[]>([]);
  readonly busyId = signal<string | null>(null);
  readonly toast = signal<string | null>(null);
  readonly reviewNotes = signal<Record<string, string>>({});

  readonly counts = computed(() => {
    const list = this.payments();
    return {
      total: list.length,
      pending: list.filter(p => p.status === 'Pending').length,
      approved: list.filter(p => p.status === 'Approved').length,
      rejected: list.filter(p => p.status === 'Rejected').length,
    };
  });

  constructor() { this.load(); }

  load(): void {
    const filter = this.statusFilter();
    const url = `${this.base}/payments` + (filter ? `?status=${filter}` : '');
    this.http.get<AdminPayment[]>(url).subscribe({
      next: list => this.payments.set(list),
      error: () => this.payments.set([]),
    });
  }

  setFilter(s: string): void {
    this.statusFilter.set(s);
    this.load();
  }

  setNotes(id: string, value: string): void {
    this.reviewNotes.update(m => ({ ...m, [id]: value }));
  }

  approve(p: AdminPayment): void {
    this.busyId.set(p.id);
    this.http.post(`${this.base}/payments/${p.id}/approve`, { notes: this.reviewNotes()[p.id] || '' })
      .subscribe({
        next: () => { this.busyId.set(null); this.flash(`Approved · ${p.orgName} → ${p.planName}`); this.load(); },
        error: () => { this.busyId.set(null); this.flash('Approval failed'); },
      });
  }

  reject(p: AdminPayment): void {
    if (!this.reviewNotes()[p.id]) { this.flash('Please add a rejection note'); return; }
    this.busyId.set(p.id);
    this.http.post(`${this.base}/payments/${p.id}/reject`, { notes: this.reviewNotes()[p.id] })
      .subscribe({
        next: () => { this.busyId.set(null); this.flash(`Rejected · ${p.orgName}`); this.load(); },
        error: () => { this.busyId.set(null); this.flash('Rejection failed'); },
      });
  }

  private flash(msg: string): void {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(null), 2500);
  }

  badge(status: string): string {
    switch (status) {
      case 'Approved': return '#16a34a';
      case 'Pending': return '#f59e0b';
      case 'Rejected': return '#dc2626';
      default: return '#64748b';
    }
  }
}
