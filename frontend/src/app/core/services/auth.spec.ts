import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';

import { AuthService } from '../../features/auth/auth.service';
import {
  REFRESH_TOKEN_KEY,
  TOKEN_KEY
} from '../constants/auth.constants';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
        provideToastr()
      ]
    });
    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return token using TOKEN_KEY', () => {
    localStorage.setItem(TOKEN_KEY, 'abc123');

    expect(service.getToken()).toBe('abc123');
  });

  it('should mark expired token as invalid', () => {
    const expiredPayload = btoa(JSON.stringify({ exp: 1 }));
    const token = `header.${expiredPayload}.sig`;
    localStorage.setItem(TOKEN_KEY, token);

    expect(service.isTokenExpired()).toBe(true);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('logout should clear auth storage keys', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    localStorage.setItem(TOKEN_KEY, 'token-value');
    localStorage.setItem(REFRESH_TOKEN_KEY, 'refresh-value');
    localStorage.setItem('im3_role', 'Agent');

    service.logout();

    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(localStorage.getItem(REFRESH_TOKEN_KEY)).toBeNull();
    expect(localStorage.getItem('im3_role')).toBeNull();
  });
});
