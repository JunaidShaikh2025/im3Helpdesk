import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="auth-layout">
      <div class="auth-left">
        <div class="brand">
          <div class="brand-icon">⚡</div>
          <span class="brand-name">iM3HelpDesk</span>
        </div>
        <div class="hero-content">
          <h1>EMAIL<br><span class="highlight">VERIFICATION</span></h1>
          <p>Verifying your account...</p>
        </div>
      </div>
      <div class="auth-right">
        <div class="auth-form-wrap">
          <div class="verify-state" *ngIf="status === 'loading'">
            <div class="spinner"></div>
            <h3>Verifying your email...</h3>
            <p>Please wait a moment</p>
          </div>

          <div class="success-state" *ngIf="status === 'success'">
            <div class="success-circle">✓</div>
            <h3>Email Verified!</h3>
            <p>Your account has been verified successfully.</p>
            <p class="redirect-msg">Redirecting to login in {{ countdown }}s...</p>
            <a routerLink="/login" class="submit-btn"
              style="display:block;text-align:center;text-decoration:none;margin-top:16px">
              Login Now →
            </a>
          </div>

          <div class="error-state" *ngIf="status === 'error'">
            <div class="error-circle">✗</div>
            <h3>Verification Failed</h3>
            <p>{{ errorMessage }}</p>
            <a routerLink="/login" class="submit-btn"
              style="display:block;text-align:center;text-decoration:none;margin-top:16px">
              Back to Login
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      min-height: 100vh;
      background: #0a0a0f;
    }
    .auth-left {
      padding: 40px 48px;
      background: linear-gradient(135deg, #0a0a1a 0%, #0d1b3e 50%, #12082a 100%);
      display: flex;
      flex-direction: column;
      justify-content: center;
    }
    .brand { display: flex; align-items: center; gap: 10px; margin-bottom: 48px; }
    .brand-icon {
      width: 40px; height: 40px; background: #3b82f6;
      border-radius: 10px; display: flex;
      align-items: center; justify-content: center; font-size: 18px;
    }
    .brand-name { color: white; font-size: 18px; font-weight: 600; }
    .hero-content h1 {
      font-size: 42px; font-weight: 800; color: white; line-height: 1.1;
    }
    .highlight { color: #3b82f6; }
    .hero-content p { color: rgba(255,255,255,0.6); margin-top: 12px; }
    .auth-right {
      display: flex; align-items: center; justify-content: center;
      background: #0f0f1a; padding: 40px;
    }
    .auth-form-wrap { width: 100%; max-width: 400px; text-align: center; }
    .spinner {
      width: 48px; height: 48px;
      border: 4px solid rgba(59,130,246,0.3);
      border-top-color: #3b82f6;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 20px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .verify-state h3 { color: white; margin-bottom: 8px; }
    .verify-state p { color: rgba(255,255,255,0.5); }
    .success-circle {
      width: 72px; height: 72px; background: #22c55e;
      border-radius: 50%; display: flex; align-items: center;
      justify-content: center; color: white; font-size: 28px;
      margin: 0 auto 16px;
    }
    .error-circle {
      width: 72px; height: 72px; background: #ef4444;
      border-radius: 50%; display: flex; align-items: center;
      justify-content: center; color: white; font-size: 28px;
      margin: 0 auto 16px;
    }
    h3 { color: white; margin-bottom: 8px; font-size: 22px; }
    p { color: rgba(255,255,255,0.5); font-size: 14px; }
    .redirect-msg { color: #3b82f6; margin-top: 8px; }
    .submit-btn {
      padding: 13px; background: #3b82f6; color: white;
      border: none; border-radius: 10px; font-size: 14px;
      font-weight: 600; cursor: pointer;
    }
  `]
})
export class VerifyEmailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  status: 'loading' | 'success' | 'error' = 'loading';
  errorMessage = 'Invalid or expired verification link.';
  countdown = 5;

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.status = 'error';
      return;
    }

    this.http.get(
      `https://localhost:7071/api/Auth/verify-email?token=${token}`
    ).subscribe({
      next: () => {
        this.status = 'success';
        this.startCountdown();
      },
      error: (err) => {
        this.status = 'error';
        this.errorMessage = err.error?.message
          || 'Verification failed. Link may be expired.';
      }
    });
  }

  startCountdown() {
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(interval);
        this.router.navigate(['/login']);
      }
    }, 1000);
  }
}