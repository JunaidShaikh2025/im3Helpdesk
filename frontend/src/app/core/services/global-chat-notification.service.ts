// Microsoft Teams-style global chat toast notifications.
// Listens to ChatService.newMessage$ and surfaces a stack of
// dismissible toasts in the top-right corner whenever a chat message
// arrives — for direct messages, group chats, and ticket-room chats.
// Suppresses the toast when the user is already viewing that thread.

import { Injectable, OnDestroy, inject, signal, computed } from '@angular/core';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { ChatService } from './chat.service';

export interface ChatToast {
  id: string;
  kind: 'dm' | 'group';
  /** Conversation key — userId for DM, groupId for group. */
  threadId: string;
  senderId: string;
  senderName: string;
  senderPhoto?: string | null;
  groupName?: string;
  preview: string;
  isAttachment: boolean;
  attachmentType?: string | null;
  receivedAt: number;
}

@Injectable({ providedIn: 'root' })
export class GlobalChatNotificationService implements OnDestroy {
  private chat = inject(ChatService);
  private router = inject(Router);

  /** Visible toast stack (newest first). Capped at MAX_VISIBLE. */
  readonly toasts = signal<ChatToast[]>([]);
  /** Total accumulated unread popups badge — informational only. */
  readonly recentCount = computed(() => this.toasts().length);

  private readonly MAX_VISIBLE = 4;
  /** Auto-dismiss after this many ms unless user hovers/replies. */
  private readonly AUTO_DISMISS_MS = 8000;

  private subs: Subscription[] = [];
  private timers = new Map<string, any>();
  private initialized = false;
  private audioCtx: AudioContext | null = null;
  private unlockHandlersBound = false;
  private lastSoundAt = 0;

  /** Wire up SignalR subscription. Safe to call multiple times. */
  init(): void {
    if (this.initialized) return;
    this.initialized = true;
    this.bindAudioUnlockHandlers();

    this.subs.push(
      this.chat.newMessage$.subscribe(msg => this.onIncomingMessage(msg))
    );
    this.subs.push(
      this.chat.currentlyViewing$.subscribe(view => {
        if (!view) return;
        // If the user opens the thread, clear any pending toast for it.
        this.toasts.update(list =>
          list.filter(t => !(t.kind === view.kind && t.threadId === view.id)));
      })
    );
  }

  private onIncomingMessage(msg: any): void {
    if (!msg) return;
    // Ignore own messages (hub stamps IsFromMe=false on receiver side).
    if (msg.isFromMe === true || msg.IsFromMe === true) return;
    const senderId = String(msg.senderId ?? msg.SenderId ?? '');
    if (!senderId) return;

    const isGroup = !!(msg.groupId ?? msg.GroupId);
    const groupId = String(msg.groupId ?? msg.GroupId ?? '');
    // For DMs the thread key from the recipient's POV is the sender.
    const threadId = isGroup ? groupId : senderId;
    const kind: 'dm' | 'group' = isGroup ? 'group' : 'dm';

    // Suppress if the user is currently viewing this thread.
    const view = this.chat.currentlyViewing$.value;
    if (view && view.kind === kind && view.id === threadId) return;

    const toast: ChatToast = {
      id: String(msg.id ?? msg.Id ?? `${Date.now()}-${Math.random()}`),
      kind,
      threadId,
      senderId,
      senderName: String(
        msg.senderName ?? msg.SenderName ?? 'New message'),
      senderPhoto: msg.senderPhoto ?? msg.SenderPhoto ?? null,
      groupName: msg.groupName ?? msg.GroupName ?? undefined,
      preview: this.buildPreview(msg),
      isAttachment: !!(msg.attachmentUrl ?? msg.AttachmentUrl),
      attachmentType: msg.attachmentType ?? msg.AttachmentType ?? null,
      receivedAt: Date.now(),
    };

    // Coalesce: if a toast for the same thread already exists, replace it.
    this.toasts.update(list => {
      const filtered = list.filter(
        t => !(t.kind === toast.kind && t.threadId === toast.threadId));
      return [toast, ...filtered].slice(0, this.MAX_VISIBLE);
    });

    this.armAutoDismiss(toast.id);
    this.playMessageSound();
  }

