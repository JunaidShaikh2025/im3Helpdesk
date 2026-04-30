import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class KnowledgeBaseService {
  private apiUrl = 'https://localhost:7071/api/KnowledgeBase';

  constructor(
    private http: HttpClient,
    private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization':
        `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(params?: any): Observable<any[]> {
    const query = new URLSearchParams();
    if (params?.category)
      query.set('category', params.category);
    if (params?.search)
      query.set('search', params.search);
    if (params?.publishedOnly !== undefined)
      query.set('publishedOnly', params.publishedOnly);
    return this.http.get<any[]>(
      `${this.apiUrl}?${query.toString()}`,
      { headers: this.getHeaders() });
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() });
  }

  create(data: any): Observable<any> {
    return this.http.post(
      this.apiUrl, data,
      { headers: this.getHeaders() });
  }

  update(id: string, data: any): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/${id}`, data,
      { headers: this.getHeaders() });
  }

  delete(id: string): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/${id}`,
      { headers: this.getHeaders() });
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.apiUrl}/categories`,
      { headers: this.getHeaders() });
  }

  // ✅ NAYA
  getUnreadCount(): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/unread-count`,
      { headers: this.getHeaders() });
  }
}