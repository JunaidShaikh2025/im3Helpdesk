import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, switchMap } from 'rxjs';
import { SubscriptionService } from '../services/subscription';

/**
 * Route guard factory. Blocks navigation unless the org's active subscription
 * includes the given feature key; redirects to /explore-plans otherwise.
 *
 *   { path: 'reports', canActivate: [featureGuard('reports')], ... }
 */
export function featureGuard(key: string): CanActivateFn {
  return () => {
    const sub = inject(SubscriptionService);
    const router = inject(Router);
    const normalized = (key || '').toLowerCase();

    const decide = () => {
      if (sub.hasFeature(normalized)) return true;
      return router.createUrlTree(['/explore-plans'], {
        queryParams: { locked: normalized },
      });
    };

    if (sub.loaded()) return decide();
    return sub.ensureLoaded().pipe(
      switchMap(() => of(decide())),
      map(v => v),
    );
  };
}
