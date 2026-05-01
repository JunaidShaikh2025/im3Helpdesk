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
import { CallLogComponent }
  from '../../call-logs/call-log.component';

type FilterType =
  'all' | 'unread' | 'online' | 'groups';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LayoutComponent,
    CallLogComponent
  ],
  templateUrl: './chat-page.html',
  styleUrls: ['./chat-page.scss']
})
export class ChatPageComponent
  implements OnInit, OnDestroy,
  AfterViewChecked {

  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  public  router      = inject(Router);
  private cdr         = inject(ChangeDetectorRef);

  @ViewChild('messagesContainer')
    msgContainer!: ElementRef;
  @ViewChild('localVideo')
    localVideoRef!: ElementRef;
  @ViewChild('remoteVideo')
    remoteVideoRef!: ElementRef;

  // ── State ──────────────────────────────
  users:         any[]     = [];
  groups:        any[]     = [];
  filteredItems: any[]     = [];
  selectedUser:  any       = null;
  selectedGroup: any       = null;
  messages:      any[]     = [];
  newMessage              = '';
  searchQuery             = '';
  activeFilter: FilterType = 'all';
  loadingUsers            = true;
  loadingMessages         = false;
  isTyping                = false;
  typingTimeout: any;
  shouldScrollToBottom    = false;
  uploadingFile           = false;

  myId   = '';
  myName = '';

  // ── Sidebar tab: 'chat' | 'calls' ──────
  sidebarTab: 'chat' | 'calls' = 'chat';

  // ── Create Group Modal ─────────────────
  showCreateGroup     = false;
  newGroupName        = '';
  newGroupDesc        = '';
  selectedMemberIds: string[] = [];
  memberSearchQuery   = '';

  get filteredModalUsers(): any[] {
    const q =
      this.memberSearchQuery.toLowerCase();
    if (!q) return this.users;
    return this.users.filter(u =>
      u.fullName?.toLowerCase().includes(q) ||
      u.email?.toLowerCase().includes(q));
  }

  // ── Add Members to Existing Group ──────
  showAddMembers    = false;
  addMemberGroupId  = '';
  addMemberGroupName = '';
  addMemberSearch   = '';
  addMemberSelected: string[] = [];
  addMemberLoading  = false;

  get filteredAddMemberUsers(): any[] {
    const q =
      this.addMemberSearch.toLowerCase();
    const existingIds =
      this.selectedGroup?.members
        ?.map((m: any) => m.userId) || [];
    let list = this.users.filter(u =>
      !existingIds.includes(u.id));
    if (q)
      list = list.filter(u =>
        u.fullName?.toLowerCase().includes(q) ||
        u.email?.toLowerCase().includes(q));
    return list;
  }

  // ── Call State ─────────────────────────
  callState:
    'idle' | 'calling' |
    'receiving' | 'active' = 'idle';
  callType: 'audio' | 'video' = 'audio';
  incomingCallData: any  = null;
  callDuration           = 0;
  callTimer: any;
  isMuted                = false;
  isCameraOff            = false;

  // WebRTC
  private pc: RTCPeerConnection | null = null;
  private isSettingRemoteAnswer = false;
  localStream: MediaStream | null = null;
  private iceCandidateQueue:
    RTCIceCandidateInit[] = [];

  private subs: Subscription[] = [];

  // ── ngOnInit ────────────────────────────
  ngOnInit() {
    this.resetCallState();

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
    this.subscribeToEvents();
  }

  // ── Reset call state ────────────────────
  private resetCallState() {
    this.callState            = 'idle';
    this.incomingCallData     = null;
    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue    = [];

    if (this.pc) {
      try { this.pc.close(); } catch {}
      this.pc = null;
    }

    this.chatService.incomingCall$.next(null);
    this.chatService.callAccepted$.next(null);
    this.chatService.callRejected$.next(null);
    this.chatService.callEnded$.next(null);
    this.chatService.iceCandidate$.next(null);
  }

  private subscribeToEvents() {
    // ── New real-time message ──
    this.subs.push(
      this.chatService.newMessage$.subscribe(
        msg => {
          if (!msg) return;

          const forCurrentUser =
            this.selectedUser &&
            !msg.groupId &&
            ((msg.senderId === this.myId &&
              msg.receiverId ===
                this.selectedUser.id) ||
             (msg.senderId ===
                this.selectedUser.id &&
              msg.receiverId === this.myId));

          const forCurrentGroup =
            this.selectedGroup &&
            msg.groupId ===
              this.selectedGroup.id;

          if (forCurrentUser ||
              forCurrentGroup) {
            const exists = this.messages
              .some(m => m.id === msg.id);
            if (!exists) {
              this.messages =
                [...this.messages, msg];
              this.shouldScrollToBottom = true;
            }
          }

          this.cdr.detectChanges();
          this.loadUsers(true);
        })
    );

    // ── Typing ──
    this.subs.push(
      this.chatService.typing$.subscribe(d => {
        if (!d) return;
        if (d.userId ===
            this.selectedUser?.id) {
          this.isTyping = d.isTyping;
          this.cdr.detectChanges();
        }
      })
    );

    // ── Online status ──
    this.subs.push(
      this.chatService.userStatus$.subscribe(
        d => {
          if (!d) return;
          const u = this.users.find(
            u => u.id === d.userId);
          if (u) {
            u.isOnline = d.isOnline;
            if (!d.isOnline)
              u.lastSeen = d.lastSeen;
            this.applyFilter();
          }
        })
    );

    // ── Incoming call — only if idle ──
    this.subs.push(
      this.chatService.incomingCall$.subscribe(
        d => {
          if (!d) return;
          if (this.callState !== 'idle') return;
          this.incomingCallData = d;
          this.callState  = 'receiving';
          this.callType   =
            d.callType || 'audio';
          this.cdr.detectChanges();
        })
    );

    // ── Call accepted ──
    this.subs.push(
      this.chatService.callAccepted$.subscribe(
        async d => {
          if (!d) return;
          if (!this.pc) return;
          if (this.isSettingRemoteAnswer) return;
          if (this.pc.signalingState !==
              'have-local-offer') return;

          try {
            this.isSettingRemoteAnswer = true;
            const ans = JSON.parse(d.answer);
            await this.pc.setRemoteDescription(
              new RTCSessionDescription(ans));
            await this.flushIceCandidates();
            this.callState = 'active';
            this.startCallTimer();
          } catch (e) {
            console.error(
              'setRemoteDescription error:', e);
            this.endCallLocal();
          } finally {
            this.isSettingRemoteAnswer = false;
          }
          this.cdr.detectChanges();
        })
    );

    // ── Call rejected ──
    this.subs.push(
      this.chatService.callRejected$.subscribe(
        d => {
          if (!d) return;
          this.endCallLocal();
        })
    );

    // ── Call ended ──
    this.subs.push(
      this.chatService.callEnded$.subscribe(
        d => {
          if (!d) return;
          this.endCallLocal();
        })
    );

    // ── ICE candidates ──
    this.subs.push(
      this.chatService.iceCandidate$.subscribe(
        async d => {
          if (!d || !this.pc) return;
          try {
            const c = JSON.parse(
              d.candidate) as RTCIceCandidateInit;
            if (!this.pc.remoteDescription) {
              this.iceCandidateQueue.push(c);
            } else {
              await this.pc.addIceCandidate(
                new RTCIceCandidate(c));
            }
          } catch {}
        })
    );

    // ✅ Call-back request from call log ──
    this.subs.push(
      this.chatService.startCallRequest$
        .subscribe(req => {
          if (!req) return;
          const user = this.users.find(
            u => u.id === req.userId);
          if (user) {
            this.selectUser(user);
            this.sidebarTab = 'chat';
            setTimeout(() =>
              this.startCall(req.type), 400);
          }
          // Reset after handling
          this.chatService
            .startCallRequest$.next(null);
        })
    );
  }

  private async flushIceCandidates() {
    if (!this.pc) return;
    while (this.iceCandidateQueue.length) {
      const c =
        this.iceCandidateQueue.shift()!;
      try {
        await this.pc.addIceCandidate(
          new RTCIceCandidate(c));
      } catch {}
    }
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    clearTimeout(this.typingTimeout);
    this.endCallLocal();
  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  // ── Sidebar tab ─────────────────────────
  setSidebarTab(t: 'chat' | 'calls') {
    this.sidebarTab = t;
  }

  // ── Load data ───────────────────────────
  loadUsers(silent = false) {
    if (!silent) this.loadingUsers = true;

    this.chatService.getChatUsers()
      .subscribe({
        next: (data) => {
          this.users        = data;
          this.loadingUsers = false;

          if (this.selectedUser) {
            const u = data.find(
              u => u.id ===
                this.selectedUser.id);
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

  // ── Filter ──────────────────────────────
  setFilter(f: FilterType) {
    this.activeFilter = f;
    this.applyFilter();
  }

  applyFilter() {
    let items: any[];

    if (this.activeFilter === 'groups') {
      items = this.groups.map(g => ({
        ...g, isGroupItem: true
      }));
    } else {
      items = [...this.users];
      if (this.activeFilter === 'unread')
        items = items.filter(
          u => (u.unreadCount || 0) > 0);
      else if (this.activeFilter === 'online')
        items = items.filter(u => u.isOnline);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      items = items.filter(i =>
        i.fullName?.toLowerCase().includes(q) ||
        i.name?.toLowerCase().includes(q) ||
        i.email?.toLowerCase().includes(q));
    }

    items.sort((a, b) => {
      const ua = a.unreadCount || 0;
      const ub = b.unreadCount || 0;
      if (ua !== ub) return ub - ua;
      const ta = a.lastMessage?.createdAt
        ? new Date(
            a.lastMessage.createdAt).getTime()
        : 0;
      const tb = b.lastMessage?.createdAt
        ? new Date(
            b.lastMessage.createdAt).getTime()
        : 0;
      return tb - ta;
    });

    this.filteredItems = items;
    this.cdr.detectChanges();
  }

  // ── Select ──────────────────────────────
  selectItem(item: any) {
    if (item.isGroupItem)
      this.selectGroup(item);
    else
      this.selectUser(item);
  }

  selectUser(user: any) {
    this.selectedUser  = user;
    this.selectedGroup = null;
    this.messages      = [];
    this.loadingMessages = true;
    this.cdr.detectChanges();

    this.chatService
      .getMessages(user.id)
      .subscribe({
        next: (data) => {
          this.messages        = data;
          this.loadingMessages = false;
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();

          if ((user.unreadCount || 0) > 0) {
            this.chatService.markRead(user.id);
            user.unreadCount = 0;
          }
        }
      });
  }

  selectGroup(group: any) {
    this.selectedGroup = group;
    this.selectedUser  = null;
    this.messages      = [];
    this.loadingMessages = true;
    this.cdr.detectChanges();

    this.chatService
      .getGroupMessages(group.id)
      .subscribe({
        next: (data) => {
          this.messages        = data;
          this.loadingMessages = false;
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();
        }
      });
  }

  // ── Send message ────────────────────────
  sendMessage() {
    const content = this.newMessage.trim();
    if (!content && !this.uploadingFile)
      return;
    if (!this.selectedUser &&
        !this.selectedGroup) return;

    this.newMessage = '';

    if (this.selectedUser) {
      this.stopTyping();
      this.chatService.sendMessage(
        this.selectedUser.id, content);
    } else if (this.selectedGroup) {
      this.chatService.sendGroupMessage(
        this.selectedGroup.id, content);
    }
  }

  // ── File upload ─────────────────────────
  onFileSelect(event: any) {
    const files =
      Array.from(event.target.files) as File[];
    if (!files.length) return;
    files.forEach(f => this.sendFile(f));
    event.target.value = '';
  }

  sendFile(file: File) {
    this.uploadingFile = true;
    this.cdr.detectChanges();

    this.chatService.uploadFile(file)
      .subscribe({
        next: (res) => {
          this.uploadingFile = false;
          const rid = this.selectedUser?.id;
          const gid = this.selectedGroup?.id;

          if (rid) {
            this.chatService.sendMessage(
              rid,
              this.newMessage || '',
              res.messageType,
              res.url, res.name, res.type);
          } else if (gid) {
            this.chatService.sendGroupMessage(
              gid,
              this.newMessage || '',
              res.messageType,
              res.url, res.name, res.type);
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

  // ── Typing indicator ────────────────────
  onTyping() {
    if (!this.selectedUser) return;
    if (!this.chatService.isConnected) return;
    this.chatService.sendTyping(
      this.selectedUser.id, true);
    clearTimeout(this.typingTimeout);
    this.typingTimeout = setTimeout(() =>
      this.stopTyping(), 2000);
  }

  private stopTyping() {
    clearTimeout(this.typingTimeout);
    if (!this.selectedUser) return;
    if (!this.chatService.isConnected) return;
    this.chatService.sendTyping(
      this.selectedUser.id, false);
  }

  // ── Create group ────────────────────────
  openCreateGroup() {
    this.showCreateGroup   = true;
    this.newGroupName      = '';
    this.newGroupDesc      = '';
    this.selectedMemberIds = [];
    this.memberSearchQuery = '';
  }

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
      name:        this.newGroupName.trim(),
      description: this.newGroupDesc,
      memberIds:   this.selectedMemberIds
    }).subscribe({
      next: () => {
        this.showCreateGroup = false;
        this.loadGroups();
        this.cdr.detectChanges();
      }
    });
  }

  // ── Add members to group ────────────────
  openAddMembers(group: any) {
    this.addMemberGroupId   = group.id;
    this.addMemberGroupName = group.name;
    this.addMemberSelected  = [];
    this.addMemberSearch    = '';
    this.showAddMembers     = true;
    this.cdr.detectChanges();
  }

  toggleAddMember(userId: string) {
    const idx =
      this.addMemberSelected.indexOf(userId);
    if (idx > -1)
      this.addMemberSelected.splice(idx, 1);
    else
      this.addMemberSelected.push(userId);
  }

  isAddMemberSelected(
    userId: string): boolean {
    return this.addMemberSelected
      .includes(userId);
  }

  confirmAddMembers() {
    if (!this.addMemberSelected.length) return;
    this.addMemberLoading = true;

    this.chatService.addGroupMembers(
      this.addMemberGroupId,
      this.addMemberSelected
    ).subscribe({
      next: () => {
        this.addMemberLoading = false;
        this.showAddMembers   = false;
        this.loadGroups();
        if (this.selectedGroup?.id ===
            this.addMemberGroupId)
          this.selectGroup(this.selectedGroup);
        this.cdr.detectChanges();
      },
      error: () => {
        this.addMemberLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // ── Calls (WebRTC) ──────────────────────
  async startCall(type: 'audio' | 'video') {
    if (!this.selectedUser) return;
    if (this.callState !== 'idle') return;
    if (!this.chatService.isConnected) return;

    this.callType  = type;
    this.callState = 'calling';
    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue     = [];
    this.cdr.detectChanges();

    try {
      this.localStream =
        await navigator.mediaDevices
          .getUserMedia({
            audio: true,
            video: type === 'video'
          });

      if (this.localVideoRef?.nativeElement &&
          type === 'video') {
        setTimeout(() => {
          this.localVideoRef.nativeElement
            .srcObject = this.localStream;
        }, 100);
      }

      this.pc = this.createPeerConnection();
      this.localStream.getTracks().forEach(t =>
        this.pc!.addTrack(
          t, this.localStream!));

      const offer =
        await this.pc.createOffer();
      await this.pc.setLocalDescription(offer);

      await this.chatService.initiateCall(
        this.selectedUser.id,
        type,
        JSON.stringify(offer));
    } catch (e) {
      console.error('startCall error:', e);
      this.endCallLocal();
    }
  }

  async answerCall() {
    if (!this.incomingCallData) return;
    this.callState             = 'active';
    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue     = [];
    this.cdr.detectChanges();

    try {
      this.localStream =
        await navigator.mediaDevices
          .getUserMedia({
            audio: true,
            video: this.callType === 'video'
          });

      if (this.localVideoRef?.nativeElement &&
          this.callType === 'video') {
        setTimeout(() => {
          this.localVideoRef.nativeElement
            .srcObject = this.localStream;
        }, 100);
      }

      this.pc = this.createPeerConnection();
      this.localStream.getTracks().forEach(t =>
        this.pc!.addTrack(
          t, this.localStream!));

      const offer = JSON.parse(
        this.incomingCallData.offer);
      await this.pc.setRemoteDescription(
        new RTCSessionDescription(offer));

      await this.flushIceCandidates();

      const answer =
        await this.pc.createAnswer();
      await this.pc.setLocalDescription(answer);

      await this.chatService.acceptCall(
        this.incomingCallData.callerId,
        JSON.stringify(answer));

      this.startCallTimer();
    } catch (e) {
      console.error('answerCall error:', e);
      this.endCallLocal();
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

  toggleMute() {
    if (!this.localStream) return;
    this.isMuted = !this.isMuted;
    this.localStream.getAudioTracks()
      .forEach(t =>
        t.enabled = !this.isMuted);
    this.cdr.detectChanges();
  }

  toggleCamera() {
    if (!this.localStream) return;
    this.isCameraOff = !this.isCameraOff;
    this.localStream.getVideoTracks()
      .forEach(t =>
        t.enabled = !this.isCameraOff);
    this.cdr.detectChanges();
  }

  endCallLocal() {
    clearInterval(this.callTimer);
    this.callDuration  = 0;
    this.isMuted       = false;
    this.isCameraOff   = false;
    this.iceCandidateQueue = [];

    this.localStream?.getTracks()
      .forEach(t => t.stop());
    this.localStream = null;

    if (this.pc) {
      try { this.pc.close(); } catch {}
      this.pc = null;
    }

    this.callState             = 'idle';
    this.incomingCallData      = null;
    this.isSettingRemoteAnswer = false;

    this.chatService.incomingCall$.next(null);
    this.chatService.callAccepted$.next(null);
    this.chatService.callRejected$.next(null);
    this.chatService.callEnded$.next(null);
    this.chatService.iceCandidate$.next(null);

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
      if (!e.candidate) return;
      const otherId =
        this.selectedUser?.id ||
        this.incomingCallData?.callerId;
      if (otherId)
        this.chatService.sendIceCandidate(
          otherId,
          JSON.stringify(e.candidate));
    };

    pc.ontrack = (e) => {
      const remoteStream = e.streams[0];
      setTimeout(() => {
        if (this.remoteVideoRef?.nativeElement)
          this.remoteVideoRef.nativeElement
            .srcObject = remoteStream;
      }, 100);
      this.cdr.detectChanges();
    };

    pc.onconnectionstatechange = () => {
      if (pc.connectionState ===
          'disconnected' ||
          pc.connectionState === 'failed' ||
          pc.connectionState === 'closed') {
        this.endCallLocal();
      }
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
    return `${m.toString().padStart(2,'0')}:` +
      `${s.toString().padStart(2,'0')}`;
  }

  // ── Scroll ──────────────────────────────
  scrollToBottom() {
    try {
      const el =
        this.msgContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }

  // ── Helpers ─────────────────────────────
  shouldShowDate(
    prev: string, curr: string): boolean {
    if (!prev) return true;
    return new Date(prev).toDateString() !==
      new Date(curr).toDateString();
  }

  openImage(url: string) {
    window.open(url, '_blank');
  }

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
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  getLastSeenText(user: any): string {
    if (user?.isOnline) return 'Online';
    if (!user?.lastSeen) return 'Offline';
    const diff = Date.now() -
      new Date(user.lastSeen).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1)  return 'Just now';
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
    const d    = new Date(dateStr);
    const diff = Date.now() - d.getTime();
    if (diff < 86400000)
      return d.toLocaleTimeString('en-US',
        { hour:'2-digit', minute:'2-digit' });
    return d.toLocaleDateString('en-US',
      { month:'short', day:'numeric' });
  }

  getRoleLabel(role: string): string {
    const m: any = {
      CompanyAdmin: 'Admin',
      Agent:        'Agent',
      Customer:     'Customer',
      SuperAdmin:   'Super Admin'
    };
    return m[role] || role || '';
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024)
      return `${bytes} B`;
    if (bytes < 1048576)
      return `${(bytes/1024).toFixed(1)} KB`;
    return `${(bytes/1048576).toFixed(1)} MB`;
  }
}