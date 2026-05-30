import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LayoutComponent } from '../../../layouts/main-layout/layout';
import { SubscriptionService } from '../../../core/services/subscription';

@Component({
  selector: 'app-plans-billing',
  standalone: true,
  imports: [CommonModule, LayoutComponent],
  templateUrl: './plans-billing.html',
  styleUrls: ['./plans-billing.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlansBillingComponent {
  private readonly subSvc = inject(SubscriptionService);
  private readonly router = inject(Router);

  /** Freshdesk-style baseline: all plans are sold in 10-agent units. */
  readonly MIN_SEATS = 10;

  readonly subscription = this.subSvc.subscription;
  readonly plan = this.subSvc.plan;
  readonly features = computed(() => this.subSvc.features());
  readonly loaded = this.subSvc.loaded;

  readonly payments = signal<any[]>([]);

  /** Display label for the cycle. */
  readonly cycleLabel = computed(() => {
    const s = this.subscription();
    if (!s) return '';
    return s.billingCycle === 'Annual' ? 'Annual' : 'Monthly';
  });

  readonly nextBillLabel = computed(() => {
    const s = this.subscription();
    if (!s) return '';
    return new Date(s.currentPeriodEnd).toLocaleDateString();
  });

  // ── Manage subscription dialog ─────────────────────────────────
  readonly showManage   = signal(false);
  readonly manageSeats  = signal(10);
  readonly manageCycle  = signal<'Monthly' | 'Annual'>('Monthly');
  readonly manageNotes  = signal('');
  readonly manageBusy   = signal(false);
  readonly manageError  = signal<string | null>(null);
  readonly manageOk     = signal<string | null>(null);

  readonly manageUnitPrice = computed(() => {
    const p = this.plan();
    if (!p) return 0;
    return this.manageCycle() === 'Annual'
      ? Math.round(p.monthlyPricePerAgent * 12 * (1 - (p.annualDiscountPct ?? 0) / 100))
      : Math.round(p.monthlyPricePerAgent);
  });

  readonly manageMonths = computed(() => this.manageCycle() === 'Annual' ? 12 : 1);

  readonly manageTotal = computed(() => {
    const p = this.plan();
    const seats = this.manageSeats();
    if (!p || seats < this.MIN_SEATS) return 0;
    return this.manageCycle() === 'Annual'
      ? Math.round(p.monthlyPricePerAgent * 12 * (1 - (p.annualDiscountPct ?? 0) / 100) * seats)
      : Math.round(p.monthlyPricePerAgent * seats);
  });

  today(): Date { return new Date(); }
  manageNextDate(): Date {
    const d = new Date();
    if (this.manageCycle() === 'Annual') d.setFullYear(d.getFullYear() + 1);
    else d.setMonth(d.getMonth() + 1);
    return d;
  }

  openManage(): void {
    const s = this.subscription();
    if (!s) return;
    this.manageSeats.set(Math.max(this.MIN_SEATS, s.agentSeats || this.MIN_SEATS));
    this.manageCycle.set(s.billingCycle === 'Annual' ? 'Annual' : 'Monthly');
    this.manageNotes.set('');
    this.manageError.set(null);
    this.manageOk.set(null);
    this.showManage.set(true);
  }

  closeManage(): void { if (!this.manageBusy()) this.showManage.set(false); }

  submitManage(): void {
    const sub = this.subscription();
    const p = this.plan();
    if (!sub || !p) return;
    const seats = this.manageSeats();
    if (seats < this.MIN_SEATS) {
      this.manageError.set(`Minimum ${this.MIN_SEATS} agent seats per plan.`);
      return;
    }
    this.manageBusy.set(true);
    this.manageError.set(null);
    this.subSvc.submitPayment({
      planId: sub.planId,
      billingCycle: this.manageCycle(),
      agentSeats: seats,
      notes: this.manageNotes() || 'Manage subscription request',
    }).subscribe({
      next: () => {
        this.manageBusy.set(false);
        this.manageOk.set('Request sent to SuperAdmin for approval.');
        this.subSvc.myPayments().subscribe({
          next: rows => this.payments.set(rows),
          error: () => void 0,
        });
      },
      error: (e) => {
        this.manageBusy.set(false);
        this.manageError.set(e?.error?.error || e?.error?.message || 'Failed to submit request.');
      },
    });
  }

  constructor() {
    this.subSvc.ensureLoaded().subscribe({ error: () => void 0 });
    this.subSvc.myPayments().subscribe({
      next: rows => this.payments.set(rows),
      error: () => this.payments.set([]),
    });
  }

  goExplore(): void {
    this.router.navigate(['/explore-plans']);
  }

  statusColor(status: string): string {
    switch (status) {
      case 'Active': return '#16a34a';
      case 'Trial': return '#0ea5e9';
      case 'PastDue': return '#f59e0b';
      case 'Expired':
      case 'Cancelled': return '#dc2626';
      case 'Approved': return '#16a34a';
      case 'Pending': return '#f59e0b';
      case 'Rejected': return '#dc2626';
      default: return '#64748b';
    }
  }
}
