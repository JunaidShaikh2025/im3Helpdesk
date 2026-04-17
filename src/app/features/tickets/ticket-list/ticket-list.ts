import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ToastrService } from 'ngx-toastr';
import { TicketService } from '../../../services/ticket';
import { AuthService } from '../../../services/auth.service';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AgentService } from '../../../services/agent';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatToolbarModule, MatProgressSpinnerModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatCardModule,LayoutComponent,MatCheckboxModule 
  ],
  templateUrl: './ticket-list.html',
  styleUrls: ['./ticket-list.scss']
})
export class TicketListComponent implements OnInit {
  private ticketService = inject(TicketService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  public cdr = inject(ChangeDetectorRef); 
  private fb = inject(FormBuilder);
  private agentService = inject(AgentService);

  tickets: any[] = [];
  allTickets: any[] = [];
  loading = true;
  showFilters = false;
  showColumnPicker = false;
  agents: any[] = [];

  displayedColumns = [
    'select', 'title', 'category', 'status', 'priority',
    'createdBy', 'assignedTo', 'sla', 'createdAt', 'actions'
  ];


  currentLayout: 'card' | 'table' | 'grid' = 'card';
  visibleColumns = [
    'title', 'status', 'priority',
    'assignedTo', 'createdAt', 'sla'
  ];

  allColumns = [
    { id: 'title', label: 'Title' },
    { id: 'category', label: 'Category' },
    { id: 'status', label: 'Status' },
    { id: 'priority', label: 'Priority' },
    { id: 'assignedTo', label: 'Assigned To' },
    { id: 'createdBy', label: 'Created By' },
    { id: 'createdAt', label: 'Date' },
    { id: 'sla', label: 'SLA' },
    { id: 'tags', label: 'Tags' },
    { id: 'ticketType', label: 'Type' }
  ];

  filterForm: FormGroup = this.fb.group({
    query: [''],
    status: ['All'],
    priority: ['All'],
    category: ['All']
  });

  statuses = ['All', 'Open', 'InProgress', 'Resolved', 'Closed'];
  priorities = ['All', 'Low', 'Medium', 'High', 'Critical'];
  categories = ['All', 'General', 'Technical', 'Billing', 'Sales', 'Network'];

  // --- Bulk Select Properties ---
  selectedTicketIds: Set<string> = new Set();
  bulkStatus = '';
  bulkUpdating = false;

ngOnInit() {
  Promise.resolve().then(() => {
    this.loading = true;
    this.cdr.detectChanges();
    this.loadTickets();
    this.loadAgents();
    this.filterForm.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.applyFilters());
  });
}

  loadAgents() {
    this.agentService.getAll().subscribe({
      next: (data: any[]) => {
        this.agents = data;
        this.cdr.detectChanges();
      },
      error: () => {
        this.agents = [];
      }
    });
  }

    toggleColumn(colId: string) {
    const idx = this.visibleColumns.indexOf(colId);
    if (idx > -1) {
      if (this.visibleColumns.length > 2)
        this.visibleColumns.splice(idx, 1);
    } else {
      this.visibleColumns.push(colId);
    }
    this.cdr.detectChanges();
  }

  isColumnVisible(colId: string): boolean {
    return this.visibleColumns.includes(colId);
  }

  loadTickets() {
    this.loading = true;
    this.cdr.detectChanges();

    this.ticketService.getAll().subscribe({
      next: (data: any[]) => {
        this.allTickets = data;
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.cdr.detectChanges();

        // 401 = token expired, logout
        if (err.status === 401) {
          this.authService.logout();
          return;
        }

        Promise.resolve().then(() =>
          this.toastr.error('Failed to load tickets')
        );
      }
    });
  }

  applyFilters() {
    const { query, status, priority, category } = this.filterForm.value;
    this.ticketService.search({ query, status, priority, category })
      .subscribe({
        next: (data: any[]) => {
          this.tickets = data;
          this.cdr.detectChanges();
        }
      });
  }

  clearFilters() {
    this.filterForm.reset({
      query: '', status: 'All',
      priority: 'All', category: 'All'
    });
    this.tickets = this.allTickets;
    this.cdr.detectChanges();
  }

