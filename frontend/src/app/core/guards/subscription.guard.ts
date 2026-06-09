import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, switchMap } from 'rxjs';
import { AuthService } from '../../features/auth/auth.service';
import { SubscriptionService } from '../services/subscription';

/**
 * Blocks access to all protected routes when an org's trial or subscription
 * has expired. SuperAdmin and Customer roles are exempt (no org subscription).
 *
 * Allowed without a paid plan:
 *   /trial-expired, /explore-plans, /plans-billing, /profile, /onboarding
 */
export const subscriptionGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const sub  = inject(SubscriptionService);
  const router = inject(Router);

  const role = auth.getUserRole();

  // SuperAdmin and Customer don't have org subscriptions — always allow
  if (role === 'SuperAdmin' || role === 'Customer') return true;

  // Routes allowed even when trial/subscription expired
  const allowedPaths = [
    '/trial-expired', '/explore-plans', '/plans-billing',
    '/profile', '/onboarding'
  ];
  if (allowedPaths.some(p => state.url.startsWith(p))) return true;

  const decide = () => {
    const s = sub.subscription();

    // No subscription row yet (brand-new org) — allow, trial row may be pending
    if (!s) return true;

    const now = new Date();
    const periodEnd = new Date(s.currentPeriodEnd);

    // Active subscription → always allow
    if (s.status === 'Active') return true;

    // Trial still running → allow
    if (s.status === 'Trial' && periodEnd > now) return true;

    // Trial expired or cancelled/expired → redirect to trial-expired
    return router.createUrlTree(['/trial-expired']);
  };

  if (sub.loaded()) return decide();

  return sub.ensureLoaded().pipe(
    switchMap(() => of(decide())),
    map(v => v)
  );
};
