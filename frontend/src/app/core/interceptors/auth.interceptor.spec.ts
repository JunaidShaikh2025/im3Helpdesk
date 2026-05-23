import {
  HttpClient,
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { AuthService } from '../../features/auth/auth.service';
import { authInterceptor } from './auth.interceptor';
import { TOKEN_KEY } from '../constants/auth.constants';
import { environment } from '../../../environments/environment';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  const authServiceMock = {
    refreshToken: vi.fn(),
    getToken: vi.fn(),
    logout: vi.fn()
  };

  beforeEach(() => {
    localStorage.clear();
    authServiceMock.refreshToken.mockReset();
    authServiceMock.getToken.mockReset();
    authServiceMock.logout.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceMock }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should attach bearer token for protected API requests', () => {
    localStorage.setItem(TOKEN_KEY, 'token-123');

    http.get(`${environment.apiUrl}/Todo`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Todo`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-123');
    req.flush([]);
  });

  it('should refresh and retry once on 401', () => {
    localStorage.setItem(TOKEN_KEY, 'expired-token');

    authServiceMock.refreshToken.mockImplementation(() => {
      localStorage.setItem(TOKEN_KEY, 'new-token');
      return of({ token: 'new-token' });
    });
    authServiceMock.getToken.mockReturnValue('new-token');

    http.get(`${environment.apiUrl}/Todo`).subscribe();

    const initial = httpMock.expectOne(`${environment.apiUrl}/Todo`);
    expect(initial.request.headers.get('Authorization')).toBe('Bearer expired-token');
    initial.flush({}, { status: 401, statusText: 'Unauthorized' });

    const retried = httpMock.expectOne(`${environment.apiUrl}/Todo`);
    expect(retried.request.headers.get('Authorization')).toBe('Bearer new-token');
    retried.flush([]);

    expect(authServiceMock.refreshToken).toHaveBeenCalled();
  });

  it('should logout if refresh fails', () => {
    localStorage.setItem(TOKEN_KEY, 'expired-token');
    authServiceMock.refreshToken.mockReturnValue(
      throwError(() => ({ status: 401 }))
    );

    http.get(`${environment.apiUrl}/Todo`).subscribe({
      error: () => undefined
    });

    const initial = httpMock.expectOne(`${environment.apiUrl}/Todo`);
    initial.flush({}, { status: 401, statusText: 'Unauthorized' });

    const retried = httpMock.match(`${environment.apiUrl}/Todo`);
    retried.forEach(req => {
      req.flush({}, { status: 401, statusText: 'Unauthorized' });
    });

    expect(authServiceMock.logout).toHaveBeenCalled();
  });
});
