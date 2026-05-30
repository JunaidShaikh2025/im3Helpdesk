import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LayoutComponent } from '../../../layouts/main-layout/layout';
import { PlanInfo, SubscriptionService } from '../../../core/services/subscription';
import { PLANS, Plan, PlanCategory, PlanFeature, PlanTier } from './plans.data';

interface PaymentForm {
  agentSeats: number;
  billingCycle: 'Monthly' | 'Annual';
  billingName: string;
  billingEmail: string;
  billingAddress: string;
  cardLast4: string;
  cardBrand: string;
  notes: string;
}

@Component({
  selector: 'app-explore-plans',
  standalone: true,
  imports: [CommonModule, FormsModule, LayoutComponent],
  templateUrl: './explore-plans.html',
  styleUrls: ['./explore-plans.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExplorePlansComponent {
  private readonly subSvc = inject(SubscriptionService);
  private readonly router = inject(Router);

  /** Minimum agents a plan can be purchased for (Freshdesk-style baseline). */
  readonly MIN_SEATS = 10;

  readonly plans = PLANS;
  readonly backendPlans = signal<Record<string, PlanInfo>>({});

  readonly currentTier = computed<PlanTier>(() => {
    const k = this.subSvc.tierKey();
    if (k === 'growth' || k === 'pro' || k === 'enterprise') return k;
    return 'growth';
  });

  readonly selectedTier  = signal<PlanTier>('growth');
  readonly expandedCats  = signal<Record<string, boolean>>({ respond: true });
  readonly selectedKey   = signal<string>('');

  readonly currentPlan = computed<Plan>(
    () => this.plans.find(p => p.tier === this.selectedTier())!
  );

  readonly currentFeature = computed<PlanFeature | undefined>(() => {
    const plan = this.currentPlan();
    const key = this.selectedKey();
    for (const c of plan.categories) {
      const hit = c.features.find(f => f.key === key);
      if (hit) return hit;
    }
    // Fallback: first feature of first category
    return plan.categories[0]?.features[0];
  });

  readonly backendForSelected = computed<PlanInfo | null>(
    () => this.backendPlans()[this.selectedTier()] ?? null
  );

  // ── Payment dialog state ─────────────────────────────────────────
  readonly showPayment = signal(false);
  readonly paymentSaving = signal(false);
  readonly paymentError = signal<string | null>(null);
  readonly paymentSuccess = signal<string | null>(null);
  readonly payment = signal<PaymentForm>({
    agentSeats: 10,
    billingCycle: 'Monthly',
    billingName: '',
    billingEmail: '',
    billingAddress: '',
    cardLast4: '',
    cardBrand: 'Visa',
    notes: '',
  });

  /** Per-seat unit price for the selected plan in the chosen billing cycle. */
  readonly unitPrice = computed(() => {
    const p = this.backendForSelected();
    const f = this.payment();
    if (!p) return 0;
    if (f.billingCycle === 'Annual') {
      return Math.round(p.monthlyPricePerAgent * 12 * (1 - (p.annualDiscountPct ?? 0) / 100));
    }
    return Math.round(p.monthlyPricePerAgent);
  });

  /** Multiplier label shown in the breakdown (1 month or 12 months). */
  readonly periodMonths = computed(() => this.payment().billingCycle === 'Annual' ? 12 : 1);

  /** Display "billed on today" date. */
  today(): Date { return new Date(); }

  /** Period-end date for the breakdown (today + 1 month or + 1 year). */
  nextBillDate(): Date {
    const d = new Date();
    if (this.payment().billingCycle === 'Annual') d.setFullYear(d.getFullYear() + 1);
    else d.setMonth(d.getMonth() + 1);
    return d;
  }

  readonly paymentAmount = computed(() => {
    const p = this.backendForSelected();
    const f = this.payment();
    if (!p || f.agentSeats < this.MIN_SEATS) return 0;
    if (f.billingCycle === 'Annual') {
      return Math.round(p.monthlyPricePerAgent * 12 * (1 - (p.annualDiscountPct ?? 0) / 100) * f.agentSeats);
    }
    return Math.round(p.monthlyPricePerAgent * f.agentSeats);
  });

  constructor() {
    const first = this.plans[0].categories[0]?.features[0];
    if (first) this.selectedKey.set(first.key);

    // Once the org's actual subscription loads, jump to that tier — but
    // only on the very first load. Otherwise the effect would snap the
    // user back to their current tier every time they click another tab.
    let initialTierApplied = false;
    effect(() => {
      if (!this.subSvc.loaded() || initialTierApplied) return;
      initialTierApplied = true;
      const tier = this.currentTier();
      if (tier !== this.selectedTier()) {
        this.selectedTier.set(tier);
      }
    });

    this.subSvc.ensureLoaded().subscribe({ error: () => void 0 });
    this.subSvc.getPlans().subscribe({
      next: list => {
        const map: Record<string, PlanInfo> = {};
        for (const p of list) map[p.tierKey] = p;
        this.backendPlans.set(map);
      },
      error: () => void 0,
    });
  }

  selectTier(tier: PlanTier): void {
    this.selectedTier.set(tier);
    // Reset expansion and selection to the first feature of the new plan
    const plan = this.plans.find(p => p.tier === tier)!;
    const firstCat = plan.categories[0];
    this.expandedCats.set({ [firstCat.key]: true });
    this.selectedKey.set(firstCat.features[0]?.key ?? '');
  }

  toggleCategory(catKey: string): void {
    const cur = this.expandedCats();
    this.expandedCats.set({ ...cur, [catKey]: !cur[catKey] });
  }

  selectFeature(cat: PlanCategory, f: PlanFeature): void {
    // Ensure category is open when a feature inside it is picked
    const cur = this.expandedCats();
    if (!cur[cat.key]) this.expandedCats.set({ ...cur, [cat.key]: true });
    this.selectedKey.set(f.key);
  }

  isCurrent(tier: PlanTier): boolean {
    return tier === this.currentTier();
  }

  canUpgradeTo(tier: PlanTier): boolean {
    const rank: Record<PlanTier, number> = { growth: 0, pro: 1, enterprise: 2 };
    return rank[tier] > rank[this.currentTier()];
  }

  /** Selected tier is BELOW current tier — downgrade requested via support. */
  isDowngrade(tier: PlanTier): boolean {
    const rank: Record<PlanTier, number> = { growth: 0, pro: 1, enterprise: 2 };
    return rank[tier] < rank[this.currentTier()];
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // ── Payment dialog ───────────────────────────────────────────────
  openPayment(): void {
    const p = this.backendForSelected();
    if (!p) return;
    const currentSeats = this.subSvc.subscription()?.agentSeats ?? 0;
    this.payment.set({
      agentSeats: Math.max(this.MIN_SEATS, currentSeats),
      billingCycle: 'Monthly',
      billingName: '',
      billingEmail: '',
      billingAddress: '',
      cardLast4: '',
      cardBrand: 'Visa',
      notes: '',
    });
    this.paymentError.set(null);
    this.paymentSuccess.set(null);
    this.showPayment.set(true);
  }

  closePayment(): void {
    if (this.paymentSaving()) return;
    this.showPayment.set(false);
  }

  updatePayment<K extends keyof PaymentForm>(key: K, value: PaymentForm[K]): void {
    this.payment.update(p => ({ ...p, [key]: value }));
  }

  submitPayment(): void {
    const p = this.backendForSelected();
    const f = this.payment();
    if (!p) return;
    if (f.agentSeats < this.MIN_SEATS) {
      this.paymentError.set(`All plans start at a minimum of ${this.MIN_SEATS} agent seats.`);
      return;
    }
    if (!/^\d{4}$/.test(f.cardLast4)) { this.paymentError.set('Enter the last 4 digits of your card'); return; }
    if (!f.billingName.trim() || !f.billingEmail.trim()) {
      this.paymentError.set('Billing name and email are required');
      return;
    }

    this.paymentSaving.set(true);
    this.paymentError.set(null);
    this.subSvc.submitPayment({
      planId: p.id,
      billingCycle: f.billingCycle,
      agentSeats: f.agentSeats,
      billingName: f.billingName,
      billingEmail: f.billingEmail,
      billingAddress: f.billingAddress,
      cardLast4: f.cardLast4,
      cardBrand: f.cardBrand,
      notes: f.notes,
    }).subscribe({
      next: res => {
        this.paymentSaving.set(false);
        this.paymentSuccess.set(res.message || 'Payment submitted for approval.');
      },
      error: err => {
        this.paymentSaving.set(false);
        this.paymentError.set(err?.error?.error || err?.message || 'Submission failed');
      },
    });
  }
}
