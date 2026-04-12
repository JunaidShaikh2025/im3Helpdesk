import {
  Component, OnInit, OnDestroy,
  ChangeDetectorRef, inject,
  ViewChild, ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { ToastrService } from 'ngx-toastr';
import { Subject, interval, takeUntil } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { TicketService } from '../../../services/ticket';
import { AgentService } from '../../../services/agent';
import { AgentGroupService } from '../../../services/agent-group';
import { AuthService } from '../../../services/auth.service';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatProgressSpinnerModule, MatDividerModule, LayoutComponent
  ],
  templateUrl: './ticket-detail.html',
  styleUrls: ['./ticket-detail.scss']
})
export class TicketDetailComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  public router = inject(Router);
  private ticketService = inject(TicketService);
  private agentService = inject(AgentService);
  private agentGroupService = inject(AgentGroupService);
  private authService = inject(AuthService);
  private toastr = inject(ToastrService);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);
  private destroy$ = new Subject<void>();

  ticket: any = null;
  loading = true;
  updating = false;
  agents: any[] = [];
  groups: any[] = [];
  ticketId = '';
  isAgent = false;

  selectedAgentId = '';
  selectedGroupId = '';
  quickReplyText = '';
  isInternalNote = false;
  newTag = '';
  timeToLog: number | null = null;

  statuses = ['Open', 'InProgress', 'Resolved', 'Closed'];

  // Properties for Editor and Attachments
  activeTab = 'reply';
  expandReply = false;
  showPrevThread = false;
  showCc = false;
  showBcc = false;
  ccEmails = '';
  bccEmails = '';
  noteText = '';
  forwardEmail = '';
  forwardText = '';
  pendingFiles: File[] = [];
  attachments: any[] = [];

  // Newly Added Properties
  notifyTo = '';
  notifyAgents: any[] = [];
  mentionResults: any[] = [];
  agentSignature = '';
  expandComposer = false;
  orgSupportEmail = '';

  @ViewChild('replyEditor') replyEditorRef: any;
  @ViewChild('noteEditor') noteEditorRef: any;
  @ViewChild('forwardEditor') forwardEditorRef: any;

  commentForm: FormGroup = this.fb.group({
    comment: ['', [Validators.required, Validators.minLength(3)]]
  });

  ngOnInit() {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';

    const token = this.authService.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role = payload[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ] || payload.role || '';
      this.isAgent = ['Agent', 'CompanyAdmin', 'SuperAdmin']
        .includes(role);
    }

    Promise.resolve().then(() => {
      this.loadTicket();
      this.loadAttachments(); 
      this.loadAgents();
      this.loadGroups();
      this.loadOrgInfo();          // Added from new methods
      this.loadAgentSignature();   // Added from new methods
      this.startPolling();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTicket() {
    this.ticketService.getById(this.ticketId).subscribe({
      next: (data: any) => {
        this.ticket = data;
        this.loading = false;
        if (data.assignedTo) {
          const found = this.agents.find(
            a => a.fullName === data.assignedTo?.fullName
          );
          if (found) this.selectedAgentId = found.id;
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadAttachments() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.get<any[]>(
      `https://localhost:7071/api/Attachments/ticket/${this.ticketId}`,
      { headers }
    ).subscribe({
      next: (data) => {
        this.attachments = data;
        this.cdr.detectChanges();
      }
    });
  }

  loadAgents() {
    this.agentService.getAll().subscribe({
      next: (data: any[]) => {
        this.agents = data;
        this.cdr.detectChanges();
      }
    });
  }

  loadGroups() {
    this.agentGroupService.getAll().subscribe({
      next: (data: any[]) => {
        this.groups = data;
        this.cdr.detectChanges();
      }
    });
  }

  startPolling() {
    interval(15000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTicket();
        this.loadAttachments();
      });
  }

  updateStatus(status: string) {
    this.ticketService.updateStatus(this.ticketId, status)
      .subscribe({
        next: () => {
          Promise.resolve().then(() =>
            this.toastr.success('Status updated!')
          );
          this.loadTicket();
        }
      });
  }

  updatePriority(priority: string) {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.put(
      `https://localhost:7071/api/Tickets/${this.ticketId}/priority`,
      { priority }, { headers }
    ).subscribe({
      next: () => {
        Promise.resolve().then(() =>
          this.toastr.success('Priority updated!')
        );
      }
    });
  }

  updateTicketType() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.put(
      `https://localhost:7071/api/Tickets/${this.ticketId}/type`,
      { ticketType: this.ticket.ticketType }, { headers }
    ).subscribe({
      next: () => {
        Promise.resolve().then(() =>
          this.toastr.success('Type updated!')
        );
      }
    });
  }

  updateAllProps() {
    if (this.selectedAgentId) this.assignTicket();
    if (this.selectedGroupId) this.assignGroup();
    Promise.resolve().then(() =>
      this.toastr.success('Updated!')
    );
  }

  assignTicket() {
    this.ticketService.assign(
      this.ticketId, this.selectedAgentId || null
    ).subscribe({
      next: () => {
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('Assigned!')
        );
        this.loadTicket();
      }
    });
  }

  assignGroup() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.put(
      `https://localhost:7071/api/Tickets/${this.ticketId}/group`,
      { agentGroupId: this.selectedGroupId || null }, { headers }
    ).subscribe({
      next: () => {
        Promise.resolve().then(() =>
          this.toastr.success('Group assigned!')
        );
      }
    });
  }

  addTag() {
    if (!this.newTag.trim()) return;
    const tags = this.getTagsArray();
    if (!tags.includes(this.newTag.trim().toLowerCase())) {
      tags.push(this.newTag.trim().toLowerCase());
    }
    this.ticketService.updateTags(this.ticketId, tags).subscribe({
      next: () => {
        this.newTag = '';
        this.loadTicket();
      }
    });
  }

  getTagsArray(): string[] {
    if (!this.ticket?.tags) return [];
    return this.ticket.tags
      .split(',')
      .filter((t: string) => t.trim());
  }

  logTime() {
    if (!this.timeToLog || this.timeToLog < 1) return;
    this.updating = true;
    this.cdr.detectChanges();

    this.ticketService.logTime(this.ticketId, this.timeToLog)
      .subscribe({
        next: (res: any) => {
          this.timeToLog = null;
          this.updating = false;
          this.cdr.detectChanges();
          Promise.resolve().then(() =>
            this.toastr.success(`${res.totalMinutes} min logged`)
          );
          this.loadTicket();
        },
        error: () => {
          this.updating = false;
          this.cdr.detectChanges();
        }
      });
  }

  // --- Editor & Attachment Logic ---

  onReplyInput(event: any) {
    this.quickReplyText = event.target.innerText || '';
  }

  onNoteInput(event: any) {
    this.noteText = event.target.innerText || '';
  }

  onForwardInput(event: any) {
    this.forwardText = event.target.innerText || '';
  }

  formatText(command: string) {
    document.execCommand(command, false);
  }

  insertLink() {
    const url = prompt('Enter URL:');
    if (url) document.execCommand('createLink', false, url);
  }

  onFileSelect(event: any) {
    const files = Array.from(event.target.files) as File[];
    this.pendingFiles.push(...files);
    this.cdr.detectChanges();
  }

  removePendingFile(index: number) {
    this.pendingFiles.splice(index, 1);
    this.cdr.detectChanges();
  }

  getFileIcon(type: string): string {
    if (type?.startsWith('image/')) return '🖼';
    if (type?.includes('pdf')) return '📄';
    if (type?.includes('word')) return '📝';
    if (type?.includes('excel') || type?.includes('sheet')) return '📊';
    if (type?.includes('zip')) return '🗜';
    return '📎';
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes/1024).toFixed(1)} KB`;
    return `${(bytes/(1024*1024)).toFixed(1)} MB`;
  }

  clearReply() {
    this.quickReplyText = '';
    if (this.replyEditorRef?.nativeElement) {
      this.replyEditorRef.nativeElement.innerText = '';
    }
    this.pendingFiles = [];
  }

  async sendReply() {
    if (!this.quickReplyText.trim()) return;
    this.updating = true;
    this.cdr.detectChanges();

    try {
      for (const file of this.pendingFiles) {
        await this.uploadFile(file);
      }

      await this.ticketService.addComment(
        this.ticketId, this.quickReplyText, false
      ).toPromise();

      this.clearReply();
      this.updating = false;
      this.cdr.detectChanges();
      Promise.resolve().then(() => this.toastr.success('Reply sent!'));
      this.loadTicket();
      this.loadAttachments();
    } catch {
      this.updating = false;
      this.cdr.detectChanges();
    }
  }

  async sendNote() {
    if (!this.noteText.trim()) return;
    this.updating = true;
    this.cdr.detectChanges();

    try {
      for (const file of this.pendingFiles) {
        await this.uploadFile(file);
      }

      await this.ticketService.addComment(
        this.ticketId, this.noteText, true
      ).toPromise();

      this.noteText = '';
      if (this.noteEditorRef?.nativeElement) {
        this.noteEditorRef.nativeElement.innerText = '';
      }
      this.pendingFiles = [];
      this.updating = false;
      this.cdr.detectChanges();
      Promise.resolve().then(() => this.toastr.success('Note added!'));
      this.loadTicket();
      this.loadAttachments();
    } catch {
      this.updating = false;
      this.cdr.detectChanges();
    }
  }

  doForward() {
    if (!this.forwardEmail.trim()) return;
    Promise.resolve().then(() =>
      this.toastr.info(`Forwarded to ${this.forwardEmail}`)
    );
    this.forwardEmail = '';
    this.activeTab = 'reply';
  }

  private uploadFile(file: File): Promise<any> {
    const formData = new FormData();
    formData.append('file', file);
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    return this.http.post(
      `https://localhost:7071/api/Attachments/upload/${this.ticketId}`,
      formData, { headers }
    ).toPromise();
  }

  logout() {
    this.authService.logout();
  }

  // --- Newly Added Methods ---

  loadOrgInfo() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.get<any>(
      'https://localhost:7071/api/Organizations/current',
      { headers }
    ).subscribe({
      next: (data) => {
        this.orgSupportEmail = data.supportEmail || '';
        this.cdr.detectChanges();
      }
    });
  }

  loadAgentSignature() {
    const token = this.authService.getToken();
    if (!token) return;
    const payload = JSON.parse(atob(token.split('.')[1]));
    const userId = payload.sub
      || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    if (!userId) return;

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    this.http.get<any>(
      `https://localhost:7071/api/Agents/${userId}`,
      { headers }
    ).subscribe({
      next: (data) => {
        this.agentSignature = data.signature || '';
        this.cdr.detectChanges();
      }
    });
  }

  searchAgentsForMention(event: any) {
    const q = event.target.value?.toLowerCase();
    if (!q || q.length < 1) {
      this.mentionResults = [];
      return;
    }
    this.mentionResults = this.agents.filter(a =>
      a.fullName?.toLowerCase().includes(q)
    ).slice(0, 5);
    this.cdr.detectChanges();
  }

  addMention(agent: any) {
    if (!this.notifyAgents.find(a => a.id === agent.id)) {
      this.notifyAgents.push(agent);
    }
    this.notifyTo = '';
    this.mentionResults = [];
    this.cdr.detectChanges();
  }

  removeNotify(agent: any) {
    this.notifyAgents = this.notifyAgents
      .filter(a => a.id !== agent.id);
    this.cdr.detectChanges();
  }

  execCmd(command: string, value?: string) {
    document.execCommand(command, false, value);
  }

  clearComposer() {
    this.quickReplyText = '';
    this.noteText = '';
    this.pendingFiles = [];
    if (this.replyEditorRef?.nativeElement)
      this.replyEditorRef.nativeElement.innerHTML = '';
    if (this.noteEditorRef?.nativeElement)
      this.noteEditorRef.nativeElement.innerHTML = '';
    this.cdr.detectChanges();
  }
}