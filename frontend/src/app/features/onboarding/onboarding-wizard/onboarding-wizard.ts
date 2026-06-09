import { Component, ChangeDetectorRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-onboarding-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './onboarding-wizard.html',
  styleUrls: ['./onboarding-wizard.scss']
})
export class OnboardingWizardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);

  currentStep = 1;
  totalSteps = 3;
  loading = false;
  logoPreview = '';
  orgName = '';
  orgLoaded = false;

  // SMTP test state
  smtpTesting = false;
  smtpTestResult: 'idle' | 'ok' | 'error' = 'idle';
  smtpTestMessage = '';

  step1Form: FormGroup = this.fb.group({
    supportEmail: ['', [Validators.required, Validators.email]],
    brandColor: ['#2563eb'],
    logoUrl: ['']
  });

  step2Form: FormGroup = this.fb.group({
    smtpHost: ['smtp.gmail.com', Validators.required],
    smtpPort: [587, [Validators.required, Validators.min(1)]],
    smtpFromEmail: ['', [Validators.required, Validators.email]],
    smtpFromName: [''],
    smtpUsername: ['', [Validators.required, Validators.email]],
    smtpPassword: ['', Validators.required],
    imapHost: ['imap.gmail.com', Validators.required],
    imapPort: [993, [Validators.required, Validators.min(1)]],
    emailPollingEnabled: [true]
  });

  step3Form: FormGroup = this.fb.group({
    timezone: ['Asia/Kolkata'],
    inviteEmail: ['', Validators.email],
    inviteName: ['']
  });

  ngOnInit() {
    const step = String(this.route.snapshot.queryParamMap.get('step') || '').toLowerCase();
    if (step === 'mail' || step === 'smtp') {
      this.currentStep = 2;
    }

    this.http.get<any>(`${environment.apiUrl}/Organizations/current`).subscribe({
      next: (org) => {
        this.orgLoaded = true;
        this.orgName = String(org?.name || org?.Name || '');

        const logoUrl = String(org?.logoUrl || '');
        this.logoPreview = logoUrl;

        this.step1Form.patchValue({
          supportEmail: org?.supportEmail || '',
          brandColor: org?.brandColor || '#2563eb',
          logoUrl: logoUrl
        }, { emitEvent: false });

        this.step2Form.patchValue({
          smtpHost: org?.smtpHost || 'smtp.gmail.com',
          smtpPort: org?.smtpPort || 587,
          smtpFromEmail: org?.smtpFromEmail || org?.supportEmail || '',
          smtpFromName: org?.smtpFromName || this.orgName || '',
          smtpUsername: org?.smtpUsername || org?.smtpFromEmail || org?.supportEmail || '',
          imapHost: org?.imapHost || 'imap.gmail.com',
          imapPort: org?.imapPort || 993,
          emailPollingEnabled: org?.emailPollingEnabled !== false
        }, { emitEvent: false });

        this.step3Form.patchValue({
          timezone: org?.timezone || 'Asia/Kolkata'
        }, { emitEvent: false });

        this.cdr.detectChanges();
      },
      error: () => {
        this.orgLoaded = true;
        this.cdr.detectChanges();
      }
    });
  }

  onLogoSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      const result = String(e.target?.result || '');
      this.logoPreview = result;
      this.step1Form.patchValue({ logoUrl: result });
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }

  goToMailStep() {
    if (this.step1Form.invalid) {
      this.step1Form.markAllAsTouched();
      return;
    }

    const supportEmail = this.step1Form.value.supportEmail || '';
    this.step2Form.patchValue({
      smtpFromEmail: supportEmail,
      smtpUsername: supportEmail,
      smtpFromName: this.orgName || 'Support'
    });
    this.currentStep = 2;
  }

  testSmtp() {
    const v = this.step2Form.value;
    if (!v.smtpHost || !v.smtpUsername) return;
    this.smtpTesting = true;
    this.smtpTestResult = 'idle';
    this.smtpTestMessage = '';
    this.cdr.detectChanges();

    this.http.post<any>(`${environment.apiUrl}/Organizations/test-smtp`, {
      smtpHost: v.smtpHost,
      smtpPort: v.smtpPort,
      smtpUsername: v.smtpUsername,
      smtpPassword: v.smtpPassword || undefined,
      smtpFromEmail: v.smtpFromEmail || v.smtpUsername,
    }).subscribe({
      next: (res) => {
        this.smtpTesting = false;
        this.smtpTestResult = 'ok';
        this.smtpTestMessage = res?.message || 'Connection successful!';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.smtpTesting = false;
        this.smtpTestResult = 'error';
        this.smtpTestMessage = err?.error?.error || err?.error?.message || 'Connection failed.';
        this.cdr.detectChanges();
      }
    });
  }

  goToStep3() {
    if (this.step2Form.invalid) {
      this.step2Form.markAllAsTouched();
      return;
    }
    this.saveStep2().subscribe({
      next: () => { this.currentStep = 3; this.cdr.detectChanges(); },
      error: () => { this.currentStep = 3; this.cdr.detectChanges(); }
    });
  }

  skipMailbox() {
    this.currentStep = 3;
    this.cdr.detectChanges();
  }

  private saveStep2() {
    const v = this.step2Form.value;
    const payload: any = {
      smtpHost: v.smtpHost, smtpPort: Number(v.smtpPort),
      smtpFromEmail: v.smtpFromEmail, smtpFromName: v.smtpFromName || 'Support',
      smtpUsername: v.smtpUsername, imapHost: v.imapHost,
      imapPort: Number(v.imapPort), emailPollingEnabled: v.emailPollingEnabled === true
    };
    if (v.smtpPassword) payload.smtpPassword = v.smtpPassword;
    return this.http.put(`${environment.apiUrl}/Organizations/current`, payload);
  }

  finish() {
    this.loading = true;
    this.cdr.detectChanges();

    const s3 = this.step3Form.value;
    const payload: any = {
      supportEmail: this.step1Form.value.supportEmail,
      brandColor: this.step1Form.value.brandColor || '#2563eb',
      logoUrl: this.step1Form.value.logoUrl || '',
      timezone: s3.timezone || 'Asia/Kolkata'
    };

    this.http.put(`${environment.apiUrl}/Organizations/current`, payload).subscribe({
      next: () => {
        // Invite agent if email provided
        const inviteEmail = (s3.inviteEmail || '').trim();
        if (inviteEmail) {
          this.http.post(`${environment.apiUrl}/Agents/invite`, {
            email: inviteEmail,
            fullName: (s3.inviteName || '').trim() || inviteEmail.split('@')[0],
            role: 'Agent'
          }).subscribe({ error: () => {} }); // non-blocking
        }
        this.completeOnboarding('Workspace setup complete! Welcome aboard 🎉');
      },
      error: (err) => {
        this.loading = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.error(err.error?.message || 'Setup failed')
        );
      }
    });
  }

  private completeOnboarding(successMessage: string) {
    this.loading = true;
    this.cdr.detectChanges();

    this.http.post(
      `${environment.apiUrl}/Organizations/current/complete-onboarding`,
      {}
    ).subscribe({
      next: () => {
        this.loading = false;
        this.authService.markFirstLoginComplete();
        this.cdr.detectChanges();
        Promise.resolve().then(() => this.toastr.success(successMessage));
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.error(err.error?.message || 'Could not complete onboarding')
        );
      }
    });
  }
}