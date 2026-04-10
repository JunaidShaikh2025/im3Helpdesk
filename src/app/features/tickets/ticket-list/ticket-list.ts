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
import { StatusFilterPipe } from '../../../pipes/status-filter-pipe';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule, FormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatToolbarModule, MatProgressSpinnerModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatCardModule, MatCheckboxModule,
    StatusFilterPipe
  ],
  templateUrl: './ticket-list.html',
  styleUrls: ['./ticket-list.scss']
})
export class TicketListComponent implements OnInit {
  private ticketService = inject(TicketService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  tickets: any[] = [];
  allTickets: any[] = [];
  loading = true;
  
  displayedColumns = [
    'select', 'title', 'category', 'status', 'priority',
    'createdBy', 'assignedTo', 'sla', 'createdAt', 'actions'
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
    this.loadTickets();
    this.filterForm.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.applyFilters());
  }

  loadTickets() {
    this.loading = true;
    this.ticketService.getAll().subscribe({
      next: (data: any[]) => {
        this.allTickets = data;
        this.tickets = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load tickets');
        this.cdr.detectChanges();
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