import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ToastrService } from 'ngx-toastr';
import { AgentService } from '../../../services/agent';
import { AgentGroupService } from '../../../services/agent-group';
import { LayoutComponent } from '../../../shared/layout/layout';

@Component({
  selector: 'app-agent-invite',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatProgressSpinnerModule, LayoutComponent
  ],
  templateUrl: './agent-invite.html',
  styleUrls: ['./agent-invite.scss']
})
export class AgentInviteComponent {
  private agentService = inject(AgentService);
  private groupService = inject(AgentGroupService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  loading = false;
  uploading = false;
  groups: any[] = [];
  selectedGroups: string[] = [];
  photoPreview = '';

  roles = [
    { value: 'Account Administrator', label: 'Account Administrator' },
    { value: 'Administrator', label: 'Administrator' },
    { value: 'Supervisor', label: 'Supervisor' },
    { value: 'Agent', label: 'Agent' }
  ];

  form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    role: ['Agent', Validators.required],
    signature: [''],
    photoUrl: ['']
  });

  constructor() {
    this.groupService.getAll().subscribe({
      next: (data) => {
        this.groups = data;
        this.cdr.detectChanges();
      }
    });
  }

  toggleGroup(groupId: string) {
    const idx = this.selectedGroups.indexOf(groupId);
    if (idx > -1) {
      this.selectedGroups.splice(idx, 1);
    } else {
      this.selectedGroups.push(groupId);
    }
  }

  isGroupSelected(groupId: string): boolean {
    return this.selectedGroups.includes(groupId);
  }

  onPhotoSelect(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.photoPreview = e.target.result;
      this.form.patchValue({ photoUrl: e.target.result });
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.cdr.detectChanges();

    const payload = {
      ...this.form.value,
      groupIds: this.selectedGroups
    };

    this.agentService.invite(payload).subscribe({
      next: () => {
        this.loading = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('Agent invited successfully!')
        );
        this.router.navigate(['/agents']);
      },
      error: (err: any) => {
        this.loading = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.error(err.error?.message || 'Failed to invite agent')
        );
      }
    });
  }
}