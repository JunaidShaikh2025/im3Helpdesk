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
  readonly activeTab = signal<'plan' | 'features' | 'payments'>('plan');

  readonly featureDetails = computed(() => this.features().map((name) => ({
    name,
    description: this.featureDescription(name),
    icon: this.featureIcon(name),
  })));

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

  readonly billingProgress = computed(() => {
    const sub = this.subscription();
    if (!sub) return 0;
    const start = new Date(sub.startedAt).getTime();
    const end = new Date(sub.currentPeriodEnd).getTime();
    const total = end - start;
    if (!Number.isFinite(total) || total <= 0) return 0;
    return Math.min(100, Math.max(0, ((Date.now() - start) / total) * 100));
  });

  readonly remainingDaysLabel = computed(() => {
    const sub = this.subscription();
    if (!sub) return '';
    const days = Math.max(0, Math.ceil((new Date(sub.currentPeriodEnd).getTime() - Date.now()) / 86_400_000));
    return days === 0 ? 'Renews today' : `${days} day${days === 1 ? '' : 's'} left`;
  });

  featureDescription(feature: string): string {
    const value = feature.toLowerCase();
    if (value.includes('agent') || value.includes('seat')) return 'Give the right people access and scale your team as it grows.';
    if (value.includes('report') || value.includes('analytic')) return 'Track service performance with clear, actionable insights.';
    if (value.includes('automation') || value.includes('workflow')) return 'Reduce repetitive work with rules that keep tickets moving.';
    if (value.includes('support') || value.includes('priority')) return 'Get faster help from our team when you need it.';
    if (value.includes('api') || value.includes('integration')) return 'Connect your helpdesk with the tools your team already uses.';
    if (value.includes('security') || value.includes('sso')) return 'Keep access controlled with enterprise-grade safeguards.';
    return 'Included with your current plan and ready for your team to use.';
  }

  featureIcon(feature: string): string {
    const value = feature.toLowerCase();
    if (value.includes('dashboard')) return 'fa-table-cells-large';
    if (value.includes('ticket')) return 'fa-ticket';
    if (value.includes('contact')) return 'fa-address-book';
    if (value.includes('todo')) return 'fa-circle-check';
    if (value.includes('note')) return 'fa-note-sticky';
    if (value.includes('calendar')) return 'fa-calendar-days';
    if (value.includes('knowledge')) return 'fa-book';
    if (value.includes('chat')) return 'fa-comments';
    if (value.includes('search')) return 'fa-magnifying-glass';
    if (value.includes('profile') || value.includes('agent')) return 'fa-user-group';
    if (value.includes('notification')) return 'fa-bell';
    if (value.includes('setting')) return 'fa-gear';
    if (value.includes('email')) return 'fa-envelope';
    if (value.includes('response')) return 'fa-reply';
    if (value.includes('analytics') || value.includes('report')) return 'fa-chart-column';
    if (value.includes('insight')) return 'fa-lightbulb';
    if (value.includes('sla')) return 'fa-stopwatch';
    if (value.includes('audit')) return 'fa-clipboard-list';
    if (value.includes('whatsapp')) return 'fa-comment-dots';
    if (value.includes('custom-field')) return 'fa-list-check';
    if (value.includes('business-hour')) return 'fa-clock';
    if (value.includes('template')) return 'fa-file-lines';
    if (value.includes('holiday')) return 'fa-umbrella-beach';
    if (value.includes('mateboard')) return 'fa-table-columns';
    if (value.includes('recycle') || value.includes('bin')) return 'fa-trash-can';
    if (value.includes('role') || value.includes('right')) return 'fa-user-shield';
    if (value.includes('organization') || value.includes('org')) return 'fa-building';
    if (value.includes('call-log')) return 'fa-phone';
    if (value.includes('slack')) return 'fa-hashtag';
    if (value.includes('portal')) return 'fa-door-open';
    if (value.includes('sso')) return 'fa-key';
    if (value.includes('lead')) return 'fa-bullseye';
    return 'fa-circle-check';
    if (value.includes('report') || value.includes('analytic')) return '↗';
    if (value.includes('automation') || value.includes('workflow')) return '⚡';
    if (value.includes('support') || value.includes('priority')) return '✦';
    if (value.includes('security') || value.includes('sso')) return '⌁';
    return '✓';
  }

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
