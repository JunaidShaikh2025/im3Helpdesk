import {
  Injectable, inject
} from '@angular/core';
import {
  HttpClient, HttpHeaders
} from '@angular/common/http';
import {
  BehaviorSubject, Observable
} from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService }
  from './auth.service';

@Injectable({ providedIn: 'root' })
export class ChatService {

  private http = inject(HttpClient);
  private authService = inject(AuthService);

  readonly BASE = 'https://localhost:7071';
  private hub!: signalR.HubConnection;

  // ── Reactive streams ─────────────────
  // Single new message stream — only for
  // real-time incoming messages, NOT history
  newMessage$ =
    new BehaviorSubject<any>(null);
  typing$ =
    new BehaviorSubject<any>(null);
  userStatus$ =
    new BehaviorSubject<any>(null);
  unreadCount$ =
    new BehaviorSubject<number>(0);
  isConnected$ =
    new BehaviorSubject<boolean>(false);

  // Call streams
  incomingCall$ =
    new BehaviorSubject<any>(null);
  callAccepted$ =
    new BehaviorSubject<any>(null);
  callRejected$ =
    new BehaviorSubject<any>(null);
  callEnded$ =
    new BehaviorSubject<any>(null);
  iceCandidate$ =
    new BehaviorSubject<any>(null);

  // ── Connection state check ────────────
  get isConnected(): boolean {
    return !!this.hub &&
      this.hub.state ===
        signalR.HubConnectionState.Connected;
  }

  getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization':
        `Bearer ${this.authService.getToken()}`
    });
  }

  // ── API calls ────────────────────────
  getChatUsers(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.BASE}/api/Chat/users`,
      { headers: this.getHeaders() });
  }

  getMessages(
    userId: string,
    page = 1): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.BASE}/api/Chat/messages/${userId}` +
      `?page=${page}&pageSize=50`,
      { headers: this.getHeaders() });
  }

  getGroups(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.BASE}/api/Chat/groups`,
      { headers: this.getHeaders() });
  }

  getGroupMessages(
    groupId: string,
    page = 1): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.BASE}/api/Chat/group/` +
      `${groupId}/messages?page=${page}`,
      { headers: this.getHeaders() });
  }

  createGroup(dto: {
    name: string;
    description?: string;
    memberIds: string[];
  }): Observable<any> {
    return this.http.post<any>(
      `${this.BASE}/api/Chat/groups`,
      dto,
      { headers: this.getHeaders() });
  }

  uploadFile(file: File): Observable<any> {
    const fd = new FormData();
    fd.append('file', file);
    const h = new HttpHeaders({
      'Authorization':
        `Bearer ${this.authService.getToken()}`
    });
    return this.http.post<any>(
      `${this.BASE}/api/Chat/upload`,
      fd, { headers: h });
  }

  getUnreadCount(): Observable<any> {
    return this.http.get<any>(
      `${this.BASE}/api/Chat/unread-count`,
      { headers: this.getHeaders() });
  }

  addGroupMembers(
    groupId: string,
    memberIds: string[]
  ): Observable<any> {
    return this.http.post<any>(
      `${this.BASE}/api/Chat/groups/${groupId}/members`,
      { memberIds },
      { headers: this.getHeaders() });
  }

  loadUnreadCount() {
    this.getUnreadCount().subscribe({
      next: (d) =>
        this.unreadCount$.next(d.count || 0),
      error: () => {}
    });
  }

  clearMessages() {
    this.newMessage$.next(null);
  }

  // ── SignalR ──────────────────────────
  connect() {
    if (this.isConnected) return;

    this.hub = new signalR
      .HubConnectionBuilder()
      .withUrl(`${this.BASE}/hubs/chat`, {
        accessTokenFactory: () =>
          this.authService.getToken() || ''
      })
      .withAutomaticReconnect([
        0, 2000, 5000, 10000, 30000
      ])
      .build();

    // Messages — emit single new message
    this.hub.on('ReceiveMessage', (msg) => {
      this.newMessage$.next(msg);
      this.loadUnreadCount();
    });

    // Typing
    this.hub.on('UserTyping', (d) => {
      this.typing$.next(d);
      setTimeout(() =>
        this.typing$.next(null), 3000);
    });

    // Online / Offline
    this.hub.on('UserOnline', (d) =>
      this.userStatus$.next(
        { ...d, isOnline: true }));

    this.hub.on('UserOffline', (d) =>
      this.userStatus$.next(
        { ...d, isOnline: false }));

    this.hub.on('MessagesRead', () =>
      this.loadUnreadCount());

    // Call signals
    this.hub.on('IncomingCall', (d) =>
      this.incomingCall$.next(d));
    this.hub.on('CallAccepted', (d) =>
      this.callAccepted$.next(d));
    this.hub.on('CallRejected', (d) =>
      this.callRejected$.next(d));
    this.hub.on('CallEnded', (d) =>
      this.callEnded$.next(d));
    this.hub.on('IceCandidate', (d) =>
      this.iceCandidate$.next(d));

    // Reconnecting — update state
    this.hub.onreconnecting(() => {
      this.isConnected$.next(false);
    });

    this.hub.onreconnected(() => {
      this.isConnected$.next(true);
      this.loadUnreadCount();
    });

    this.hub.onclose(() => {
      this.isConnected$.next(false);
    });

    this.hub.start()
      .then(() => {
        this.isConnected$.next(true);
        this.loadUnreadCount();
      })
      .catch(e =>
        console.error('Chat hub error:', e));
  }

  disconnect() {
    this.hub?.stop();
  }

  // ── Hub invoke — safe wrapper ─────────
  private safeInvoke(
    method: string,
    ...args: any[]
  ): Promise<void> {
    if (!this.isConnected) {
      // Silently ignore — don't throw
      return Promise.resolve();
    }
    return this.hub.invoke(method, ...args)
      .catch(e => {
        // Only log non-state errors
        if (!e?.message?.includes(
            'not in the \'Connected\''))
          console.error(
            `Hub.${method} error:`, e);
      });
  }

  // ── Hub methods ──────────────────────
  sendMessage(
    receiverId: string,
    content: string,
    messageType = 'text',
    attachmentUrl?: string,
    attachmentName?: string,
    attachmentType?: string
  ): Promise<void> {
    return this.safeInvoke(
      'SendMessage',
      receiverId,
      content,
      messageType,
      attachmentUrl ?? null,
      attachmentName ?? null,
      attachmentType ?? null);
  }

  sendGroupMessage(
    groupId: string,
    content: string,
    messageType = 'text',
    attachmentUrl?: string,
    attachmentName?: string,
    attachmentType?: string
  ): Promise<void> {
    return this.safeInvoke(
      'SendGroupMessage',
      groupId,
      content,
      messageType,
      attachmentUrl ?? null,
      attachmentName ?? null,
      attachmentType ?? null);
  }

  markRead(senderId: string): Promise<void> {
    return this.safeInvoke(
      'MarkRead', senderId);
  }

  sendTyping(
    receiverId: string,
    isTyping: boolean
  ): Promise<void> {
    return this.safeInvoke(
      'Typing', receiverId, isTyping);
  }

  // Call methods
  initiateCall(
    receiverId: string,
    callType: string,
    offer: string
  ): Promise<void> {
    return this.safeInvoke(
      'InitiateCall',
      receiverId, callType, offer);
  }

  acceptCall(
    callerId: string,
    answer: string
  ): Promise<void> {
    return this.safeInvoke(
      'AcceptCall', callerId, answer);
  }

  rejectCall(
    callerId: string
  ): Promise<void> {
    return this.safeInvoke(
      'RejectCall', callerId);
  }

  endCall(userId: string): Promise<void> {
    return this.safeInvoke(
      'EndCall', userId);
  }

  sendIceCandidate(
    targetId: string,
    candidate: string
  ): Promise<void> {
    return this.safeInvoke(
      'SendIceCandidate',
      targetId, candidate);
  }
}