import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'https://localhost:7071/api/Auth';

  constructor(private http: HttpClient, private router: Router) {}

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, data);
  }

  forgotPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, data);
  }

  verifyEmail(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-email?token=${token}`, {});
  }

  saveToken(token: string): void {
    localStorage.setItem('im3_token', token);
  }

  saveUserData(data: any): void {
    localStorage.setItem('im3_token', data.token);
    localStorage.setItem('im3_isFirstLogin', data.isFirstLogin?.toString());
    localStorage.setItem('im3_role', data.user?.role || '');
    localStorage.setItem('im3_name', data.user?.fullName || '');
  }

  getToken(): string | null {
    return localStorage.getItem('im3_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isFirstLogin(): boolean {
    return localStorage.getItem('im3_isFirstLogin') === 'true';
  }

  getUserRole(): string {
    return localStorage.getItem('im3_role') || '';
  }

  getUserName(): string {
    return localStorage.getItem('im3_name') || '';
  }

  logout(): void {
    localStorage.removeItem('im3_token');
    localStorage.removeItem('im3_isFirstLogin');
    localStorage.removeItem('im3_role');
    localStorage.removeItem('im3_name');
    this.router.navigate(['/login']);
  }
}