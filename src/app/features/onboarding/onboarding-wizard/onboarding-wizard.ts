import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-onboarding-wizard',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatFormFieldModule,
    MatInputModule, MatStepperModule,
    MatIconModule, MatChipsModule,FormsModule
  ],
  templateUrl: './onboarding-wizard.html',
  styleUrls: ['./onboarding-wizard.scss']
})
export class OnboardingWizardComponent {
  step1Form: FormGroup;
  step2Form: FormGroup;
  step3Form: FormGroup;
  step4Form: FormGroup;

  categories: string[] = ['General', 'Technical', 'Billing', 'Sales'];
  newCategory = '';
  agentEmails: string[] = [];
  newAgentEmail = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private toastr: ToastrService
  ) {
    this.step1Form = this.fb.group({
      logoUrl: [''],
      brandColor: ['#1976d2']
    });

    this.step2Form = this.fb.group({
      supportEmail: ['', [Validators.required, Validators.email]]
    });

    this.step3Form = this.fb.group({});
    this.step4Form = this.fb.group({});
  }

  addCategory() {
    if (this.newCategory.trim()) {
      this.categories.push(this.newCategory.trim());
      this.newCategory = '';
    }
  }

  removeCategory(cat: string) {
    this.categories = this.categories.filter(c => c !== cat);
  }

  addAgentEmail() {
    if (this.newAgentEmail.trim() && !this.agentEmails.includes(this.newAgentEmail)) {
      this.agentEmails.push(this.newAgentEmail.trim());
      this.newAgentEmail = '';
    }
  }

  removeAgent(email: string) {
    this.agentEmails = this.agentEmails.filter(e => e !== email);
  }

  finish() {
    this.toastr.success('Onboarding complete! Welcome to iM3 Helpdesk!');
    this.router.navigate(['/dashboard']);
  }
}