  private bindAudioUnlockHandlers(): void {
    if (this.unlockHandlersBound) return;
    this.unlockHandlersBound = true;

    const unlock = () => {
      this.ensureAudioContext().catch(() => {});
    };

    window.addEventListener('pointerdown', unlock, { passive: true, capture: true });
    window.addEventListener('keydown', unlock, { capture: true });
  }

  private async ensureAudioContext(): Promise<AudioContext | null> {
    try {
      const Ctor = (window as any).AudioContext || (window as any).webkitAudioContext;
      if (!Ctor) return null;
      if (!this.audioCtx || this.audioCtx.state === 'closed') {
        this.audioCtx = new Ctor();
      }
      const ctx = this.audioCtx;
      if (!ctx) return null;
      if (ctx.state === 'suspended') {
        await ctx.resume();
      }
      return ctx;
    } catch {
      return null;
    }
  }

  private playMessageSound(): void {
    const now = Date.now();
    // Avoid noisy burst when many events land together.
    if (now - this.lastSoundAt < 700) return;
    this.lastSoundAt = now;

    this.ensureAudioContext().then(ctx => {
      if (!ctx) return;
      try {
        // Two short, soft pings (Teams-like).
        this.playTone(ctx, 880, 0.045, 0);
        this.playTone(ctx, 1175, 0.05, 0.11);
      } catch {}
    }).catch(() => {});
  }

  private playTone(
    ctx: AudioContext,
    frequency: number,
    duration: number,
    delaySec: number
  ): void {
    const t0 = ctx.currentTime + delaySec;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = 'sine';
    osc.frequency.setValueAtTime(frequency, t0);
    gain.gain.setValueAtTime(0.0001, t0);
    gain.gain.exponentialRampToValueAtTime(0.08, t0 + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, t0 + duration);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start(t0);
    osc.stop(t0 + duration + 0.02);
  }

  private buildPreview(msg: any): string {
    const content: string = String(msg.content ?? msg.Content ?? '').trim();
    if (content) {
      const stripped = content
        .replace(/<[^>]+>/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
      return stripped.length > 140 ? stripped.slice(0, 137) + '…' : stripped;
    }
    const name = msg.attachmentName ?? msg.AttachmentName;
    const type = msg.attachmentType ?? msg.AttachmentType;
    if (name) return `📎 ${name}`;
    if (type?.startsWith('image/')) return '🖼️ Sent a picture';
    if (type?.startsWith('audio/')) return '🎤 Voice message';
    if (type?.startsWith('video/')) return '🎬 Video message';
    return 'Sent an attachment';
  }

  private armAutoDismiss(id: string): void {
    const prev = this.timers.get(id);
    if (prev) clearTimeout(prev);
    const t = setTimeout(() => this.dismiss(id), this.AUTO_DISMISS_MS);
    this.timers.set(id, t);
  }

  pauseAutoDismiss(id: string): void {
    const t = this.timers.get(id);
    if (t) { clearTimeout(t); this.timers.delete(id); }
  }
  resumeAutoDismiss(id: string): void { this.armAutoDismiss(id); }

  dismiss(id: string): void {
    const t = this.timers.get(id);
    if (t) { clearTimeout(t); this.timers.delete(id); }
    this.toasts.update(list => list.filter(x => x.id !== id));
  }

  dismissAll(): void {
    this.timers.forEach(t => clearTimeout(t));
    this.timers.clear();
    this.toasts.set([]);
  }

  /** Open the chat page focused on the toast's thread, then dismiss. */
  openThread(toast: ChatToast): void {
    const queryParams = toast.kind === 'group'
      ? { groupId: toast.threadId }
      : { userId: toast.threadId };
    this.router.navigate(['/chat'], { queryParams });
    this.dismiss(toast.id);
  }

  /** Send a quick reply directly from the toast. */
  async quickReply(toast: ChatToast, text: string): Promise<void> {
    const trimmed = (text ?? '').trim();
    if (!trimmed) return;
    try {
      if (toast.kind === 'group') {
        await this.chat.sendGroupMessage(toast.threadId, trimmed);
      } else {
        await this.chat.sendMessage(toast.threadId, trimmed);
      }
    } finally {
      this.dismiss(toast.id);
    }
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.subs = [];
    this.dismissAll();
    try { this.audioCtx?.close(); } catch {}
    this.audioCtx = null;
  }
}
