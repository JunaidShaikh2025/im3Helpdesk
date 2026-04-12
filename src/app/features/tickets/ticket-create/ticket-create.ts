import { Component, OnInit, ChangeDetectorRef, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { TicketService } from '../../../services/ticket';
import { AgentService } from '../../../services/agent';
import { AgentGroupService } from '../../../services/agent-group';
import { AuthService } from '../../../services/auth.service';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-ticket-create',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    FormsModule, MatProgressSpinnerModule, LayoutComponent
  ],
  templateUrl: './ticket-create.html',
  styleUrls: ['./ticket-create.scss']
})
export class TicketCreateComponent implements OnInit {
  private ticketService = inject(TicketService);
  private agentService = inject(AgentService);
  private groupService = inject(AgentGroupService);
  private authService = inject(AuthService);
  private http = inject(HttpClient);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  @ViewChild('descEditor') descEditorRef!: ElementRef;

  loading = false;
  uploading = false;
  agents: any[] = [];
  groups: any[] = [];
  templates: any[] = [];
  selectedTemplateId = '';
  pendingFiles: File[] = [];
  tagInput = '';
  tags: string[] = [];

  ticketTypes = [
    'Question', 'Incident', 'Problem', 'Feature Request',
    'Request', 'Data', 'Customer Training', 'Backend Script',
    'System Gap', 'Release', 'Information Only', 'On Hold'
  ];

  statuses = [
    'Open', 'Pending', 'Resolved on Beta',
    'Resolved', 'On Hold', 'Close'
  ];

  priorities = ['Low', 'Medium', 'High', 'Urgent'];

  categories = [
    'General', 'Technical', 'Billing',
    'Sales', 'Network', 'Hardware', 'Other'
  ];

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: ['', Validators.required],
    category: ['General', Validators.required],
    priority: ['Medium', Validators.required],
    ticketType: ['Question', Validators.required],
    status: ['Open', Validators.required],
    assignedToUserId: [''],
    agentGroupId: ['']
  });

  ngOnInit() {
    this.loadAgents();
    this.loadGroups();
    this.loadTemplates();
  }

  loadAgents() {
    this.agentService.getAll().subscribe({
      next: (data) => {
        this.agents = data;
        this.cdr.detectChanges();
      }
    });
  }

  loadGroups() {
    this.groupService.getAll().subscribe({
      next: (data) => {
        this.groups = data;
        this.cdr.detectChanges();
      }
    });
  }

  loadTemplates() {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
    this.http.get<any[]>(
      'https://localhost:7071/api/TicketTemplates', { headers }
    ).subscribe({
      next: (data) => {
        this.templates = data;
        this.cdr.detectChanges();
      }
    });
  }

  applyTemplate(templateId: string) {
    const t = this.templates.find(t => t.id === templateId);
    if (t) {
      this.form.patchValue({
        title: t.title,
        description: t.description,
        category: t.category,
        priority: t.priority,
        ticketType: t.ticketType || 'Support'
      });
    }
  }

  addTag() {
    const tag = this.tagInput.trim().toLowerCase();
    if (tag && !this.tags.includes(tag)) {
      this.tags.push(tag);
      this.tagInput = '';
      this.cdr.detectChanges();
    }
  }

  removeTag(tag: string) {
    this.tags = this.tags.filter(t => t !== tag);
    this.cdr.detectChanges();
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
    if (type?.includes('excel')) return '📊';
    if (type?.includes('zip')) return '🗜';
    return '📎';
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes/1024).toFixed(1)} KB`;
    return `${(bytes/1048576).toFixed(1)} MB`;
  }

  onDescInput(event: any) {
    this.form.patchValue({
      description: event.target.innerHTML
    }, { emitEvent: false });
  }

  docExec(command: string) {
    document.execCommand(command, false);
  }

  async onSubmit() {
  if (this.form.invalid) return;
  this.loading = true;
  this.cdr.detectChanges();

  try {
    const formVal = this.form.value;
    const payload = {
      title: formVal.title,
      description: formVal.description,
      category: formVal.category,
      priority: formVal.priority,
      ticketType: formVal.ticketType,
      tags: this.tags.length > 0 ? this.tags.join(',') : '',
      assignedToUserId: formVal.assignedToUserId
        ? formVal.assignedToUserId : null,
      agentGroupId: formVal.agentGroupId
        ? formVal.agentGroupId : null
    };

    const res: any = await this.ticketService
      .create(payload).toPromise();
    const ticketId = res?.id;

    if (ticketId && this.pendingFiles.length > 0) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${this.authService.getToken()}`
      });
      for (const file of this.pendingFiles) {
        const formData = new FormData();
        formData.append('file', file);
        await this.http.post(
          `https://localhost:7071/api/Attachments/upload/${ticketId}`,
          formData, { headers }
        ).toPromise();
      }
    }

    this.loading = false;
    this.cdr.detectChanges();
    Promise.resolve().then(() =>
      this.toastr.success('Ticket created!')
    );
    this.router.navigate(['/tickets', ticketId]);
  } catch (err: any) {
    this.loading = false;
    this.cdr.detectChanges();
    Promise.resolve().then(() =>
      this.toastr.error(err?.error?.message || 'Failed')
    );
  }
}
}
