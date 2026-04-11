import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { LayoutComponent } from '../../../shared/layout/layout';


@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatButtonModule, MatToolbarModule, MatCardModule,
    MatSlideToggleModule, MatSelectModule,
    MatFormFieldModule, MatDividerModule,LayoutComponent
  ],
  templateUrl: './settings-page.html',
  styleUrls: ['./settings-page.scss']
})
export class SettingsPageComponent implements OnInit {
  private authService = inject(AuthService);
  public router = inject(Router);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  currentTheme = 'theme-blue';
  emailNotifications = true;
  browserNotifications = false;
  language = 'en';

  themes = [
    { id: 'theme-blue', name: 'Ocean Blue', color: '#1976d2' },
    { id: 'theme-dark', name: 'Dark Mode', color: '#1a1a2e' },
    { id: 'theme-green', name: 'Forest Green', color: '#2e7d32' },
    { id: 'theme-purple', name: 'Royal Purple', color: '#6a1b9a' }
  ];

  languages = [
    { code: 'en', name: 'English' },
    { code: 'hi', name: 'Hindi' },
    { code: 'mr', name: 'Marathi' }
  ];

  ngOnInit() {
    this.currentTheme =
      localStorage.getItem('im3_theme') || 'theme-blue';
    this.emailNotifications =
      localStorage.getItem('im3_email_notif') !== 'false';
    this.browserNotifications =
      localStorage.getItem('im3_browser_notif') === 'true';
    this.language =
      localStorage.getItem('im3_lang') || 'en';
  }

  applyTheme(themeId: string) {
    const allThemes = this.themes.map(t => t.id);
    document.body.classList.remove(...allThemes);
    document.body.classList.add(themeId);
    localStorage.setItem('im3_theme', themeId);
    this.currentTheme = themeId;
    this.cdr.detectChanges();
    this.toastr.success('Theme applied!');
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
    this.toastr.success('Language saved! Reloading...');
    setTimeout(() => window.location.reload(), 1000);
  }

  clearAllData() {
    if (!confirm('Clear all local data? You will be logged out.')) return;
    this.authService.logout();
  }

  logout() {
    this.authService.logout();
  }
}