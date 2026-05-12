import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-whatsapp-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './whatsapp-settings.html',
  styleUrls: ['./whatsapp-settings.scss']
})
export class WhatsappSettingsComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  config = {
    whatsAppNumber: '',
    twilioAccountSid: '',
    twilioAuthToken: ''
  };
  saving = false;
  webhookUrl = 'https://localhost:7071/api/WhatsApp/webhook';

  private getHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  ngOnInit() {
    this.http.get<any>(
      'https://localhost:7071/api/Organizations/current',
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.config.whatsAppNumber = data.whatsAppNumber || '';
        this.config.twilioAccountSid = data.twilioAccountSid || '';
        this.cdr.detectChanges();
      }
    });
  }

  save() {
    this.saving = true;
    this.http.put(
      'https://localhost:7071/api/Organizations/current',
      {
        whatsAppNumber: this.config.whatsAppNumber,
        twilioAccountSid: this.config.twilioAccountSid,
        twilioAuthToken: this.config.twilioAuthToken
      },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.saving = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('WhatsApp settings saved!')
        );
      },
      error: () => {
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  copyWebhook() {
    navigator.clipboard.writeText(this.webhookUrl);
    Promise.resolve().then(() =>
      this.toastr.success('Webhook URL copied!')
    );
  }
}