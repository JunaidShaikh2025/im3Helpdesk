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
import { LiveChatComponent } from '../../chat/live-chat/live-chat';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatProgressSpinnerModule, MatDividerModule, LayoutComponent,LiveChatComponent
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
  viewers: any[] = [];

    loadTicket() {
      this.ticketService.getById(this.ticketId).subscribe({
        next: (data: any) => {
          this.ticket = data;
          this.loading = false;
          // Record view
          this.recordView();
          this.loadViewers();
          if (data.assignedTo) {
            const found = this.agents.find(
              a => a.fullName === data.assignedTo?.fullName
            );
            if (found) this.selectedAgentId = found.id;
          }
          this.cdr.detectChanges();
        }
      });
    }

    recordView() {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`
      });
      this.http.post(
        `https://localhost:7071/api/Tickets/${this.ticketId}/view`,
        {}, { headers }
      ).subscribe();
    }

    loadViewers() {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`
      });
      this.http.get<any[]>(
        `https://localhost:7071/api/Tickets/${this.ticketId}/viewers`,
        { headers }
      ).subscribe({
        next: (data) => {
          this.viewers = data;
          this.cdr.detectChanges();
        }
      });
    }

  ngOnInit() {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';

    const token = this.authService.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role = payload[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ] || payload.role || '';
      this.isAgent = ['Agent', 'CompanyAdmin', 'SuperAdmin'].includes(role);
    }

    Promise.resolve().then(() => {
      this.loadTicket();
      this.loadAttachments();
      this.loadAgents();
      this.loadGroups();
      this.loadOrgInfo();
      this.loadAgentSignature();
      this.startPolling();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
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
    this.ticketService.updateStatus(this.ticketId, status).subscribe({
      next: () => {
        Promise.resolve().then(() => this.toastr.success('Status updated!'));
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
        Promise.resolve().then(() => this.toastr.success('Priority updated!'));
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
        Promise.resolve().then(() => this.toastr.success('Type updated!'));
      }
    });
  }

  updateAllProps() {
    this.updating = true;
    this.cdr.detectChanges();

    const updates: Promise<any>[] = [];

    if (this.selectedAgentId !== undefined) {
      updates.push(this.ticketService.assign(
        this.ticketId,
        this.selectedAgentId || null
      ).toPromise());
    }

    if (this.selectedGroupId !== undefined) {
      updates.push(this.assignGroup());
    }

    Promise.all(updates).then(() => {
      this.updating = false;
      this.cdr.detectChanges();
      Promise.resolve().then(() =>
        this.toastr.success('Ticket updated successfully!')
      );
      this.loadTicket();
    }).catch(() => {
      this.updating = false;
      this.cdr.detectChanges();
      Promise.resolve().then(() =>
        this.toastr.error('Update failed')
      );
    });
  }

  assignGroup(): Promise<any> {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    return this.http.put(
      `https://localhost:7071/api/Tickets/${this.ticketId}/group`,
      { agentGroupId: this.selectedGroupId || null },
      { headers }
    ).toPromise();
  }

  assignTicket() {
    this.ticketService.assign(
      this.ticketId, this.selectedAgentId || null
    ).subscribe({
      next: () => {
        this.cdr.detectChanges();
        Promise.resolve().then(() => this.toastr.success('Assigned!'));
        this.loadTicket();
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
    return this.ticket.tags.split(',').filter((t: string) => t.trim());
  }

  getCommentAttachments(commentId: string): any[] {
    if (!this.attachments) return [];
    return this.attachments.filter(a => a.commentId === commentId);
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#ef4444', '#f97316', '#eab308', '#22c55e',
      '#3b82f6', '#8b5cf6', '#ec4899'
    ];
    const idx = (name?.charCodeAt(0) || 0) % colors.length;
    return colors[idx];
  }

  logTime() {
    if (!this.timeToLog || this.timeToLog < 1) return;
    this.updating = true;
    this.cdr.detectChanges();

    this.ticketService.logTime(this.ticketId, this.timeToLog).subscribe({
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
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
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
    const res: any = await this.ticketService.addComment(
      this.ticketId, this.quickReplyText, false
    ).toPromise();

    const commentId = res?.commentId;

    // Upload files with commentId
    if (commentId && this.pendingFiles.length > 0) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`
      });
      for (const file of this.pendingFiles) {
        const formData = new FormData();
        formData.append('file', file);
        await this.http.post(
          `https://localhost:7071/api/Attachments/upload/${this.ticketId}?commentId=${commentId}`,
          formData, { headers }
        ).toPromise();
      }
    }

    this.clearComposer();
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
    const res: any = await this.ticketService.addComment(
      this.ticketId, this.noteText, true
    ).toPromise();

    const commentId = res?.commentId;

    if (commentId && this.pendingFiles.length > 0) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`
      });
      for (const file of this.pendingFiles) {
        const formData = new FormData();
        formData.append('file', file);
        await this.http.post(
          `https://localhost:7071/api/Attachments/upload/${this.ticketId}?commentId=${commentId}`,
          formData, { headers }
        ).toPromise();
      }
    }

    this.noteText = '';
    if (this.noteEditorRef?.nativeElement)
      this.noteEditorRef.nativeElement.innerHTML = '';
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
    this.notifyAgents = this.notifyAgents.filter(a => a.id !== agent.id);
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