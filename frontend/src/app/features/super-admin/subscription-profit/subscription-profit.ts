import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { LayoutComponent } from '../../../layouts/main-layout/layout';
import { environment } from '../../../../environments/environment';

interface Money { currency: string; amount: number; }
interface Company { id: string; name: string; isActive: boolean; totalPaid: Money[]; subscription: any | null; }
interface ProfitOverview { approvedRevenue: Money[]; approvedPaymentCount: number; subscribedCompanyCount: number; companies: Company[]; }

@Component({
  selector: 'app-subscription-profit',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, LayoutComponent],
  templateUrl: './subscription-profit.html',
  styleUrls: ['./subscription-profit.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubscriptionProfitComponent {
  private readonly http = inject(HttpClient);
  readonly loading = signal(true);
  readonly error = signal(false);
  readonly overview = signal<ProfitOverview | null>(null);

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true); this.error.set(false);
    this.http.get<ProfitOverview>(`${environment.apiUrl}/SuperAdmin/subscription-profit`).subscribe({
      next: value => { this.overview.set(value); this.loading.set(false); },
      error: () => { this.error.set(true); this.loading.set(false); },
    });
  }
}
