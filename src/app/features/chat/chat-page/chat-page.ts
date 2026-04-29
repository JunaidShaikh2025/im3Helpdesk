import {
  Component, OnInit, OnDestroy,
  ChangeDetectorRef, inject,
  ViewChild, ElementRef,
  AfterViewChecked
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ChatService }
  from '../../../services/chat.service';
import { AuthService }
  from '../../../services/auth.service';
import { LayoutComponent }
  from '../../../shared/layout/layout';

type FilterType =
  'all' | 'unread' | 'online' | 'groups';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LayoutComponent
  ],
  templateUrl: './chat-page.html',
  styleUrls: ['./chat-page.scss']
})
export class ChatPageComponent
  implements OnInit, OnDestroy,
  AfterViewChecked {

  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  public router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  @ViewChild('messagesContainer')
    msgContainer!: ElementRef;
  @ViewChild('fileInput')
    fileInput!: ElementRef;

  // State
  users: any[] = [];
  groups: any[] = [];
  filteredItems: any[] = [];
  selectedUser: any = null;
  selectedGroup: any = null;
  messages: any[] = [];
  newMessage = '';
  searchQuery = '';
  activeFilter: FilterType = 'all';
  loadingUsers = true;
  loadingMessages = false;
  isTyping = false;
  typingTimeout: any;
  shouldScrollToBottom = false;
  uploadingFile = false;

  myId = '';
  myName = '';

  // Create group modal
  showCreateGroup = false;
  newGroupName = '';
  newGroupDesc = '';
  selectedMemberIds: string[] = [];

  // Call state
  callState: 'idle' | 'calling' |
    'receiving' | 'active' = 'idle';
  callType: 'audio' | 'video' = 'audio';
  incomingCallData: any = null;
  callDuration = 0;
  callTimer: any;

  // WebRTC
  private pc: RTCPeerConnection | null = null;
  localStream: MediaStream | null = null;
  remoteStream: MediaStream | null = null;
  @ViewChild('localVideo')
    localVideoRef!: ElementRef;
  @ViewChild('remoteVideo')
    remoteVideoRef!: ElementRef;

  private subs: Subscription[] = [];

  ngOnInit() {
    const token = this.authService.getToken();
    if (token) {
      try {
        const p = JSON.parse(
          atob(token.split('.')[1]));
        this.myId =
          p.sub ||
          p['http://schemas.xmlsoap.org/ws/' +
            '2005/05/identity/claims/' +
            'nameidentifier'] || '';
        this.myName =
          p.fullName ||
          p.email?.split('@')[0] || '';
      } catch {}
    }

    this.chatService.connect();
    this.loadUsers();
    this.loadGroups();

    // New messages
    this.subs.push(
      this.chatService.messages$.subscribe(
        msgs => {
          if (this.selectedUser) {
            this.messages = msgs.filter(m =>
              !m.groupId &&
              ((m.senderId === this.myId &&
                m.receiverId ===
                  this.selectedUser.id) ||
               (m.senderId ===
                  this.selectedUser.id &&
                m.receiverId === this.myId))
            );
          } else if (this.selectedGroup) {
            this.messages = msgs.filter(m =>
              m.groupId === this.selectedGroup.id
            );
          }
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();
          this.loadUsers();
        }
      )
    );

    // Typing
    this.subs.push(
      this.chatService.typing$.subscribe(d => {
        if (!d) return;
        if (d.userId === this.selectedUser?.id)
          this.isTyping = d.isTyping;
        this.cdr.detectChanges();
      })
    );

    // Online status
    this.subs.push(
      this.chatService.userStatus$.subscribe(d => {
        if (!d) return;
        const u = this.users.find(
          u => u.id === d.userId);
        if (u) {
          u.isOnline = d.isOnline;
          if (!d.isOnline) u.lastSeen = d.lastSeen;
          this.applyFilter();
          this.cdr.detectChanges();
        }
      })
    );

    // Incoming call
    this.subs.push(
      this.chatService.incomingCall$.subscribe(
        d => {
          if (!d) return;
          this.incomingCallData = d;
          this.callState = 'receiving';
          this.callType = d.callType || 'audio';
          this.cdr.detectChanges();
        }
      )
    );

    // Call accepted
    this.subs.push(
      this.chatService.callAccepted$.subscribe(
        async d => {
          if (!d) return;
          if (this.pc && d.answer) {
            const ans =
              JSON.parse(d.answer);
            await this.pc.setRemoteDescription(
              new RTCSessionDescription(ans));
          }
          this.callState = 'active';
          this.startCallTimer();
          this.cdr.detectChanges();
        }
      )
    );

    // Call rejected/ended
    this.subs.push(
      this.chatService.callRejected$.subscribe(
        d => {
          if (!d) return;
          this.endCallLocal(
            'Call rejected');
        }
      )
    );

    this.subs.push(
      this.chatService.callEnded$.subscribe(
        d => {
          if (!d) return;
          this.endCallLocal(
            'Call ended');
        }
      )
    );

    // ICE candidates
    this.subs.push(
      this.chatService.iceCandidate$.subscribe(
        async d => {
          if (!d || !this.pc) return;
          try {
            await this.pc.addIceCandidate(
              new RTCIceCandidate(
                JSON.parse(d.candidate)));
          } catch {}
        }
      )
    );
  }

  shouldShowDate(
    prevDate: string,
    currDate: string): boolean {
    if (!prevDate) return true;
    const prev = new Date(prevDate);
    const curr = new Date(currDate);
    return prev.toDateString() !==
      curr.toDateString();
  }
  
  openImage(url: string) {
    window.open(url, '_blank');
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    this.endCallLocal();
    clearInterval(this.callTimer);
  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  // ── DATA LOAD ────────────────────────
  loadUsers() {
    this.chatService.getChatUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loadingUsers = false;
        if (this.selectedUser) {
          const u = data.find(
            u => u.id === this.selectedUser.id);
          if (u) {
            this.selectedUser.isOnline =
              u.isOnline;
            this.selectedUser.lastSeen =
              u.lastSeen;
            this.selectedUser.unreadCount =
              u.unreadCount;
          }
        }
        this.applyFilter();
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingUsers = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadGroups() {
    this.chatService.getGroups().subscribe({
      next: (data) => {
        this.groups = data;
        if (this.activeFilter === 'groups')
          this.applyFilter();
        this.cdr.detectChanges();
      }
    });
  }

  // ── FILTER ───────────────────────────
  setFilter(f: FilterType) {
    this.activeFilter = f;
    this.applyFilter();
  }

  applyFilter() {
    let items: any[];

    if (this.activeFilter === 'groups') {
      items = this.groups.map(g => ({
        ...g,
        isGroupItem: true
      }));
    } else {
      items = [...this.users];

      if (this.activeFilter === 'unread')
        items = items.filter(
          u => u.unreadCount > 0);
      else if (this.activeFilter === 'online')
        items = items.filter(u => u.isOnline);
    }

    // Search
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      items = items.filter(i =>
        i.fullName?.toLowerCase().includes(q) ||
        i.name?.toLowerCase().includes(q) ||
        i.email?.toLowerCase().includes(q)
      );
    }

    this.filteredItems = items;
    this.cdr.detectChanges();
  }

  // ── SELECT ───────────────────────────
  selectItem(item: any) {
    if (item.isGroupItem) {
      this.selectGroup(item);
    } else {
      this.selectUser(item);
    }
  }

  selectUser(user: any) {
    this.selectedUser = user;
    this.selectedGroup = null;
    this.messages = [];
    this.loadingMessages = true;
    this.chatService.clearMessages();
    this.cdr.detectChanges();

    this.chatService
      .getMessages(user.id)
      .subscribe({
        next: (data) => {
          this.messages = data;
          this.chatService.messages$.next(data);
          this.loadingMessages = false;
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();

          if (user.unreadCount > 0) {
            this.chatService.markRead(user.id);
            user.unreadCount = 0;
          }
        }
      });
  }

  selectGroup(group: any) {
    this.selectedGroup = group;
    this.selectedUser = null;
    this.messages = [];
    this.loadingMessages = true;
    this.chatService.clearMessages();
    this.cdr.detectChanges();

    this.chatService
      .getGroupMessages(group.id)
      .subscribe({
        next: (data) => {
          this.messages = data;
          this.chatService.messages$.next(data);
          this.loadingMessages = false;
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();
        }
      });
  }

  // ── SEND MESSAGE ─────────────────────
  sendMessage() {
    if (!this.newMessage.trim() &&
        !this.uploadingFile) return;
    if (!this.selectedUser &&
        !this.selectedGroup) return;

    const content = this.newMessage.trim();
    this.newMessage = '';

    if (this.selectedUser) {
      this.chatService.sendTyping(
        this.selectedUser.id, false);
      this.chatService.sendMessage(
        this.selectedUser.id,
        content
      ).catch(() => {});
    } else if (this.selectedGroup) {
      this.chatService.sendGroupMessage(
        this.selectedGroup.id,
        content
      ).catch(() => {});
    }
  }

  // ── FILE UPLOAD ──────────────────────
  onFileSelect(event: any) {
    const files =
      Array.from(event.target.files) as File[];
    if (!files.length) return;

    for (const file of files) {
      this.sendFile(file);
    }

    // Reset input
    event.target.value = '';
  }

  sendFile(file: File) {
    this.uploadingFile = true;
    this.cdr.detectChanges();

    this.chatService.uploadFile(file)
      .subscribe({
        next: (res) => {
          this.uploadingFile = false;

          const receiverId =
            this.selectedUser?.id;
          const groupId =
            this.selectedGroup?.id;

          if (receiverId) {
            this.chatService.sendMessage(
              receiverId,
              this.newMessage || '',
              res.messageType,
              res.url,
              res.name,
              res.type
            ).catch(() => {});
          } else if (groupId) {
            this.chatService.sendGroupMessage(
              groupId,
              this.newMessage || '',
              res.messageType,
              res.url,
              res.name,
              res.type
            ).catch(() => {});
          }

          this.newMessage = '';
          this.cdr.detectChanges();
        },
        error: () => {
          this.uploadingFile = false;
          this.cdr.detectChanges();
        }
      });
  }

  onKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' &&
        !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    } else {
      this.onTyping();
    }
  }

  onTyping() {
    if (!this.selectedUser) return;
    this.chatService.sendTyping(
      this.selectedUser.id, true);
    clearTimeout(this.typingTimeout);
    this.typingTimeout = setTimeout(() =>
      this.chatService.sendTyping(
        this.selectedUser.id, false), 2000);
  }

  // ── CREATE GROUP ─────────────────────
  toggleMember(userId: string) {
    const idx =
      this.selectedMemberIds.indexOf(userId);
    if (idx > -1)
      this.selectedMemberIds.splice(idx, 1);
    else
      this.selectedMemberIds.push(userId);
  }

  isMemberSelected(userId: string): boolean {
    return this.selectedMemberIds
      .includes(userId);
  }

  createGroup() {
    if (!this.newGroupName.trim()) return;

    this.chatService.createGroup({
      name: this.newGroupName.trim(),
      description: this.newGroupDesc,
      memberIds: this.selectedMemberIds
    }).subscribe({
      next: (g) => {
        this.showCreateGroup = false;
        this.newGroupName = '';
        this.newGroupDesc = '';
        this.selectedMemberIds = [];
        this.loadGroups();
        this.cdr.detectChanges();
      }
    });
  }

  // ── CALLS (WebRTC) ───────────────────
  async startCall(type: 'audio' | 'video') {
    if (!this.selectedUser) return;
    this.callType = type;
    this.callState = 'calling';
    this.cdr.detectChanges();

    try {
      this.localStream =
        await navigator.mediaDevices
          .getUserMedia({
            audio: true,
            video: type === 'video'
          });

      if (this.localVideoRef?.nativeElement
          && type === 'video') {
        this.localVideoRef.nativeElement.srcObject
          = this.localStream;
      }

      this.pc = this.createPeerConnection();

      this.localStream.getTracks().forEach(t =>
        this.pc!.addTrack(t,
          this.localStream!));

      const offer = await this.pc.createOffer();
      await this.pc.setLocalDescription(offer);

      await this.chatService.initiateCall(
        this.selectedUser.id,
        type,
        JSON.stringify(offer));
    } catch (e) {
      this.endCallLocal('Failed to start call');
    }
  }

  async answerCall() {
    if (!this.incomingCallData) return;
    this.callState = 'active';
    this.cdr.detectChanges();

    try {
      this.localStream =
        await navigator.mediaDevices
          .getUserMedia({
            audio: true,
            video:
              this.callType === 'video'
          });

      if (this.localVideoRef?.nativeElement
          && this.callType === 'video') {
        this.localVideoRef.nativeElement
          .srcObject = this.localStream;
      }

      this.pc = this.createPeerConnection();

      this.localStream.getTracks().forEach(t =>
        this.pc!.addTrack(t,
          this.localStream!));

      const offer = JSON.parse(
        this.incomingCallData.offer);
      await this.pc.setRemoteDescription(
        new RTCSessionDescription(offer));

      const answer = await this.pc.createAnswer();
      await this.pc.setLocalDescription(answer);

      await this.chatService.acceptCall(
        this.incomingCallData.callerId,
        JSON.stringify(answer));

      this.startCallTimer();
    } catch (e) {
      this.endCallLocal('Failed to answer');
    }
  }

  rejectCall() {
    if (!this.incomingCallData) return;
    this.chatService.rejectCall(
      this.incomingCallData.callerId);
    this.endCallLocal();
  }

  hangUp() {
    const otherId =
      this.selectedUser?.id ||
      this.incomingCallData?.callerId;
    if (otherId)
      this.chatService.endCall(otherId);
    this.endCallLocal();
  }

  endCallLocal(msg?: string) {
    clearInterval(this.callTimer);
    this.callDuration = 0;
    this.callState = 'idle';
    this.incomingCallData = null;

    this.localStream?.getTracks()
      .forEach(t => t.stop());
    this.localStream = null;
    this.remoteStream = null;

    if (this.pc) {
      this.pc.close();
      this.pc = null;
    }

    this.cdr.detectChanges();
  }

  private createPeerConnection():
    RTCPeerConnection {
    const pc = new RTCPeerConnection({
      iceServers: [
        { urls: 'stun:stun.l.google.com:19302' },
        { urls: 'stun:stun1.l.google.com:19302' }
      ]
    });

    pc.onicecandidate = (e) => {
      if (e.candidate) {
        const otherId =
          this.selectedUser?.id ||
          this.incomingCallData?.callerId;
        if (otherId)
          this.chatService.sendIceCandidate(
            otherId,
            JSON.stringify(e.candidate));
      }
    };

    pc.ontrack = (e) => {
      this.remoteStream = e.streams[0];
      if (this.remoteVideoRef?.nativeElement) {
        this.remoteVideoRef.nativeElement
          .srcObject = this.remoteStream;
      }
      this.cdr.detectChanges();
    };

    return pc;
  }

  startCallTimer() {
    this.callDuration = 0;
    this.callTimer = setInterval(() => {
      this.callDuration++;
      this.cdr.detectChanges();
    }, 1000);
  }

  getCallDurationStr(): string {
    const m = Math.floor(
      this.callDuration / 60);
    const s = this.callDuration % 60;
    return `${m.toString().padStart(2, '0')}:` +
      `${s.toString().padStart(2, '0')}`;
  }

  // ── SCROLL ───────────────────────────
  scrollToBottom() {
    try {
      const el =
        this.msgContainer?.nativeElement;
      if (el)
        el.scrollTop = el.scrollHeight;
    } catch {}
  }

  // ── HELPERS ──────────────────────────
  getAvatarColor(name: string): string {
    const c = [
      '#ef4444','#f97316','#eab308',
      '#22c55e','#3b82f6',
      '#8b5cf6','#ec4899'
    ];
    return c[
      (name?.charCodeAt(0) || 0) % c.length];
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ')
      .map(n => n[0] || '')
      .join('').toUpperCase().slice(0, 2);
  }

  getLastSeenText(user: any): string {
    if (user?.isOnline) return 'Online';
    if (!user?.lastSeen) return 'Offline';
    const diff =
      Date.now() -
      new Date(user.lastSeen).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    const days = Math.floor(hrs / 24);
    if (days < 7) return `${days}d ago`;
    return new Date(user.lastSeen)
      .toLocaleDateString();
  }

  getTimeStr(dateStr: string): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    const diff =
      Date.now() - d.getTime();
    if (diff < 86400000)
      return d.toLocaleTimeString('en-US',
        { hour: '2-digit', minute: '2-digit' });
    return d.toLocaleDateString('en-US',
      { month: 'short', day: 'numeric' });
  }

  getRoleLabel(role: string): string {
    const m: any = {
      'CompanyAdmin': 'Admin',
      'Agent': 'Agent',
      'Customer': 'Customer',
      'SuperAdmin': 'Super Admin'
    };
    return m[role] || role || '';
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576)
      return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  isImageType(type: string): boolean {
    return type?.startsWith('image/') ||
      ['jpg','jpeg','png','gif','webp']
        .some(e => type?.includes(e));
  }
}