  // --- UI Helper Methods ---

  getStatusColor(status: string): string {
    const colors: any = {
      'Open': '#f44336', 'InProgress': '#ff9800',
      'Resolved': '#4caf50', 'Closed': '#9e9e9e'
    };
    return colors[status] || '#666';
  }

  getPriorityColor(priority: string): string {
    const colors: any = {
      'Critical': '#f44336', 'High': '#ff9800',
      'Medium': '#2196f3', 'Low': '#4caf50'
    };
    return colors[priority] || '#666';
  }

  hasActiveFilters(): boolean {
    const v = this.filterForm.value;
    return v.status !== 'All' || v.priority !== 'All'
      || v.category !== 'All' || !!v.query;
  }

  getAvatarColor(name: string): string {
    const colors = ['#ef4444','#f97316','#eab308','#22c55e',
      '#3b82f6','#8b5cf6','#ec4899'];
    const idx = (name?.charCodeAt(0) || 0) % colors.length;
    return colors[idx];
  }

  isNew(createdAt: string): boolean {
    const diff = Date.now() - new Date(createdAt).getTime();
    return diff < 24 * 60 * 60 * 1000;
  }

  getTagsArr(tags: string): string[] {
    if (!tags) return [];
    return tags.split(',').filter(t => t.trim());
  }

  getTicketNum(id: string): string {
    return id.substring(0, 5).toUpperCase();
  }

  getTimeAgo(date: string): string {
    const diff = Date.now() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)} days ago`;
  }

  getStatusLabel(status: string): string {
    const map: any = {
      'Open': 'Open', 'InProgress': 'Pending',
      'Resolved': 'Resolved', 'Closed': 'Closed'
    };
    return map[status] || status;
  }

  getSlaText(t: any): string {
    if (!t.slaDeadline) return '';
    if (t.slaStatus === 'Breached') return 'SLA Breached';
    const diff = new Date(t.slaDeadline).getTime() - Date.now();
    const hrs = Math.floor(diff / 3600000);
    if (hrs < 0) return 'Overdue';
    if (hrs < 24) return `First response due in ${hrs} hours`;
    return `Due in ${Math.floor(hrs/24)} days`;
  }

  onAgentFilter(event: any) {
    // Filter by agent logic here
  }

  // Added missing toggleFilter method
  toggleFilter(filterName: string) {
    // Toggle logic for filters
  }

  viewTicket(id: string) {
    this.router.navigate(['/tickets', id]);
  }

  logout() {
    this.authService.logout();
  }

  // --- Bulk Select & Export Methods ---

  toggleSelect(id: string) {
    if (this.selectedTicketIds.has(id)) {
      this.selectedTicketIds.delete(id);
    } else {
      this.selectedTicketIds.add(id);
    }
    this.cdr.detectChanges();
  }

  selectAll() {
    if (this.selectedTicketIds.size === this.tickets.length) {
      this.selectedTicketIds.clear();
    } else {
      this.tickets.forEach(t => this.selectedTicketIds.add(t.id));
    }
    this.cdr.detectChanges();
  }

  clearSelection() {
    this.selectedTicketIds.clear();
    this.cdr.detectChanges();
  }

  bulkUpdateStatus() {
    if (!this.bulkStatus || !this.selectedTicketIds.size) return;
    this.bulkUpdating = true;
    this.cdr.detectChanges();

    this.ticketService.bulkUpdate({
      ticketIds: Array.from(this.selectedTicketIds),
      status: this.bulkStatus
    }).subscribe({
      next: (res: any) => {
        this.bulkUpdating = false;
        this.selectedTicketIds.clear();
        this.bulkStatus = '';
        this.toastr.success(res.message);
        this.loadTickets();
      },
      error: () => {
        this.bulkUpdating = false;
        this.toastr.error('Bulk update failed');
        this.cdr.detectChanges();
      }
    });
  }

  exportCsv() {
    this.ticketService.exportTickets().subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `tickets-${new Date().toISOString().split('T')[0]}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastr.success('CSV exported!');
      },
      error: () => this.toastr.error('Export failed')
    });
  }
}