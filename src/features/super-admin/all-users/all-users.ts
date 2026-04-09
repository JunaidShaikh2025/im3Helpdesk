import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SuperAdminService } from '../../../services/super-admin';
import { AuthService } from '../../../app/services/auth.service';


@Component({
  selector: 'app-all-users',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatButtonModule, MatToolbarModule,
    MatTableModule, MatProgressSpinnerModule,
    MatFormFieldModule, MatInputModule
  ],
  templateUrl: './all-users.html',
  styleUrls: ['./all-users.scss']
})
export class AllUsersComponent implements OnInit {
  private superAdminService = inject(SuperAdminService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  allUsers: any[] = [];
  filteredUsers: any[] = [];
  loading = true;
  searchQuery = '';
  displayedColumns = [
    'name', 'email', 'role',
    'organization', 'verified', 'lastLogin'
  ];

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;
    this.superAdminService.getAllUsers().subscribe({
      next: (data: any[]) => {
        this.allUsers = data;
        this.filteredUsers = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load users');
        this.cdr.detectChanges();
      }
    });
  }

  search() {
    const q = this.searchQuery.toLowerCase();
    this.filteredUsers = this.allUsers.filter(u =>
      u.fullName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      u.organization?.name?.toLowerCase().includes(q)
    );
    this.cdr.detectChanges();
  }

  getRoleColor(role: string): string {
    const colors: any = {
      'SuperAdmin': '#9c27b0',
      'CompanyAdmin': '#1976d2',
      'Agent': '#009688',
      'Customer': '#666'
    };
    return colors[role] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}