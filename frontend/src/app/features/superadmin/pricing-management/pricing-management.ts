import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LayoutComponent } from '../../../layouts/main-layout/layout';
import { environment } from '../../../../environments/environment';

interface AdminPlan {
  id: string;
  tierKey: string;
  name: string;
  tagline: string;
  accent: string;
  currency: string;
  monthlyPricePerAgent: number;
  annualDiscountPct: number;
  annualPricePerAgent: number;
  featureKeys: string[];
  isActive: boolean;
  sortOrder: number;
}

@Component({
  selector: 'app-pricing-management',
  standalone: true,
  imports: [CommonModule, FormsModule, LayoutComponent],
  templateUrl: './pricing-management.html',
  styleUrls: ['./pricing-management.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PricingManagementComponent {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/superadmin/subscription`;

  readonly plans = signal<AdminPlan[]>([]);
  readonly saving = signal<Record<string, boolean>>({});
  readonly toast = signal<string | null>(null);
  readonly featuresInput = signal<Record<string, string>>({});

  constructor() { this.load(); }

  load(): void {
    this.http.get<AdminPlan[]>(`${this.base}/plans`).subscribe({
      next: list => {
        this.plans.set(list);
        const map: Record<string, string> = {};
        for (const p of list) map[p.id] = (p.featureKeys || []).join(', ');
        this.featuresInput.set(map);
      },
    });
  }

  update<K extends keyof AdminPlan>(id: string, key: K, value: AdminPlan[K]): void {
    this.plans.update(list => list.map(p => p.id === id ? { ...p, [key]: value } : p));
  }

  setFeatures(id: string, csv: string): void {
    this.featuresInput.update(m => ({ ...m, [id]: csv }));
  }

  save(plan: AdminPlan): void {
    this.saving.update(m => ({ ...m, [plan.id]: true }));
    const featuresCsv = this.featuresInput()[plan.id] || '';
    const featureKeys = featuresCsv.split(',').map(s => s.trim()).filter(Boolean);
    const payload = {
      name: plan.name,
      tagline: plan.tagline,
      accent: plan.accent,
      monthlyPricePerAgent: +plan.monthlyPricePerAgent,
      annualDiscountPct: +plan.annualDiscountPct,
      currency: plan.currency,
      isActive: plan.isActive,
      featureKeys,
    };
    this.http.put(`${this.base}/plans/${plan.id}`, payload).subscribe({
      next: () => {
        this.saving.update(m => ({ ...m, [plan.id]: false }));
        this.toast.set(`${plan.name} updated`);
        setTimeout(() => this.toast.set(null), 2500);
        this.load();
      },
      error: () => {
        this.saving.update(m => ({ ...m, [plan.id]: false }));
        this.toast.set('Save failed');
        setTimeout(() => this.toast.set(null), 2500);
      },
    });
  }
}
