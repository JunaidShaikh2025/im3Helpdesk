import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatButtonModule, MatToolbarModule, MatCardModule,
    MatSlideToggleModule, MatSelectModule,
    MatFormFieldModule, MatDividerModule,FormsModule
  ],
  templateUrl: './settings-page.html',
  styleUrls: ['./settings-page.scss']
})
export class SettingsPageComponent implements OnInit {
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  isDarkMode = false;
  emailNotifications = true;
  browserNotifications = false;
  language = 'en';

languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Spanish' },
    { code: 'fr', name: 'French' },
    { code: 'zh', name: 'Chinese' },
     { code: 'hi', name: 'Hindi' }
  ];

  ngOnInit() {
    this.isDarkMode = localStorage.getItem('im3_dark') === 'true';
    this.emailNotifications =
      localStorage.getItem('im3_email_notif') !== 'false';
    this.browserNotifications =
      localStorage.getItem('im3_browser_notif') === 'true';
    this.language =
      localStorage.getItem('im3_lang') || 'en';

    if (this.isDarkMode) {
      document.body.classList.add('dark-mode');
    }
  }

  toggleDarkMode() {
    this.isDarkMode = !this.isDarkMode;
    localStorage.setItem('im3_dark', this.isDarkMode.toString());
    if (this.isDarkMode) {
      document.body.classList.add('dark-mode');
    } else {
      document.body.classList.remove('dark-mode');
    }
    this.toastr.success(
      this.isDarkMode ? 'Dark mode enabled' : 'Light mode enabled'
    );
    this.cdr.detectChanges();
  }

  saveNotificationSettings() {
    localStorage.setItem(
      'im3_email_notif', this.emailNotifications.toString());
    localStorage.setItem(
      'im3_browser_notif', this.browserNotifications.toString());
    this.toastr.success('Notification settings saved!');
  }

  saveLanguage() {
    localStorage.setItem('im3_lang', this.language);
    this.toastr.success('Language preference saved!');
  }

  clearAllData() {
    if (!confirm('Clear all local data? You will be logged out.')) return;
    this.authService.logout();
  }

  logout() {
    this.authService.logout();
  }
}