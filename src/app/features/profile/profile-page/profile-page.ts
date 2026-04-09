import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { ToastrService } from 'ngx-toastr';
import { ProfileService } from '../../../services/profile';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatFormFieldModule, MatInputModule,
    MatToolbarModule, MatCardModule, MatTabsModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './profile-page.html',
  styleUrls: ['./profile-page.scss']
})
export class ProfilePageComponent implements OnInit {
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  profile: any = null;
  loading = true;
  savingProfile = false;
  savingPassword = false;
  savingOrg = false;

  profileForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    phoneNumber: ['']
  });

  passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmNewPassword: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  orgForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    supportEmail: ['', Validators.email],
    logoUrl: [''],
    brandColor: ['#1976d2']
  });

  passwordMatchValidator(control: AbstractControl) {
    const np = control.get('newPassword')?.value;
    const cp = control.get('confirmNewPassword')?.value;
    if (np !== cp) {
      control.get('confirmNewPassword')?.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    return null;
  }

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.loading = true;
    this.profileService.getProfile().subscribe({
      next: (data: any) => {
        this.profile = data;
        this.profileForm.patchValue({
          fullName: data.fullName,
          phoneNumber: data.phoneNumber || ''
        });
        if (data.organization) {
          this.orgForm.patchValue({
            name: data.organization.name || '',
            supportEmail: data.organization.supportEmail || '',
            logoUrl: data.organization.logoUrl || '',
            brandColor: data.organization.brandColor || '#1976d2'
          });
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.toastr.error('Failed to load profile');
        this.cdr.detectChanges();
      }
    });
  }

  saveProfile() {
    if (this.profileForm.invalid) return;
    this.savingProfile = true;
    this.cdr.detectChanges();

    this.profileService.updateProfile(this.profileForm.value).subscribe({
      next: () => {
        this.savingProfile = false;
        this.cdr.detectChanges();
        this.toastr.success('Profile updated!');
      },
      error: (err: any) => {
        this.savingProfile = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed to update profile');
      }
    });
  }

  changePassword() {
    if (this.passwordForm.invalid) return;
    this.savingPassword = true;
    this.cdr.detectChanges();

    this.profileService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.savingPassword = false;
        this.passwordForm.reset();
        this.cdr.detectChanges();
        this.toastr.success('Password changed successfully!');
      },
      error: (err: any) => {
        this.savingPassword = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed to change password');
      }
    });
  }

  saveOrganization() {
    if (this.orgForm.invalid) return;
    this.savingOrg = true;
    this.cdr.detectChanges();

    this.profileService.updateOrganization(this.orgForm.value).subscribe({
      next: () => {
        this.savingOrg = false;
        this.cdr.detectChanges();
        this.toastr.success('Organization updated!');
      },
      error: (err: any) => {
        this.savingOrg = false;
        this.cdr.detectChanges();
        this.toastr.error(err.error?.message || 'Failed to update organization');
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}