import { Component, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SubscriptionService } from '../../core/services/subscription';

@Component({
  selector: 'app-trial-expired',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  templateUrl: './trial-expired.html',
  styleUrls: ['./trial-expired.scss']
})
export class TrialExpiredComponent implements OnInit {
  private router  = inject(Router);
  readonly sub    = inject(SubscriptionService);
  private cdr     = inject(ChangeDetectorRef);

  planName  = '';
  trialDays = 14;

  ngOnInit() {
    this.sub.ensureLoaded().subscribe({
      next: () => {
        this.planName = this.sub.plan()?.name ?? '';
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  goToPlans() {
    this.router.navigate(['/explore-plans']);
  }
}
