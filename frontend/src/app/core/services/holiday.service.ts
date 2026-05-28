import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * Holiday Setup API client.
 *
 * Endpoints map 1-1 to `HolidaysController`:
 *   GET    /api/Holidays/years
 *   GET    /api/Holidays/years/{year}
 *   PUT    /api/Holidays/years/{year}
 *   POST   /api/Holidays/years/{year}/upload-pdf      (multipart)
 *   DELETE /api/Holidays/years/{year}
 *   POST   /api/Holidays
 *   PUT    /api/Holidays/{id}
 *   DELETE /api/Holidays/{id}
 *   GET    /api/Holidays/reminders                    (used by topbar)
 *   GET    /api/Holidays/calendar?start=&end=         (used by calendar)
 */
export interface HolidayRow {
  id: string;
  year: number;
  date: string;       // yyyy-MM-dd
  occasion: string;
  day?: string | null;
  isFloating: boolean;
}

export interface YearSetupSummary {
  id?: string | null;
  year: number;
  holidayCount: number;
  pdfFileUrl?: string | null;
  pdfFileName?: string | null;
  floatingHolidayAllowance: number;
  policyText?: string | null;
  updatedAt?: string | null;
}

export interface YearDetail {
  setup: YearSetupSummary;
  holidays: HolidayRow[];
}

export interface HolidayUpsert {
  year: number;
  date: string;
  occasion: string;
  day?: string | null;
  isFloating: boolean;
}

@Injectable({ providedIn: 'root' })
export class HolidayService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/Holidays`;

  listYears(): Observable<YearSetupSummary[]> {
    return this.http.get<YearSetupSummary[]>(`${this.base}/years`);
  }

  getYear(year: number): Observable<YearDetail> {
    return this.http.get<YearDetail>(`${this.base}/years/${year}`);
  }

  saveYearSetup(year: number, body: {
    year: number;
    floatingHolidayAllowance: number;
    policyText?: string | null;
  }): Observable<any> {
    return this.http.put(`${this.base}/years/${year}`, body);
  }

  uploadPdf(year: number, file: File, replace: boolean = false): Observable<{
    fileUrl: string;
    fileName: string;
    extracted?: number;
    added?: number;
    skipped?: number;
    warnings?: string[];
  }> {
    const fd = new FormData();
    fd.append('file', file, file.name);
    const params = new HttpParams().set('replace', String(replace));
    return this.http.post<any>(
      `${this.base}/years/${year}/upload-pdf`, fd, { params }
    );
  }

  deleteYear(year: number): Observable<any> {
    return this.http.delete(`${this.base}/years/${year}`);
  }

  create(body: HolidayUpsert): Observable<HolidayRow> {
    return this.http.post<HolidayRow>(this.base, body);
  }

  update(id: string, body: HolidayUpsert): Observable<HolidayRow> {
    return this.http.put<HolidayRow>(`${this.base}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  reminders(): Observable<{
    today: string;
    tomorrow: string;
    todayCount: number;
    tomorrowCount: number;
    items: { id: string; occasion: string; date: string; when: 'today' | 'tomorrow'; isFloating: boolean }[];
  }> {
    return this.http.get<any>(`${this.base}/reminders`);
  }
}
