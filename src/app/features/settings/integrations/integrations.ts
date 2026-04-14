import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-integrations',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './integrations.html',
  styleUrls: ['./integrations.scss']
})
export class IntegrationsComponent implements OnInit {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private toastr = inject(ToastrService);
  private cdr = inject(ChangeDetectorRef);

  slackWebhookUrl = '';
  teamsWebhookUrl = '';
  saving = false;

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
        this.slackWebhookUrl = data.slackWebhookUrl || '';
        this.teamsWebhookUrl = data.teamsWebhookUrl || '';
        this.cdr.detectChanges();
      }
    });
  }

  saveSlack() {
    this.saving = true;
    this.http.put(
      'https://localhost:7071/api/Organizations/current',
      { slackWebhookUrl: this.slackWebhookUrl },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.saving = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('Slack configured!')
        );
      }
    });
  }

  saveTeams() {
    this.saving = true;
    this.http.put(
      'https://localhost:7071/api/Organizations/current',
      { teamsWebhookUrl: this.teamsWebhookUrl },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () => {
        this.saving = false;
        this.cdr.detectChanges();
        Promise.resolve().then(() =>
          this.toastr.success('Teams configured!')
        );
      }
    });
  }

  testSlack() {
    const token = this.authService.getToken();
    if (!token) return;
    const payload = JSON.parse(atob(token.split('.')[1]));
    const orgId = payload.organizationId;

    this.http.post(
      'https://localhost:7071/api/Slack/notify',
      {
        orgId,
        message: 'Test notification from iM3 Helpdesk!',
        ticketTitle: 'Test Ticket',
        status: 'Open'
      },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () =>
        Promise.resolve().then(() =>
          this.toastr.success('Test sent to Slack!')
        ),
      error: () =>
        Promise.resolve().then(() =>
          this.toastr.error('Failed. Check webhook URL.')
        )
    });
  }

  testTeams() {
    const token = this.authService.getToken();
    if (!token) return;
    const payload = JSON.parse(atob(token.split('.')[1]));
    const orgId = payload.organizationId;

    this.http.post(
      'https://localhost:7071/api/Slack/teams/notify',
      {
        orgId,
        message: 'Test notification from iM3 Helpdesk!',
        ticketTitle: 'Test Ticket',
        status: 'Open'
      },
      { headers: this.getHeaders() }
    ).subscribe({
      next: () =>
        Promise.resolve().then(() =>
          this.toastr.success('Test sent to Teams!')
        ),
      error: () =>
        Promise.resolve().then(() =>
          this.toastr.error('Failed. Check webhook URL.')
        )
    });
  }
}