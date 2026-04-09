import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { ToastrService } from 'ngx-toastr';
import { TicketService } from '../../../services/ticket';
import { AuthService } from '../../../services/auth.service';
import { StatusFilterPipe } from '../../../pipes/status-filter-pipe';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatToolbarModule, MatProgressSpinnerModule,
    MatFormFieldModule, MatInputModule,
    MatSelectModule, MatCardModule,
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
    'title', 'category', 'status',
    'priority', 'createdBy', 'assignedTo',
    'createdAt', 'actions'
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
}