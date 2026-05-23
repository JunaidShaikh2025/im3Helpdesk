import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { AuthService } from '../../features/auth/auth.service';

import { authGuard } from './auth.guard';

describe('authGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => authGuard(...guardParameters));

  const authServiceMock = {
    isAuthenticated: vi.fn()
  };

  beforeEach(() => {
    authServiceMock.isAuthenticated.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
        provideToastr(),
        { provide: AuthService, useValue: authServiceMock }
      ]
    });
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });

  it('should allow when authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);

    const result = executeGuard({} as any, {} as any);
    expect(result).toBe(true);
  });

  it('should redirect when token is missing or invalid', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);

    const result = executeGuard({} as any, {} as any);
    expect((result as any)?.toString?.()).toContain('/auth/login');
  });
});
