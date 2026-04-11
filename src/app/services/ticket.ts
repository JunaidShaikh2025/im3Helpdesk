import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, interval, switchMap, distinctUntilChanged } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private apiUrl = 'https://localhost:7071/api/Tickets';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl, { headers: this.getHeaders() });
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`,
      { headers: this.getHeaders() });
  }

  create(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.getHeaders() });
  }

  logTime(id: string, minutes: number, note?: string): Observable<any> {
  return this.http.put(`${this.apiUrl}/${id}/log-time`,
    { minutes, note }, { headers: this.getHeaders() });
}

  updateStatus(id: string, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/status`,
      { status }, { headers: this.getHeaders() });
  }

  addComment(id: string, comment: string,
    isInternal: boolean = false): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/comments`,
      { comment, isInternal }, { headers: this.getHeaders() });
  }

  assign(id: string, agentId: string | null): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/assign`,
      { agentId }, { headers: this.getHeaders() });
  }

  search(params: any): Observable<any[]> {
    const query = new URLSearchParams();
    if (params.query) query.set('query', params.query);
    if (params.status) query.set('status', params.status);
    if (params.priority) query.set('priority', params.priority);
    if (params.category) query.set('category', params.category);
    return this.http.get<any[]>(
      `${this.apiUrl}/search?${query.toString()}`,
      { headers: this.getHeaders() });
  }

  bulkUpdate(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/bulk-update`, data,
      { headers: this.getHeaders() });
  }
  
  exportTickets(status?: string, priority?: string): Observable<Blob> {
    const params = new URLSearchParams();
    if (status) params.set('status', status);
    if (priority) params.set('priority', priority);

    return this.http.get(
      `${this.apiUrl}/export?${params.toString()}`,
      { headers: this.getHeaders(), responseType: 'blob' }
    );
  }

  updateTags(id: string, tags: string[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/tags`,
      { tags }, { headers: this.getHeaders() });
  }

  getByTag(tag: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/by-tag/${tag}`,
      { headers: this.getHeaders() });
  }

  // Poll ticket every 15 seconds for real-time updates
  pollTicket(id: string): Observable<any> {
    return interval(15000).pipe(
      switchMap(() => this.getById(id))
    );
  }

  // Poll ticket list every 30 seconds
  pollTickets(): Observable<any[]> {
    return interval(30000).pipe(
      switchMap(() => this.getAll())
    );
  }
}