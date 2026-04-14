import { Component, OnInit, OnDestroy, ChangeDetectorRef, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../services/chat';
import { AuthService } from '../../../services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-live-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './live-chat.html',
  styleUrls: ['./live-chat.scss']
})
export class LiveChatComponent implements OnInit, OnDestroy {
  @Input() ticketId = '';
  @Input() isAgent = false;

  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private sub!: Subscription;

  messages: any[] = [];
  newMessage = '';
  connected = false;
  senderName = '';

ngOnInit() {
  const token = this.authService.getToken();
  if (token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    this.senderName = payload[
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
    ] || payload.email || 'User';
  }

  // Try to connect
  this.chatService.connect()
    .then(() => {
      this.connected = true;
      if (this.ticketId) {
        return this.chatService.joinTicketRoom(this.ticketId);
      }
      return Promise.resolve();
    })
    .then(() => this.cdr.detectChanges())
    .catch(() => {
      // SignalR not available — use polling fallback
      this.connected = false;
      this.cdr.detectChanges();
    });

  this.sub = this.chatService.messages$.subscribe(msgs => {
    this.messages = msgs.filter(
      m => !this.ticketId || m.ticketId === this.ticketId
    );
    this.cdr.detectChanges();
    this.scrollToBottom();
  });
}

scrollToBottom() {
  setTimeout(() => {
    const el = document.querySelector('.chat-messages');
    if (el) el.scrollTop = el.scrollHeight;
  }, 50);
}

sendMessage() {
  if (!this.newMessage.trim()) return;

  if (this.connected) {
    this.chatService.sendMessage(
      this.ticketId || 'general',
      this.newMessage,
      this.senderName,
      this.isAgent
    );
  } else {
    // Fallback: show locally
    const localMsg = {
      ticketId: this.ticketId,
      message: this.newMessage,
      senderName: this.senderName,
      isAgent: this.isAgent,
      timestamp: new Date()
    };
    const current = [...this.messages, localMsg];
    this.messages = current;
    this.cdr.detectChanges();
  }

  this.newMessage = '';
  this.scrollToBottom();
}

  ngOnDestroy() {
    if (this.ticketId) {
      this.chatService.leaveTicketRoom(this.ticketId);
    }
    this.sub?.unsubscribe();
  }

  getAvatarColor(name: string): string {
    const colors = ['#ef4444','#f97316','#22c55e',
      '#3b82f6','#8b5cf6','#ec4899'];
    const idx = (name?.charCodeAt(0) || 0) % colors.length;
    return colors[idx];
  }
}