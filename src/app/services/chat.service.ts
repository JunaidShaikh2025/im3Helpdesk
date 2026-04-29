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
  messages$ =
    new BehaviorSubject<any[]>([]);
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
    // No Content-Type — browser sets it
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

  // ── SignalR ──────────────────────────
  connect() {
    if (this.hub &&
        this.hub.state ===
          signalR.HubConnectionState.Connected)
      return;

    this.hub = new signalR
      .HubConnectionBuilder()
      .withUrl(`${this.BASE}/hubs/chat`, {
        accessTokenFactory: () =>
          this.authService.getToken() || ''
      })
      .withAutomaticReconnect([0, 2000, 5000])
      .build();

    // Messages
    this.hub.on('ReceiveMessage', (msg) => {
      const curr = this.messages$.value;
      this.messages$.next([...curr, msg]);
      this.loadUnreadCount();
    });

    // Typing
    this.hub.on('UserTyping', (d) => {
      this.typing$.next(d);
      setTimeout(() =>
        this.typing$.next(null), 3000);
    });

    // Online/Offline
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

    this.hub.onreconnected(() => {
      this.isConnected$.next(true);
      this.loadUnreadCount();
    });

    this.hub.onclose(() =>
      this.isConnected$.next(false));

    this.hub.start()
      .then(() => {
        this.isConnected$.next(true);
        this.loadUnreadCount();
      })
      .catch(e =>
        console.error('Chat hub:', e));
  }

  disconnect() {
    this.hub?.stop();
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
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'SendMessage',
      receiverId, content,
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
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'SendGroupMessage',
      groupId, content, messageType,
      attachmentUrl ?? null,
      attachmentName ?? null,
      attachmentType ?? null);
  }

  markRead(senderId: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'MarkRead', senderId);
  }

  sendTyping(
    receiverId: string,
    isTyping: boolean): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'Typing', receiverId, isTyping);
  }

  // Call methods
  initiateCall(
    receiverId: string,
    callType: string,
    offer: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'InitiateCall',
      receiverId, callType, offer);
  }

  acceptCall(
    callerId: string,
    answer: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'AcceptCall', callerId, answer);
  }

  rejectCall(callerId: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'RejectCall', callerId);
  }

  endCall(userId: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'EndCall', userId);
  }

  sendIceCandidate(
    targetId: string,
    candidate: string): Promise<void> {
    if (!this.hub) return Promise.resolve();
    return this.hub.invoke(
      'SendIceCandidate', targetId, candidate);
  }

  loadUnreadCount() {
    this.getUnreadCount().subscribe({
      next: (d) =>
        this.unreadCount$.next(d.count || 0),
      error: () => {}
    });
  }

  clearMessages() {
    this.messages$.next([]);
  }
}