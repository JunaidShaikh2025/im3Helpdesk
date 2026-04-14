import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private apiUrl = 'https://localhost:7071/api/Profile';
  private baseUrl = 'https://localhost:7071';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getProfile(): Observable<any> {
    return this.http.get<any>(this.apiUrl, { headers: this.getHeaders() });
  }

  updateProfile(data: any): Observable<any> {
    return this.http.put(this.apiUrl, data, { headers: this.getHeaders() });
  }

  changePassword(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/change-password`, data, { headers: this.getHeaders() });
  }

  updateOrganization(data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/api/Organizations/current`, data, { headers: this.getHeaders() });
  }

  // Naya method photo upload ke liye
  uploadPhoto(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/upload-photo`, formData, { 
      headers: this.getHeaders() 
    });
  }

  // Helper method full URL banane ke liye
  getFullPhotoUrl(path: string | null): string {
    if (!path) return '';
    return path.startsWith('http') ? path : `${this.baseUrl}${path}`;
  }
}