import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface NoteBook {
  id: string;
  name: string;
  color?: string;
  displayOrder: number;
  sectionCount?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface NoteSection {
  id: string;
  noteBookId: string;
  name: string;
  color?: string;
  displayOrder: number;
  createdAt: string;
  updatedAt?: string;
  pages?: NotePage[];
}

export interface NotePage {
  id: string;
  noteSectionId: string;
  title: string;
  content?: string;
  displayOrder: number;
  createdAt: string;
  updatedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class NotesService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/Notes`;

  getBooks()                              { return this.http.get<NoteBook[]>(`${this.base}/books`); }
  createBook(name: string, color?: string){ return this.http.post<NoteBook>(`${this.base}/books`, { name, color }); }
  updateBook(id: string, name: string, color?: string){ return this.http.put<any>(`${this.base}/books/${id}`, { name, color }); }
  deleteBook(id: string)                  { return this.http.delete(`${this.base}/books/${id}`); }

  getSections(bookId: string)             { return this.http.get<NoteSection[]>(`${this.base}/books/${bookId}/sections`); }
  createSection(bookId: string, name: string){ return this.http.post<NoteSection>(`${this.base}/sections`, { noteBookId: bookId, name }); }
  updateSection(id: string, name: string) { return this.http.put<any>(`${this.base}/sections/${id}`, { name }); }
  deleteSection(id: string)               { return this.http.delete(`${this.base}/sections/${id}`); }

  getPage(id: string)                     { return this.http.get<NotePage>(`${this.base}/pages/${id}`); }
  createPage(sectionId: string, title: string){ return this.http.post<NotePage>(`${this.base}/pages`, { noteSectionId: sectionId, title }); }
  updatePage(id: string, title: string, content: string){ return this.http.put<any>(`${this.base}/pages/${id}`, { title, content }); }
  deletePage(id: string)                  { return this.http.delete(`${this.base}/pages/${id}`); }
}
