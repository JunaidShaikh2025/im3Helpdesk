import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../features/auth/auth.service';

/**
 * Allows CompanyAdmin and SuperAdmin only.
 * Non-admin agents are redirected to the dashboard
 */
export const companyAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const role = authService.getUserRole();
  if (role !== 'CompanyAdmin' && role !== 'SuperAdmin') {
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};
