import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
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
    return this.http.get<any>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  create(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.getHeaders() });
  }

  updateStatus(id: string, status: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/status`,
      { status }, { headers: this.getHeaders() });
  }

  addComment(id: string, comment: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/comments`,
      { comment }, { headers: this.getHeaders() });
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
}