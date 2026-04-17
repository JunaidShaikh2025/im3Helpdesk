import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { KnowledgeBaseService } from '../../../services/knowledge-base';
import { AuthService } from '../../../services/auth.service';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-kb-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatButtonModule, MatToolbarModule,
    MatCardModule, MatProgressSpinnerModule,LayoutComponent
  ],
  templateUrl: './kb-detail.html',
  styleUrls: ['./kb-detail.scss']
})
export class KbDetailComponent implements OnInit {
  private kbService = inject(KnowledgeBaseService);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  public router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  article: any = null;
  loading = true;
  userRole = '';

  ngOnInit() {
    this.userRole = this.authService.getUserRole();
    const id = this.route.snapshot.paramMap.get('id') || '';
    this.kbService.getById(id).subscribe({
      next: (data: any) => {
        this.article = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  canManage(): boolean {
    return this.userRole === 'CompanyAdmin' ||
      this.userRole === 'Agent';
  }

  getBackRoute(): string {
    return this.userRole === 'Customer' ? '/customer' : '/kb';
  }

  logout() {
    this.authService.logout();
  }
}