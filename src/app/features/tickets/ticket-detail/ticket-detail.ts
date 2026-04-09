import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { ToastrService } from 'ngx-toastr';
import { TicketService } from '../../../services/ticket';
import { AuthService } from '../../../services/auth.service';
import { AgentService } from '../../../services/agent';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule, FormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatChipsModule, MatToolbarModule,
    MatProgressSpinnerModule, MatCardModule, MatDividerModule
  ],
  templateUrl: './ticket-detail.html',
  styleUrls: ['./ticket-detail.scss']
})
export class TicketDetailComponent implements OnInit {
  ticket: any = null;
  loading = true;
  updating = false;
  commentForm: FormGroup;
  ticketId = '';
  
  // New properties
  private agentService = inject(AgentService);
  agents: any[] = [];
  selectedAgentId = '';

  statuses = ['Open', 'InProgress', 'Resolved', 'Closed'];

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private ticketService: TicketService,
    private authService: AuthService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.commentForm = this.fb.group({
      comment: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  ngOnInit() {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';
    this.loadTicket();
    this.loadAgents();
  }

  loadTicket() {
    this.loading = true;
    this.ticketService.getById(this.ticketId).subscribe({
      next: (data) => {
        this.ticket = data;
        this.loading = false;
        // Agar ticket already assigned hai, toh dropdown mein wo value set ho jayegi
        if (data.assignedToId) {
          this.selectedAgentId = data.assignedToId;
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load ticket');
        this.cdr.detectChanges();
      }
    });
  }

  loadAgents() {
    this.agentService.getAll().subscribe({
      next: (data: any[]) => {
        this.agents = data;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastr.error('Failed to load agents');
      }
    });
  }

  assignTicket() {
    this.updating = true;
    this.ticketService.assign(
      this.ticketId,
      this.selectedAgentId || null
    ).subscribe({
      next: () => {
        this.updating = false;
        this.toastr.success('Ticket assigned!');
        this.loadTicket();
      },
      error: () => {
        this.updating = false;
        this.toastr.error('Failed to assign ticket');
        this.cdr.detectChanges();
      }
    });
  }

  updateStatus(status: string) {
    this.updating = true;
    this.ticketService.updateStatus(this.ticketId, status).subscribe({
      next: () => {
        this.ticket.status = status;
        this.updating = false;
        this.toastr.success('Status updated!');
        this.cdr.detectChanges();
      },
      error: () => {
        this.updating = false;
        this.toastr.error('Failed to update status');
        this.cdr.detectChanges();
      }
    });
  }

  addComment() {
    if (this.commentForm.invalid) return;
    this.updating = true;
    this.cdr.detectChanges();

    this.ticketService.addComment(
      this.ticketId,
      this.commentForm.value.comment
    ).subscribe({
      next: () => {
        this.commentForm.reset();
        this.updating = false;
        this.toastr.success('Comment added!');
        this.loadTicket();
      },
      error: () => {
        this.updating = false;
        this.toastr.error('Failed to add comment');
        this.cdr.detectChanges();
      }
    });
  }

  getStatusColor(status: string): string {
    const colors: any = {
      'Open': '#f44336',
      'InProgress': '#ff9800',
      'Resolved': '#4caf50',
      'Closed': '#9e9e9e'
    };
    return colors[status] || '#666';
  }

  getPriorityColor(priority: string): string {
    const colors: any = {
      'Critical': '#f44336',
      'High': '#ff9800',
      'Medium': '#2196f3',
      'Low': '#4caf50'
    };
    return colors[priority] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}