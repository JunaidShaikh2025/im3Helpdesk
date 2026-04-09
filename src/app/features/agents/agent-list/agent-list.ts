import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { ToastrService } from 'ngx-toastr';
import { AgentService } from '../../../services/agent';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatTableModule, MatButtonModule,
    MatToolbarModule, MatProgressSpinnerModule,
    MatChipsModule, MatIconModule
  ],
  templateUrl: './agent-list.html',
  styleUrls: ['./agent-list.scss']
})
export class AgentListComponent implements OnInit {
  private agentService = inject(AgentService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  agents: any[] = [];
  loading = true;
  displayedColumns = ['name', 'email', 'role', 'verified', 'lastLogin', 'actions'];

  ngOnInit() {
    this.loadAgents();
  }

  loadAgents() {
    this.loading = true;
    this.agentService.getAll().subscribe({
      next: (data: any[]) => {
        this.agents = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load agents');
        this.cdr.detectChanges();
      }
    });
  }

  deleteAgent(id: string, name: string) {
    if (!confirm(`Are you sure you want to remove ${name}?`)) return;

    this.agentService.delete(id).subscribe({
      next: () => {
        this.toastr.success('Agent removed successfully');
        this.loadAgents();
      },
      error: () => {
        this.toastr.error('Failed to remove agent');
      }
    });
  }

  getRoleColor(role: string): string {
    return role === 'CompanyAdmin' ? '#1976d2' : '#666';
  }

  logout() {
    this.authService.logout();
  }
}