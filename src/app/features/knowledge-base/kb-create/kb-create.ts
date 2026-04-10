import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

// Angular Material Imports
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card'; // <-- YAHAN ADD KIYA HAI

import { ToastrService } from 'ngx-toastr';
import { KnowledgeBaseService } from '../../../services/knowledge-base';

@Component({
  selector: 'app-kb-create',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterModule,
    MatButtonModule, 
    MatFormFieldModule, 
    MatInputModule,
    MatSelectModule, 
    MatToolbarModule, 
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatCardModule // <-- AUR YAHAN BHI ADD KIYA HAI
  ],
  templateUrl: './kb-create.html',
  styleUrls: ['./kb-create.scss']
})
export class KbCreateComponent implements OnInit {
  private kbService = inject(KnowledgeBaseService);
  public router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  loading = false;
  isEdit = false;
  articleId = '';

  categories = [
    'General', 'Technical', 'Billing',
    'Account', 'Features', 'Troubleshooting'
  ];

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    content: ['', [Validators.required, Validators.minLength(20)]],
    category: ['General', Validators.required],
    tags: [''],
    isPublished: [true]
  });

  ngOnInit() {
    this.articleId = this.route.snapshot.paramMap.get('id') || '';
    if (this.articleId) {
      this.isEdit = true;
      this.loadArticle();
    }
  }

  loadArticle() {
    this.kbService.getById(this.articleId).subscribe({
      next: (data: any) => {
        this.form.patchValue(data);
        this.cdr.detectChanges();
      }
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.cdr.detectChanges();

    const action = this.isEdit
      ? this.kbService.update(this.articleId, this.form.value)
      : this.kbService.create(this.form.value);

    action.subscribe({
      next: () => {
        this.loading = false;
        this.cdr.detectChanges();
        this.toastr.success(
          this.isEdit ? 'Article updated!' : 'Article created!'
        );
        this.router.navigate(['/kb']);
      },
      error: (err: any) => {
        this.loading = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed');
      }
    });
  }
}