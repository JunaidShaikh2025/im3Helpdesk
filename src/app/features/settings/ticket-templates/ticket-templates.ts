import { Component, OnInit, ChangeDetectorRef, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { TicketTemplateService } from '../../../services/ticket-template';
import { AuthService } from '../../../services/auth.service';


@Component({
  selector: 'app-ticket-templates',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatToolbarModule, MatCardModule,
    MatTableModule, MatProgressSpinnerModule
  ],
  templateUrl: './ticket-templates.html',
  styleUrls: ['./ticket-templates.scss']
})
export class TicketTemplatesComponent implements OnInit {
  private templateService = inject(TicketTemplateService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  @Input() embedded = false;
  templates: any[] = [];
  loading = true;
  saving = false;
  showForm = false;
  displayedColumns = ['name', 'title', 'category', 'priority', 'actions'];

  categories = ['General', 'Technical', 'Billing', 'Sales', 'Network'];
  priorities = ['Low', 'Medium', 'High', 'Critical'];

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    title: ['', Validators.required],
    description: ['', Validators.required],
    category: ['General', Validators.required],
    priority: ['Medium', Validators.required]
  });

  ngOnInit() {
    this.loadTemplates();
  }

  loadTemplates() {
    this.loading = true;
    this.templateService.getAll().subscribe({
      next: (data: any[]) => {
        this.templates = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  saveTemplate() {
    if (this.form.invalid) return;
    this.saving = true;
    this.cdr.detectChanges();

    this.templateService.create(this.form.value).subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.form.reset({
          category: 'General', priority: 'Medium'
        });
        this.toastr.success('Template created!');
        this.loadTemplates();
      },
      error: () => {
        this.saving = false;
        this.toastr.error('Failed to create template');
        this.cdr.detectChanges();
      }
    });
  }

  deleteTemplate(id: string) {
    if (!confirm('Delete this template?')) return;
    this.templateService.delete(id).subscribe({
      next: () => {
        this.toastr.success('Template deleted');
        this.loadTemplates();
      },
      error: () => this.toastr.error('Failed to delete')
    });
  }

  getPriorityColor(priority: string): string {
    const colors: any = {
      'Critical': '#f44336', 'High': '#ff9800',
      'Medium': '#2196f3', 'Low': '#4caf50'
    };
    return colors[priority] || '#666';
  }

  logout() {
    this.authService.logout();
  }
}