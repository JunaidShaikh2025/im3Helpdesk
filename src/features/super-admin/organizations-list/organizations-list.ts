import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { ToastrService } from 'ngx-toastr';
import { SuperAdminService } from '../../../services/super-admin';
import { AuthService } from '../../../app/services/auth.service';
import { ActiveFilterPipe } from '../../../app/pipes/active-filter-pipe';




@Component({
  selector: 'app-organizations-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatTableModule, MatProgressSpinnerModule,
    MatCardModule,ActiveFilterPipe
  ],
  templateUrl: './organizations-list.html',
  styleUrls: ['./organizations-list.scss']
})
export class OrganizationsListComponent implements OnInit {
  private superAdminService = inject(SuperAdminService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  organizations: any[] = [];
  loading = true;
  displayedColumns = [
    'name', 'slug', 'supportEmail',
    'users', 'tickets', 'trial',
    'status', 'actions'
  ];

  ngOnInit() {
    this.loadOrganizations();
  }

  loadOrganizations() {
    this.loading = true;
    this.superAdminService.getOrganizations().subscribe({
      next: (data: any[]) => {
        this.organizations = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load organizations');
        this.cdr.detectChanges();
      }
    });
  }

  toggleOrg(id: string, name: string) {
    this.superAdminService.toggleOrganization(id).subscribe({
      next: (res: any) => {
        const org = this.organizations.find(o => o.id === id);
        if (org) {
          org.isActive = res.isActive;
          this.cdr.detectChanges();
        }
        this.toastr.success(res.message);
      },
      error: () => {
        this.toastr.error('Failed to toggle organization');
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}