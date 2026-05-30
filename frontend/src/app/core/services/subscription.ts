import { HttpClient } from '@angular/common/http';
import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PlanInfo {
  id: string;
  tierKey: 'growth' | 'pro' | 'enterprise' | string;
  name: string;
  tagline: string;
  accent: string;
  currency: string;
  monthlyPricePerAgent: number;
  annualDiscountPct: number;
  annualPricePerAgent?: number;
  featureKeys?: string[];
}

export interface SubscriptionInfo {
  id: string;
  planId: string;
  status: 'Trial' | 'Active' | 'PastDue' | 'Expired' | 'Cancelled' | string;
  billingCycle: 'Monthly' | 'Annual' | string;
  agentSeats: number;
  amount: number;
  currency: string;
  startedAt: string;
  currentPeriodEnd: string;
  daysRemaining: number;
  isTrial: boolean;
}

export interface MySubscriptionResponse {
  subscription: SubscriptionInfo | null;
  plan: PlanInfo | null;
  features: string[];
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/subscription`;

  private readonly _features = signal<Set<string>>(new Set());
  private readonly _subscription = signal<SubscriptionInfo | null>(null);
  private readonly _plan = signal<PlanInfo | null>(null);
  private readonly _loaded = signal(false);

  readonly features = computed(() => Array.from(this._features()));
  readonly subscription = this._subscription.asReadonly();
  readonly plan = this._plan.asReadonly();
  readonly loaded = this._loaded.asReadonly();
  readonly tierKey = computed(() => this._plan()?.tierKey ?? null);

  /** Synchronous gate used by *hasFeature directive and feature guards. */
  hasFeature(key: string): boolean {
    return this._features().has(key);
  }

  /** Refresh the cached subscription/feature list from the API. */
  loadMine(): Observable<MySubscriptionResponse> {
    return this.http.get<MySubscriptionResponse>(`${this.base}/me`).pipe(
      tap(res => {
        this._subscription.set(res.subscription);
        this._plan.set(res.plan);
        this._features.set(new Set((res.features || []).map(f => f.toLowerCase())));
        this._loaded.set(true);
      }),
    );
  }

  /** Ensure features are loaded (call once at app bootstrap or guard entry). */
  ensureLoaded(): Observable<MySubscriptionResponse | null> {
    if (this._loaded()) return of(null);
    return this.loadMine();
  }

  getPlans(): Observable<PlanInfo[]> {
    return this.http.get<PlanInfo[]>(`${this.base}/plans`);
  }

  submitPayment(body: {
    planId: string;
    billingCycle: 'Monthly' | 'Annual';
    agentSeats: number;
    billingName?: string;
    billingEmail?: string;
    billingAddress?: string;
    cardLast4?: string;
    cardBrand?: string;
    notes?: string;
  }): Observable<{ id: string; status: string; amount: number; currency: string; message: string }> {
    return this.http.post<any>(`${this.base}/payments`, body);
  }

  myPayments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/payments`);
  }

  clear(): void {
    this._features.set(new Set());
    this._subscription.set(null);
    this._plan.set(null);
    this._loaded.set(false);
  }
}
