import { Injectable, signal, DestroyRef, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  AUTH_IS_LOGGED_IN,
  AUTH_USER_KEY,
  REFRESH_TOKEN_KEY,
  TOKEN_KEY,
  TWO_FACTOR_ENABLED_KEY
} from '../constants/auth.constants';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export interface CurrentUser {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isTwoFactorEnabled?: boolean;
  organizationId?: string;
  organizationName?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private destroyRef = inject(DestroyRef);
  private apiUrl = `${environment.apiUrl}/Auth`;
  public currentUser = signal<CurrentUser | null>(null);
  private inMemoryAccessToken: string | null = null;

  constructor(private http: HttpClient, private router: Router) {
    this.hydrateUserFromStorage();
  }

  private hydrateUserFromStorage(): void {
    const raw = localStorage.getItem(AUTH_USER_KEY);
    if (raw) {
      try {
        const user = JSON.parse(raw);
        this.currentUser.set(user);
      } catch {
        localStorage.removeItem(AUTH_USER_KEY);
      }
    }
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data, {
      withCredentials: true
    });
  }

  login(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, data, {
      withCredentials: true
    }).pipe(
      tap(res => {
        if (!res?.requiresOtp) {
          this.saveUserData(res);
        }
      })
    );
  }

  verifyOtp(dto: { email: string; otp: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/verify-otp`, dto, {
      withCredentials: true
    }).pipe(
      tap(res => this.saveUserData(res))
    );
  }

  resendOtp(dto: { email: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/resend-otp`, dto, {
      withCredentials: true
    });
  }

  forgotPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, data, {
      withCredentials: true
    });
  }

  verifyEmail(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-email?token=${token}`, {}, {
      withCredentials: true
    });
  }

  registerCustomer(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-customer`, data, {
      withCredentials: true
    });
  }

  saveUserData(data: any): void {
    if (data?.user) {
      const u: CurrentUser = {
        id: String(data.user.id || data.user.Id || ''),
        fullName: data.user.fullName || '',
        email: data.user.email || '',
        role: data.user.role || '',
        isTwoFactorEnabled: data.user.isTwoFactorEnabled,
        organizationId: data.user.organizationId,
        organizationName: data.user.organizationName
      };

      this.currentUser.set(u);
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(u));
      localStorage.setItem(AUTH_IS_LOGGED_IN, 'true');
      localStorage.setItem('im3_userId', u.id);
      localStorage.setItem('im3_email', u.email);
      localStorage.setItem('im3_name', u.fullName);
      localStorage.setItem('im3_role', u.role);
      localStorage.setItem('im3_orgName', u.organizationName || '');
    }

    localStorage.setItem('im3_isFirstLogin', data?.isFirstLogin ? 'true' : 'false');
    if (data?.user?.isTwoFactorEnabled !== undefined) {
      this.setTwoFactorEnabled(Boolean(data.user.isTwoFactorEnabled));
    }

    if (data?.token) {
      this.inMemoryAccessToken = data.token;
    }
  }

  getUserId(): string {
    const user = this.currentUser();
    if (user?.id) return user.id;
    return localStorage.getItem('im3_userId') || '';
  }

  getUserEmail(): string {
    const user = this.currentUser();
    if (user?.email) return user.email;
    return localStorage.getItem('im3_email') || '';
  }

  getUserRole(): string {
    const user = this.currentUser();
    if (user?.role) return user.role;
    return localStorage.getItem('im3_role') || '';
  }

  getUserName(): string {
    const user = this.currentUser();
    if (user?.fullName) return user.fullName;
    return localStorage.getItem('im3_name') || '';
  }

  getCurrentUser(): CurrentUser | null {
    return this.currentUser();
  }

  // Tokens are stored in HttpOnly cookies to prevent XSS theft, with in-memory caching for active sockets.
  getToken(): string | null {
    return this.inMemoryAccessToken || localStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return null;
  }

  saveToken(_token: string): void {
    // No-op: Tokens stored securely in HttpOnly cookies
  }

  saveRefreshToken(_token: string): void {
    // No-op: Tokens stored securely in HttpOnly cookies
  }

  refreshToken(): Observable<any> {
    // Sends silent refresh request; browser auto-attaches HttpOnly im3_refresh cookie
    return this.http.post<any>(`${this.apiUrl}/refresh`, {}, {
      withCredentials: true
    }).pipe(
      tap(res => {
        if (res?.user) {
          this.saveUserData(res);
        }
      })
    );
  }

  refreshAccessToken(): Observable<any> {
    return this.refreshToken();
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  isAuthenticated(): boolean {
    return localStorage.getItem(AUTH_IS_LOGGED_IN) === 'true';
  }

  isTokenValid(): boolean {
    return this.isAuthenticated();
  }

  isTokenExpired(): boolean {
    return !this.isAuthenticated();
  }

  isFirstLogin(): boolean {
    return localStorage.getItem('im3_isFirstLogin') === 'true';
  }

  markFirstLoginComplete(): void {
    localStorage.setItem('im3_isFirstLogin', 'false');
  }

  isTwoFactorEnabled(): boolean {
    return localStorage.getItem(TWO_FACTOR_ENABLED_KEY) === 'true';
  }

  setTwoFactorEnabled(enabled: boolean): void {
    localStorage.setItem(TWO_FACTOR_ENABLED_KEY, String(enabled));
  }

  logout(): void {
    this.cleanLocalStateAndRedirect();
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => void 0,
      error: () => void 0
    });
  }

  private cleanLocalStateAndRedirect(): void {
    this.inMemoryAccessToken = null;
    this.currentUser.set(null);
    localStorage.removeItem(AUTH_IS_LOGGED_IN);
    localStorage.removeItem(AUTH_USER_KEY);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem('im3_userId');
    localStorage.removeItem('im3_email');
    localStorage.removeItem('im3_name');
    localStorage.removeItem('im3_role');
    localStorage.removeItem('im3_orgName');
    localStorage.removeItem('im3_isFirstLogin');
    localStorage.removeItem(TWO_FACTOR_ENABLED_KEY);

    this.router.navigate(['/auth/login']);
  }
}
