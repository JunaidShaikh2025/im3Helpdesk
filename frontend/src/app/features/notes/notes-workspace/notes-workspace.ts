import {
  Component, OnInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef, inject,
  ViewChild, ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { NotesService, NoteBook, NoteSection, NotePage } from '../../../core/services/notes.service';
import { LayoutComponent } from '../../../layouts/main-layout/layout';

@Component({
  selector: 'app-notes-workspace',
  standalone: true,
  imports: [CommonModule, FormsModule, LayoutComponent],
  templateUrl: './notes-workspace.html',
  styleUrls: ['./notes-workspace.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotesWorkspaceComponent implements OnInit, OnDestroy {
  private svc = inject(NotesService);
  private cdr = inject(ChangeDetectorRef);
  private san = inject(DomSanitizer);

  @ViewChild('editorEl')   editorEl?:   ElementRef<HTMLDivElement>;
  @ViewChild('titleInput') titleInput?: ElementRef<HTMLInputElement>;

  books:   NoteBook[]   = [];
  sections: NoteSection[] = [];

  selectedBook:    NoteBook    | null = null;
  selectedSection: NoteSection | null = null;
  selectedPage:    NotePage    | null = null;

  editingTitle = '';
  saveState: 'saved' | 'saving' | 'unsaved' = 'saved';
  private saveTimer: any;
  private lastTitle   = '';
  private lastContent = '';

  // Inline rename/new
  renamingBookId    = '';
  renameBookVal     = '';
  renamingSectionId = '';
  renameSectionVal  = '';
  showNewBook       = false;
  newBookName       = '';
  showNewSection    = false;
  newSectionName    = '';
  newSectionBookId  = '';

  // Confirm-delete (2-click)
  deletingBookId    = '';
  deletingSectionId = '';
  deletingPageId    = '';

  // Collapsed sections
  collapsedSecs = new Set<string>();

  loading     = false;
  loadingPage = false;

  readonly COLORS = [
    '#6366f1','#3b82f6','#10b981','#f59e0b',
    '#ef4444','#ec4899','#14b8a6','#8b5cf6','#f97316'
  ];

  ngOnInit() { this.loadBooks(); }

  ngOnDestroy() {
    clearTimeout(this.saveTimer);
    if (this.saveState === 'unsaved') this.flush();
  }

  // ── Load ──────────────────────────────────────────────────────────────────
  loadBooks() {
    this.loading = true;
    this.svc.getBooks().subscribe({
      next: (books) => {
        this.books   = books;
        this.loading = false;
        if (books.length) this.selectBook(books[0]);
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  selectBook(book: NoteBook) {
    if (this.selectedBook?.id === book.id) return;
    this.flush();
    this.selectedBook = book;
    this.selectedPage = null;
    this.sections     = [];
    this.loadSections(book.id);
  }

  loadSections(bookId: string) {
    this.svc.getSections(bookId).subscribe({
      next: (secs) => {
        this.sections = secs;
        // Auto-open first page
        if (!this.selectedPage) {
          const first = secs.flatMap(s => s.pages ?? []).find(Boolean);
          if (first) {
            const sec = secs.find(s => s.pages?.some(p => p.id === first.id));
            if (sec) this.selectPage(first, sec);
          }
        }
        this.cdr.detectChanges();
      }
    });
  }

  selectPage(page: NotePage, section: NoteSection) {
    this.flush();
    this.selectedSection = section;
    this.selectedPage    = { ...page };
    this.editingTitle    = page.title;
    this.loadingPage     = true;
    this.cdr.detectChanges();

    this.svc.getPage(page.id).subscribe({
      next: (full) => {
        this.selectedPage    = full;
        this.editingTitle    = full.title;
        this.lastTitle       = full.title;
        this.lastContent     = full.content ?? '';
        this.saveState       = 'saved';
        this.loadingPage     = false;
        this.cdr.detectChanges();
        setTimeout(() => {
          if (this.editorEl) this.editorEl.nativeElement.innerHTML = full.content ?? '';
        }, 0);
      },
      error: () => { this.loadingPage = false; this.cdr.detectChanges(); }
    });
  }

  // ── Book CRUD ─────────────────────────────────────────────────────────────
  startNewBook() {
    this.showNewBook = true;
    this.newBookName = '';
    setTimeout(() => (document.getElementById('new-book-inp') as HTMLInputElement)?.focus(), 50);
  }

  confirmNewBook() {
    const name = this.newBookName.trim();
    this.showNewBook = false;
    this.newBookName = '';
    if (!name) return;
    const color = this.COLORS[this.books.length % this.COLORS.length];
    this.svc.createBook(name, color).subscribe({
      next: (b) => { this.books.push(b); this.selectBook(b); this.cdr.detectChanges(); }
    });
  }

  startRenameBook(b: NoteBook, e: Event) {
    e.stopPropagation();
    this.renamingBookId = b.id;
    this.renameBookVal  = b.name;
    setTimeout(() => (document.getElementById(`rb-${b.id}`) as HTMLInputElement)?.focus(), 50);
  }

  confirmRenameBook(b: NoteBook) {
    const name = this.renameBookVal.trim();
    this.renamingBookId = '';
    if (!name) return;
    this.svc.updateBook(b.id, name, b.color).subscribe({ next: () => { b.name = name; this.cdr.detectChanges(); } });
  }

  deleteBook(b: NoteBook, e: Event) {
    e.stopPropagation();
    if (this.deletingBookId === b.id) {
      this.svc.deleteBook(b.id).subscribe({ next: () => {
        this.books = this.books.filter(x => x.id !== b.id);
        if (this.selectedBook?.id === b.id) { this.selectedBook = null; this.sections = []; this.selectedPage = null; }
        if (this.books.length) this.selectBook(this.books[0]);
        this.deletingBookId = '';
        this.cdr.detectChanges();
      }});
    } else {
      this.deletingBookId = b.id;
      setTimeout(() => { if (this.deletingBookId === b.id) { this.deletingBookId = ''; this.cdr.detectChanges(); } }, 3000);
      this.cdr.detectChanges();
    }
  }

  // ── Section CRUD ──────────────────────────────────────────────────────────
  startNewSection(bookId: string) {
    this.newSectionBookId = bookId;
    this.showNewSection   = true;
    this.newSectionName   = '';
    setTimeout(() => (document.getElementById('new-sec-inp') as HTMLInputElement)?.focus(), 50);
  }

  confirmNewSection() {
    const name = this.newSectionName.trim();
    const bid  = this.newSectionBookId;
    this.showNewSection = false;
    this.newSectionName = '';
    if (!name || !bid) return;
    this.svc.createSection(bid, name).subscribe({
      next: (s) => {
        this.sections.push({ ...s, pages: [] });
        this.cdr.detectChanges();
      }
    });
  }

  startRenameSection(s: NoteSection, e: Event) {
    e.stopPropagation();
    this.renamingSectionId = s.id;
    this.renameSectionVal  = s.name;
    setTimeout(() => (document.getElementById(`rs-${s.id}`) as HTMLInputElement)?.focus(), 50);
  }

  confirmRenameSection(s: NoteSection) {
    const name = this.renameSectionVal.trim();
    this.renamingSectionId = '';
    if (!name) return;
    this.svc.updateSection(s.id, name).subscribe({ next: () => { s.name = name; this.cdr.detectChanges(); } });
  }

  deleteSection(s: NoteSection, e: Event) {
    e.stopPropagation();
    if (this.deletingSectionId === s.id) {
      this.svc.deleteSection(s.id).subscribe({ next: () => {
        this.sections = this.sections.filter(x => x.id !== s.id);
        if (this.selectedSection?.id === s.id) { this.selectedSection = null; this.selectedPage = null; }
        this.deletingSectionId = '';
        this.cdr.detectChanges();
      }});
    } else {
      this.deletingSectionId = s.id;
      setTimeout(() => { if (this.deletingSectionId === s.id) { this.deletingSectionId = ''; this.cdr.detectChanges(); } }, 3000);
      this.cdr.detectChanges();
    }
  }

  toggleSection(id: string) {
    if (this.collapsedSecs.has(id)) this.collapsedSecs.delete(id);
    else this.collapsedSecs.add(id);
    this.cdr.detectChanges();
  }

  isCollapsed(id: string) { return this.collapsedSecs.has(id); }

  // ── Page CRUD ─────────────────────────────────────────────────────────────
  addPage(s: NoteSection) {
    this.svc.createPage(s.id, 'Untitled Page').subscribe({
      next: (p) => {
        if (!s.pages) s.pages = [];
        s.pages.push(p);
        this.selectPage(p, s);
        this.cdr.detectChanges();
        setTimeout(() => this.titleInput?.nativeElement?.select(), 120);
      }
    });
  }

  deletePage(p: NotePage, s: NoteSection, e: Event) {
    e.stopPropagation();
    if (this.deletingPageId === p.id) {
      this.svc.deletePage(p.id).subscribe({ next: () => {
        if (s.pages) s.pages = s.pages.filter(x => x.id !== p.id);
        if (this.selectedPage?.id === p.id) {
          this.selectedPage = null;
          this.saveState    = 'saved';
          const next = this.sections.flatMap(sec => sec.pages ?? []).find(Boolean);
          if (next) { const sec = this.sections.find(sec => sec.pages?.some(pg => pg.id === next.id)); if (sec) this.selectPage(next, sec); }
        }
        this.deletingPageId = '';
        this.cdr.detectChanges();
      }});
    } else {
      this.deletingPageId = p.id;
      setTimeout(() => { if (this.deletingPageId === p.id) { this.deletingPageId = ''; this.cdr.detectChanges(); } }, 3000);
      this.cdr.detectChanges();
    }
  }

  // ── Editor ────────────────────────────────────────────────────────────────
  onTitleChange() { this.markUnsaved(); }

  onContentInput() { this.markUnsaved(); }

  markUnsaved() {
    this.saveState = 'unsaved';
    clearTimeout(this.saveTimer);
    this.saveTimer = setTimeout(() => this.flush(), 1500);
  }

  flush() {
    if (!this.selectedPage) return;
    const title   = (this.editingTitle || '').trim() || 'Untitled Page';
    const content = this.editorEl?.nativeElement?.innerHTML ?? '';
    if (title === this.lastTitle && content === this.lastContent) {
      this.saveState = 'saved';
      return;
    }
    this.saveState = 'saving';
    this.cdr.detectChanges();

    this.svc.updatePage(this.selectedPage.id, title, content).subscribe({
      next: (res) => {
        if (this.selectedPage) {
          this.selectedPage.title     = title;
          this.selectedPage.updatedAt = res.updatedAt;
          // Sync title in sidebar list
          const sec = this.sections.find(s => s.id === this.selectedPage!.noteSectionId);
          const pg  = sec?.pages?.find(p => p.id === this.selectedPage!.id);
          if (pg) { pg.title = title; pg.updatedAt = res.updatedAt; }
        }
        this.lastTitle   = title;
        this.lastContent = content;
        this.saveState   = 'saved';
        this.cdr.detectChanges();
      },
      error: () => { this.saveState = 'unsaved'; this.cdr.detectChanges(); }
    });
  }

  fmt(cmd: string, val?: string) {
    document.execCommand(cmd, false, val);
    this.editorEl?.nativeElement?.focus();
    this.markUnsaved();
  }

  insertLink() {
    const url = prompt('Enter URL:');
    if (url) this.fmt('createLink', url);
  }

  // ── Date helpers ─────────────────────────────────────────────────────────
  fmtDate(iso?: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleDateString('en-US', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
    });
  }

  fmtTime(iso?: string): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }

  relTime(iso?: string): string {
    if (!iso) return '';
    const diffMs = Date.now() - new Date(iso).getTime();
    const m = Math.floor(diffMs / 60000);
    if (m < 1) return 'just now';
    if (m < 60) return `${m}m ago`;
    const h = Math.floor(m / 60);
    if (h < 24) return `${h}h ago`;
    return this.fmtDate(iso);
  }

  trackById(_: number, item: { id: string }) { return item.id; }
}
