import {
  Component, OnInit, OnDestroy,
  ChangeDetectorRef, inject, Input,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-live-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  // ✅ Correct template file name
  templateUrl: './live-chat.html',
  styleUrls: ['./live-chat.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LiveChatComponent implements OnInit {
  @Input() ticketId = '';
  @Input() isAgent = false;

  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  messages: any[] = [];
  newMessage = '';
  connected = false;
  senderName = '';

  ngOnInit() {
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(
          atob(token.split('.')[1]));
        this.senderName = payload[
          'http://schemas.xmlsoap.org/ws/2005/05/' +
          'identity/claims/name'
        ] || payload.email?.split('@')[0] || 'User';
      } catch {}
    }
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;

    const msg = {
      ticketId: this.ticketId,
      message: this.newMessage,
      senderName: this.senderName,
      isAgent: this.isAgent,
      timestamp: new Date()
    };

    this.messages = [...this.messages, msg];
    this.newMessage = '';
    this.cdr.markForCheck();
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#ef4444', '#f97316', '#22c55e',
      '#3b82f6', '#8b5cf6', '#ec4899'
    ];
    const idx = (name?.charCodeAt(0) || 0) % colors.length;
    return colors[idx];
  }
}