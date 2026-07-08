// FILE: src/app/core/services/global-call-notification.service.ts
// Root service — WebRTC yahan hai, tab/page change pe alive rahega

import { Injectable, inject, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';
import { ChatService } from './chat.service';
import { environment } from '../../../environments/environment';

export interface ConferenceParticipant {
  id: string;
  name: string;
  photoUrl?: string | null;
}

export interface CallDiagnostics {
  quality: 'excellent' | 'good' | 'fair' | 'poor';
  rttMs: number;
  jitterMs: number;
  packetLossPct: number;
  inboundKbps: number;
  outboundKbps: number;
  availableOutgoingKbps: number;
  localCandidateType: string;
  remoteCandidateType: string;
  updatedAt: number;
}

@Injectable({ providedIn: 'root' })
export class GlobalCallNotificationService implements OnDestroy {

  private chatSvc = inject(ChatService);
  private subs: Subscription[] = [];

  // ── Incoming popup ───────────────────────
  incomingCall: any = null;
  isVisible         = false;

  // ── Active call ──────────────────────────
  isCallActive  = false;
  isMinimized   = false;
  activeCall: any = null;
  callType: 'audio' | 'video' = 'audio';
  callDuration  = 0;
  rtcState: RTCPeerConnectionState = 'new';

  // ── WebRTC (yahan store — never destroyed) ──
  pc: RTCPeerConnection | null     = null;
  localStream: MediaStream | null  = null;
  remoteStream: MediaStream | null = null;
  iceCandidateQueue: RTCIceCandidateInit[] = [];
  isSettingRemoteAnswer = false;
  incomingCallData: any = null;
  activeCallOtherId     = '';

  // ── Conference state (mesh) ──────────────
  isConference = false;
  /** Active CallLog id, used as the conference roomId. */
  currentCallLogId: string | null = null;
  /** userId → RTCPeerConnection (one per remote peer). */
  peers = new Map<string, RTCPeerConnection>();
  /** userId → inbound MediaStream from that peer. */
  remoteStreams = new Map<string, MediaStream>();
  /** userId → ICE candidates queued before remoteDescription set. */
  private peerIceQueue = new Map<string, RTCIceCandidateInit[]>();
  /** Roster of remote participants currently in the call. */
  participants = new Map<string, ConferenceParticipant>();
  /** Pending conference invite popup data (when not yet in call). */
  conferenceInvite: any = null;

  // ── In-call ephemeral chat ───────────────
  callMessages: Array<{
    fromId: string; fromName: string;
    text: string; at: Date; mine: boolean;
  }> = [];
  callChatChanged$ = new BehaviorSubject<number>(0);
  /** Local user id, populated when call starts (used for `mine` flag). */
  myUserId: string | null = null;
  /** Local user display name, captured from JWT for use in the roster. */
  myUserName: string = '';
  private _seenMsgKeys = new Set<string>();

  // ── Speaking activity (Teams-style avatar blink) ──
  /** userId → true when their mic is currently producing audio. */
  speakingIds = new Set<string>();
  speakingChanged$ = new BehaviorSubject<number>(0);
  private _audioWatchers = new Map<string, () => void>();

  // ── Streams ──────────────────────────────
  expandCallRequest$ = new BehaviorSubject<boolean>(false);
  remoteStream$      = new BehaviorSubject<MediaStream | null>(null);
  /** Emits whenever conference roster / streams change so UI re-renders. */
  conferenceChanged$ = new BehaviorSubject<number>(0);
  /** Live call-network diagnostics for quality verification. */
  diagnostics$ = new BehaviorSubject<CallDiagnostics | null>(null);

  private callTimer: any;
  private disconnectTimer: any;
  private diagnosticsTimer: any;
  private ringCtx: AudioContext | null = null;
  private audioUnlockBound = false;
  private remoteAudioEl: HTMLAudioElement | null = null;
  private externalMediaConsumers = 0;
  private prevInboundBytes = 0;
  private prevOutboundBytes = 0;
  private prevDiagAt = 0;
  private _stopRingFn: (() => void) | null = null;
  private _navigateToChat: (() => void) | null = null;

  // ── Flag: prevent re-entrant endCall ────
  private _isEnding = false;

  private readonly fallbackIceServers: RTCIceServer[] = [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' }
  ];

  private readonly audioConstraints: MediaTrackConstraints = {
    echoCancellation: true,
    noiseSuppression: true,
    autoGainControl: true,
    channelCount: { ideal: 1 },
    sampleRate: { ideal: 48000 }
  };

  private readonly videoConstraints: MediaTrackConstraints = {
    width: { min: 320, ideal: 1280, max: 1920 },
    height: { min: 240, ideal: 720, max: 1080 },
    frameRate: { ideal: 30, max: 30 },
    facingMode: 'user'
  };

  // ─────────────────────────────────────────
  init(navigateToChatFn: () => void) {
    this._navigateToChat = navigateToChatFn;
    this.bindAudioUnlock();

    // Capture our own user id from JWT once for `mine` flag in chat.
    try {
      const token = localStorage.getItem('auth_token');
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.myUserId = String(
          payload.sub || payload.nameid
          || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
          || ''
        ) || null;
        this.myUserName = String(
          payload.name
          || payload.fullName
          || payload.unique_name
          || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
          || ''
        );
      }
    } catch {}

    // Incoming call → show popup
    this.subs.push(
      this.chatSvc.incomingCall$.subscribe(d => {
        if (!d) {
          // Stream null hua — popup hide
          this.isVisible    = false;
          this.incomingCall = null;
          this.stopRingtone();
          return;
        }
        // ✅ Agar call already active/minimized hai — popup mat dikhao
        if (this.isCallActive) return;
        this.incomingCall     = d;
        this.incomingCallData = d;
        this.isVisible        = true;
        this.callType         = d.callType || 'audio';
        this.currentCallLogId = d.callLogId || d.CallLogId || null;
        this.playRingtone();
      })
    );

    // Caller side: backend echoes the new CallLog id so we know our
    // conference roomId in case the user later promotes the call.
    this.subs.push(
      this.chatSvc.callInitiated$.subscribe(d => {
        if (!d) return;
        this.currentCallLogId = d.callLogId || d.CallLogId || null;
      })
    );

    // Outgoing call accepted by remote
    this.subs.push(
      this.chatSvc.callAccepted$.subscribe(async d => {
        if (!d || !this.pc) return;
        if (this.isSettingRemoteAnswer) return;
        if (this.pc.signalingState !== 'have-local-offer') return;
        try {
          this.isSettingRemoteAnswer = true;
          const ans = this.parseRtcJson(d.answer || d.Answer);
          if (!ans) return;
          await this.pc.setRemoteDescription(
            new RTCSessionDescription(ans));
          await this.flushIceCandidates();
          this.isCallActive = true;
          this.isVisible    = false;
          this.incomingCall = null;
          this.startCallTimer();
        } catch (e) {
          console.error('callAccepted error:', e);
          this.endCall(false);
        } finally {
          this.isSettingRemoteAnswer = false;
        }
      })
    );

    // Remote ended the call
    this.subs.push(
      this.chatSvc.callEnded$.subscribe(d => {
        if (!d) return;
        this.hidePopup();
        this.endCall(false); // false = hub ko signal mat bhejo (remote ne bheja)
      })
    );

    // Call rejected
    this.subs.push(
      this.chatSvc.callRejected$.subscribe(d => {
        if (!d) return;
        this.hidePopup();
        this.endCall(false);
      })
    );

    // ICE candidates
    this.subs.push(
      this.chatSvc.iceCandidate$.subscribe(async d => {
        if (!d || !this.pc) return;
        try {
          const c = this.parseRtcJson(
            d.candidate || d.Candidate) as RTCIceCandidateInit | null;
          if (!c) return;
          if (!this.pc.remoteDescription) {
            this.iceCandidateQueue.push(c);
          } else {
            await this.pc.addIceCandidate(new RTCIceCandidate(c));
          }
        } catch {}
      })
    );

    // ── Conference signaling ──────────────────────────

    this.subs.push(
      this.chatSvc.conferenceInvite$.subscribe(d => {
        if (!d) return;
        // Don't ring if user is already in this exact call.
        const id = d.callLogId || d.CallLogId;
        if (this.isCallActive && this.currentCallLogId === id) return;
        this.conferenceInvite = d;
        this.callType = (d.callType || d.CallType || 'audio') as 'audio' | 'video';
        this.playRingtone();
      })
    );

    this.subs.push(
      this.chatSvc.conferenceParticipantJoined$.subscribe(d => {
        if (!d) return;
        const eventCallLogId = String(d.callLogId || d.CallLogId || '');
        // Auto-promote: if this is OUR active 1-to-1 call and someone
        // else is joining, we need to switch into conference mode so
        // the new peer's offer gets handled and shown in the grid.
        if (!this.isConference && this.isCallActive &&
            eventCallLogId && eventCallLogId === this.currentCallLogId) {
          this.isConference = true;
          if (this.pc && this.activeCallOtherId) {
            this.peers.set(this.activeCallOtherId, this.pc);
            if (this.remoteStream) {
              this.remoteStreams.set(
                this.activeCallOtherId, this.remoteStream);
            }
            if (!this.participants.has(this.activeCallOtherId)) {
              this.participants.set(this.activeCallOtherId, {
                id: this.activeCallOtherId,
                name: this.activeCall?.name
                  || this.incomingCallData?.callerName
                  || 'Participant'
              });
            }
            this.rebindPeerIce(this.activeCallOtherId, this.pc);
          }
        }
        if (!this.isConference) return;
        const id = String(d.userId || d.UserId);
        if (this.myUserId && id === this.myUserId) return;
        const name = String(d.fullName || d.FullName || 'Participant');
        const photo = d.photoUrl || d.PhotoUrl || null;
        this.participants.set(id, { id, name, photoUrl: photo });
        this.bumpConferenceUI();
      })
    );

    this.subs.push(
      this.chatSvc.conferenceParticipantLeft$.subscribe(d => {
        if (!d) return;
        const id = String(d.userId || d.UserId);
        this.removePeer(id);
        this.participants.delete(id);
        this.bumpConferenceUI();
        // If we're left alone in the room, end the call.
        if (this.isConference && this.participants.size === 0) {
          this.endCall(false);
        }
      })
    );

    // Someone in the room offered us a peer connection (they joined late
    // and are now dialing every existing member).
    this.subs.push(
      this.chatSvc.conferenceOffer$.subscribe(async d => {
        if (!d || !this.isConference) return;
        await this.handleConferenceOffer(d);
      })
    );

    this.subs.push(
      this.chatSvc.conferenceAnswer$.subscribe(async d => {
        if (!d) return;
        const fromId = String(d.fromUserId || d.FromUserId);
        const pc = this.peers.get(fromId);
        if (!pc) return;
        try {
          const ans = this.parseRtcJson(d.answer || d.Answer);
          if (!ans) return;
          if (pc.signalingState === 'have-local-offer') {
            await pc.setRemoteDescription(
              new RTCSessionDescription(ans));
            await this.flushPeerIce(fromId);
          }
        } catch (e) {
          console.error('conferenceAnswer error', e);
        }
      })
    );

    this.subs.push(
      this.chatSvc.conferenceIce$.subscribe(async d => {
        if (!d) return;
        const fromId = String(d.fromUserId || d.FromUserId);
        const pc = this.peers.get(fromId);
        const cand = this.parseRtcJson(
          d.candidate || d.Candidate) as RTCIceCandidateInit | null;
        if (!cand) return;
        if (!pc || !pc.remoteDescription) {
          const q = this.peerIceQueue.get(fromId) || [];
          q.push(cand);
          this.peerIceQueue.set(fromId, q);
          return;
        }
        try { await pc.addIceCandidate(new RTCIceCandidate(cand)); }
        catch {}
      })
    );

    this.subs.push(
      this.chatSvc.callMessage$.subscribe(d => {
        if (!d) return;
        const callId = String(d.callLogId || d.CallLogId || '');
        if (!this.currentCallLogId || callId !== this.currentCallLogId) return;
        const fromId = String(d.fromUserId || d.FromUserId);
        // Skip server echo of our own message (we already added it locally).
        if (this.myUserId && fromId === this.myUserId) return;
        const text = String(d.text || d.Text || '');
        const at = new Date(d.at || d.At || Date.now());
        // Dedupe: same sender + same text within last 3s already shown.
        const key = `${fromId}|${text}|${Math.floor(at.getTime() / 1000)}`;
        if (this._seenMsgKeys.has(key)) return;
        this._seenMsgKeys.add(key);
        if (this._seenMsgKeys.size > 200) {
          // bound the set
          const first = this._seenMsgKeys.values().next().value;
          if (first) this._seenMsgKeys.delete(first);
        }
        this.callMessages.push({
          fromId,
          fromName: String(d.fromName || d.FromName || ''),
          text,
          at,
          mine: false
        });
        this.callChatChanged$.next(this.callMessages.length);
      })
    );
  }

  // ── Popup ────────────────────────────────
  hidePopup() {
    this.isVisible    = false;
    this.incomingCall = null;
    this.stopRingtone();
  }

  // Decline from global popup
  rejectIncomingCall() {
    const callerId = this.incomingCallData?.callerId;
    this.hidePopup();
    this.incomingCallData = null;
    // ✅ incomingCall$ clear karo — dobara popup nahi aayega
    this.chatSvc.incomingCall$.next(null);
    if (callerId) this.chatSvc.rejectCall(callerId);
  }

  // Accept from global popup
  async acceptIncomingCall() {
    if (!this.incomingCallData) return;

    // ✅ TURANT popup hide — async se pehle
    this.isVisible    = false;
    this.incomingCall = null;
    this.stopRingtone();

    // ✅ incomingCall$ clear karo — navigate ke baad dobara popup nahi aayega
    this.chatSvc.incomingCall$.next(null);

    const type = this.incomingCallData.callType || 'audio';
    this.callType = type;

    // Chat page pe navigate — taaki UI ready ho
    if (this._navigateToChat) this._navigateToChat();

    // Thoda wait karo, phir WebRTC answer karo
    setTimeout(() => this.answerCallInternal(), 150);
  }

  // ── WebRTC: Answer ───────────────────────
  async answerCallInternal() {
    if (!this.incomingCallData) return;
    if (this.isCallActive) return;

    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue     = [];
    this.activeCallOtherId     = this.incomingCallData.callerId;
    // Remember the caller's name so the call window header / roster
    // show "Naresh" instead of the generic "Active Call" fallback.
    this.activeCall = {
      name: this.incomingCallData.callerName || '',
      type: this.callType
    };

    try {
      this.localStream = await navigator.mediaDevices
        .getUserMedia({
          audio: true,
          video: this.callType === 'video'
        });
      this.attachAudioWatcher(
        this.myUserId || 'me', this.localStream);

      this.pc = this.createPeerConnection();
      await this.attachLocalTracks(this.pc, this.localStream);

      const offer = this.parseRtcJson(
        this.incomingCallData.offer || this.incomingCallData.Offer);
      if (!offer) throw new Error('Incoming offer missing');
      await this.pc.setRemoteDescription(
        new RTCSessionDescription(offer));
      await this.flushIceCandidates();

      const answer = await this.pc.createAnswer();
      await this.pc.setLocalDescription(answer);

      await this.chatSvc.acceptCall(
        this.incomingCallData.callerId,
        JSON.stringify(answer));

      this.isCallActive = true;
      this.isMinimized  = false;
      this.callDuration = 0;
      this.rtcState = 'connecting';
      this.startCallTimer();

    } catch (e) {
      console.error('answerCallInternal error:', e);
      this.endCall(false);
    }
  }

  // ── WebRTC: Start outgoing ───────────────
  async startCallInternal(
    receiverId: string,
    type: 'audio' | 'video',
    receiverName: string = ''
  ) {
    if (this.isCallActive) return;
    this.callType          = type;
    this.activeCallOtherId = receiverId;
    // Save the receiver's name so the caller's roster / header show
    // the real person ("Junaid Shaikh"), not the generic "Active Call".
    this.activeCall = { name: receiverName, type };
    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue     = [];

    try {
      this.localStream = await navigator.mediaDevices
        .getUserMedia({ audio: true, video: type === 'video' });
      this.attachAudioWatcher(
        this.myUserId || 'me', this.localStream);

      this.pc = this.createPeerConnection();
      await this.attachLocalTracks(this.pc, this.localStream);

      const offer = await this.pc.createOffer();
      await this.pc.setLocalDescription(offer);

      await this.chatSvc.initiateCall(
        receiverId, type, JSON.stringify(offer));

    } catch (e) {
      console.error('startCallInternal error:', e);
      this.endCall(false);
    }
  }

  // ── End call ────────────────────────────
  // sendSignal=true: ham end kar rahe hain — hub ko batao
  // sendSignal=false: remote ne end kiya — already notified
  endCall(sendSignal = true) {
    if (this._isEnding) return; // ✅ re-entrant guard
    this._isEnding = true;

    // Conference path: leave the room (server tells other peers).
    if (this.isConference && this.currentCallLogId && sendSignal) {
      this.chatSvc.leaveConference(this.currentCallLogId);
    }
    if (sendSignal && !this.isConference && this.activeCallOtherId) {
      this.chatSvc.endCall(this.activeCallOtherId);
    }

    // Tear down all conference peers.
    this.peers.forEach((pc) => { try { pc.close(); } catch {} });
    this.peers.clear();
    this.remoteStreams.clear();
    this.peerIceQueue.clear();
    this.participants.clear();
    this.isConference = false;
    this.currentCallLogId = null;
    this.callMessages = [];
    this.callChatChanged$.next(0);
    this.stopDiagnostics();
    this.diagnostics$.next(null);
    this._seenMsgKeys.clear();
    // Stop every audio level watcher so the mic visualiser doesn't keep
    // running after the call ends.
    this._audioWatchers.forEach(stop => { try { stop(); } catch {} });
    this._audioWatchers.clear();
    this.speakingIds.clear();
    this.speakingChanged$.next(0);
    this.bumpConferenceUI();

    this.localStream?.getTracks().forEach(t => t.stop());
    this.localStream  = null;
    this.remoteStream = null;
    this.remoteStream$.next(null);
    this.detachRemoteAudioSink();

    if (this.pc) { try { this.pc.close(); } catch {} this.pc = null; }

    this.isCallActive          = false;
    this.isMinimized           = false;
    this.activeCall            = null;
    this.callDuration          = 0;
    this.rtcState              = 'closed';
    this.activeCallOtherId     = '';
    this.incomingCallData      = null;
    this.isSettingRemoteAnswer = false;
    this.iceCandidateQueue     = [];
    clearInterval(this.callTimer);
    clearTimeout(this.disconnectTimer);

    // ✅ Streams clear — but callEnded$ null se clear karo
    // Pehle callEnded$ ko null karo taaki subscriber loop nahi hoga
    this.chatSvc.callAccepted$.next(null);
    this.chatSvc.callRejected$.next(null);
    this.chatSvc.iceCandidate$.next(null);
    this.chatSvc.incomingCall$.next(null);
    // callEnded$ last mein — aur sirf ek baar
    if (sendSignal) {
      // Apna khud ka signal — subscribers ko batao
      setTimeout(() => {
        this.chatSvc.callEnded$.next(null);
        this._isEnding = false;
      }, 50);
    } else {
      this.chatSvc.callEnded$.next(null);
      this._isEnding = false;
    }
  }

  // ── Mini bar ────────────────────────────
  showMiniBar(name: string, type: 'audio' | 'video') {
    this.activeCall  = { name, type };
    this.isMinimized = true;
  }

  hideMiniBar() {
    this.isMinimized = false;
  }

  expandCall() {
    this.isMinimized = false;
    this.expandCallRequest$.next(true);
    setTimeout(() => this.expandCallRequest$.next(false), 100);
  }

  endMiniBar() {
    // Mini bar se end — full call end
    this.endCall(true);
  }

  getMiniDuration(): string {
    const m = Math.floor(this.callDuration / 60);
    const s = this.callDuration % 60;
    return `${m.toString().padStart(2,'0')}:${s.toString().padStart(2,'0')}`;
  }

  toggleMute(isMuted: boolean) {
    this.localStream?.getAudioTracks()
      .forEach(t => t.enabled = !isMuted);
  }

  toggleCamera(isCameraOff: boolean) {
    this.localStream?.getVideoTracks()
      .forEach(t => t.enabled = !isCameraOff);
  }

  acquireExternalMediaConsumer(): () => void {
    this.externalMediaConsumers += 1;
    this.detachRemoteAudioSink();
    let released = false;
    return () => {
      if (released) return;
      released = true;
      this.externalMediaConsumers = Math.max(0, this.externalMediaConsumers - 1);
    };
  }

  /**
   * Attach a Web Audio AnalyserNode to a media stream and emit speaking
   * activity for the given userId. RMS averaged over a short window;
   * a small hysteresis avoids flicker. Safe to call multiple times for
   * the same id — the previous watcher is replaced.
   */
  attachAudioWatcher(userId: string, stream: MediaStream | null): void {
    if (!stream || !userId) return;
    // Replace any existing watcher for this id.
    const prev = this._audioWatchers.get(userId);
    if (prev) { try { prev(); } catch {} }

    const tracks = stream.getAudioTracks();
    if (!tracks.length) return;

    let ctx: AudioContext | null = null;
    let raf = 0;
    let lastSpeak = 0;
    try {
      const Ctor = (window as any).AudioContext
        || (window as any).webkitAudioContext;
      if (!Ctor) return;
      const audioCtx: AudioContext = new Ctor();
      ctx = audioCtx;
      const src = audioCtx.createMediaStreamSource(stream);
      const analyser = audioCtx.createAnalyser();
      analyser.fftSize = 512;
      analyser.smoothingTimeConstant = 0.6;
      src.connect(analyser);
      const buf = new Uint8Array(analyser.frequencyBinCount);

      const tick = () => {
        analyser.getByteTimeDomainData(buf);
        // RMS around the 128 baseline.
        let sum = 0;
        for (let i = 0; i < buf.length; i++) {
          const v = (buf[i] - 128) / 128;
          sum += v * v;
        }
        const rms = Math.sqrt(sum / buf.length);
        const now = performance.now();
        if (rms > 0.04) lastSpeak = now;
        const speaking = (now - lastSpeak) < 350;
        const had = this.speakingIds.has(userId);
        if (speaking && !had) {
          this.speakingIds.add(userId);
          this.speakingChanged$.next(this.speakingChanged$.value + 1);
        } else if (!speaking && had) {
          this.speakingIds.delete(userId);
          this.speakingChanged$.next(this.speakingChanged$.value + 1);
        }
        raf = requestAnimationFrame(tick);
      };
      raf = requestAnimationFrame(tick);
    } catch { /* AudioContext may not be allowed in some contexts */ }

    this._audioWatchers.set(userId, () => {
      cancelAnimationFrame(raf);
      try { ctx?.close(); } catch {}
      this.speakingIds.delete(userId);
      this.speakingChanged$.next(this.speakingChanged$.value + 1);
    });
  }

  // ═════════════════════════════════════════════════════════
  // CONFERENCE / GROUP CALL  (mesh)
  // ═════════════════════════════════════════════════════════

  /** Promote a 1-to-1 to a conference and invite N more users.
   *  Also used to add more people to an existing conference. */
  async inviteParticipants(userIds: string[]) {
    if (!userIds?.length) return;
    if (!this.currentCallLogId || !this.isCallActive) return;

    // First-time promotion: migrate the legacy 1-to-1 PC into the
    // peers map so it sits alongside future peer connections.
    if (!this.isConference) {
      this.isConference = true;
      if (this.pc && this.activeCallOtherId) {
        this.peers.set(this.activeCallOtherId, this.pc);
        if (this.remoteStream) {
          this.remoteStreams.set(
            this.activeCallOtherId, this.remoteStream);
        }
        // Other participant's name will arrive via roster when we ask
        // for the room. For now, register a placeholder so UI shows
        // a tile.
        if (!this.participants.has(this.activeCallOtherId)) {
          this.participants.set(this.activeCallOtherId, {
            id: this.activeCallOtherId,
            name: this.activeCall?.name || 'Participant'
          });
        }
        // Re-bind onicecandidate so it routes via the conference relay
        // instead of the legacy SendIceCandidate.
        this.rebindPeerIce(this.activeCallOtherId, this.pc);
      }
      this.bumpConferenceUI();
    }

    await this.chatSvc.inviteToConference(
      this.currentCallLogId, userIds);
  }

  /** Accept a conference invite popup (we are NOT in a call yet). */
  async acceptConferenceInvite() {
    if (!this.conferenceInvite) return;
    const invite = this.conferenceInvite;
    const callLogId = String(invite.callLogId || invite.CallLogId);
    const callType: 'audio' | 'video' =
      (invite.callType || invite.CallType || 'audio');

    this.conferenceInvite = null;
    this.stopRingtone();

    this.callType = callType;
    this.currentCallLogId = callLogId;
    this.isConference = true;
    this.iceCandidateQueue = [];

    // Navigate to chat so UI is mounted.
    if (this._navigateToChat) this._navigateToChat();

    try {
      this.localStream = await navigator.mediaDevices
        .getUserMedia({
          audio: true, video: callType === 'video'
        });
      this.attachAudioWatcher(
        this.myUserId || 'me', this.localStream);

      const res: any = await this.chatSvc.joinConference(callLogId);
      const existing = (res?.participants ?? res?.Participants ?? []) as any[];

      // Register each existing peer in the roster, then dial them.
      for (const p of existing) {
        const id = String(p.userId || p.UserId);
        if (this.myUserId && id === this.myUserId) continue;
        const name = String(p.fullName || p.FullName || 'Participant');
        const photo = p.photoUrl || p.PhotoUrl || null;
        this.participants.set(id, { id, name, photoUrl: photo });
        await this.dialPeer(id);
      }
      this.isCallActive = true;
      this.isMinimized  = false;
      this.callDuration = 0;
      this.startCallTimer();
      this.bumpConferenceUI();
    } catch (e) {
      console.error('acceptConferenceInvite error', e);
      this.endCall(true);
    }
  }

  /** Decline a conference invite popup (do not join the room). */
  rejectConferenceInvite() {
    const invite = this.conferenceInvite;
    this.conferenceInvite = null;
    this.stopRingtone();
    if (invite) {
      const callLogId = String(invite.callLogId || invite.CallLogId);
      this.chatSvc.rejectConference(callLogId);
    }
  }

  /** Create a fresh PC to `peerId`, attach local tracks, send offer. */
  private async dialPeer(peerId: string): Promise<void> {
    if (!this.currentCallLogId) return;
    if (this.myUserId && peerId === this.myUserId) return;
    if (this.peers.has(peerId)) return;

    const pc = this.makeConferencePc(peerId);
    this.peers.set(peerId, pc);

    if (this.localStream) {
      await this.attachLocalTracks(pc, this.localStream);
    }

    try {
      const offer = await pc.createOffer();
      await pc.setLocalDescription(offer);
      await this.chatSvc.relayConferenceOffer(
        this.currentCallLogId, peerId, JSON.stringify(offer));
    } catch (e) {
      console.error('dialPeer error', e);
      this.removePeer(peerId);
    }
  }

  /** Handle an incoming conference SDP offer from a (possibly new) peer. */
  private async handleConferenceOffer(d: any): Promise<void> {
    const fromId = String(d.fromUserId || d.FromUserId);
    if (this.myUserId && fromId === this.myUserId) return;
    if (!this.currentCallLogId) return;
    // Auto-promote: if we get a conference offer while still in 1-to-1
    // mode for this same call, flip into conference mode and migrate
    // the existing peer connection.
    if (!this.isConference && this.isCallActive) {
      this.isConference = true;
      if (this.pc && this.activeCallOtherId
          && !this.peers.has(this.activeCallOtherId)) {
        this.peers.set(this.activeCallOtherId, this.pc);
        if (this.remoteStream) {
          this.remoteStreams.set(this.activeCallOtherId, this.remoteStream);
        }
        if (!this.participants.has(this.activeCallOtherId)) {
          this.participants.set(this.activeCallOtherId, {
            id: this.activeCallOtherId,
            name: this.activeCall?.name
              || this.incomingCallData?.callerName
              || 'Participant'
          });
        }
        this.rebindPeerIce(this.activeCallOtherId, this.pc);
      }
      this.bumpConferenceUI();
    }
    let pc = this.peers.get(fromId);
    if (!pc) {
      pc = this.makeConferencePc(fromId);
      this.peers.set(fromId, pc);
      if (this.localStream) {
        await this.attachLocalTracks(pc, this.localStream);
      }
    }
    try {
      const offer = this.parseRtcJson(d.offer || d.Offer);
      if (!offer) throw new Error('Conference offer missing');
      await pc.setRemoteDescription(new RTCSessionDescription(offer));
      await this.flushPeerIce(fromId);
      const answer = await pc.createAnswer();
      await pc.setLocalDescription(answer);
      await this.chatSvc.relayConferenceAnswer(
        this.currentCallLogId, fromId, JSON.stringify(answer));
    } catch (e) {
      console.error('handleConferenceOffer error', e);
      this.removePeer(fromId);
    }
  }

  private makeConferencePc(peerId: string): RTCPeerConnection {
    const pc = new RTCPeerConnection(this.buildRtcConfig());
    pc.onicecandidate = (e) => {
      if (!e.candidate || !this.currentCallLogId) return;
      this.chatSvc.relayConferenceIce(
        this.currentCallLogId, peerId, JSON.stringify(e.candidate));
    };
    pc.ontrack = (e) => {
      const stream = this.getPeerRemoteStream(peerId, e);
      this.remoteStreams.set(peerId, stream);
      this.attachAudioWatcher(peerId, stream);
      this.bumpConferenceUI();
    };
    pc.onconnectionstatechange = () => {
      if (pc.connectionState === 'failed' ||
          pc.connectionState === 'closed') {
        this.removePeer(peerId);
        this.bumpConferenceUI();
      }
    };
    return pc;
  }

  /** Re-attach the ice handler of a legacy 1-to-1 PC so its candidates
   *  flow through the conference relay path instead. */
  private rebindPeerIce(peerId: string, pc: RTCPeerConnection): void {
    pc.onicecandidate = (e) => {
      if (!e.candidate || !this.currentCallLogId) return;
      this.chatSvc.relayConferenceIce(
        this.currentCallLogId, peerId, JSON.stringify(e.candidate));
    };
  }

  private async flushPeerIce(peerId: string): Promise<void> {
    const pc = this.peers.get(peerId);
    const queue = this.peerIceQueue.get(peerId);
    if (!pc || !queue?.length) return;
    while (queue.length) {
      const c = queue.shift()!;
      try { await pc.addIceCandidate(new RTCIceCandidate(c)); } catch {}
    }
  }

  private removePeer(peerId: string): void {
    const pc = this.peers.get(peerId);
    if (pc) { try { pc.close(); } catch {} }
    this.peers.delete(peerId);
    this.remoteStreams.delete(peerId);
    this.peerIceQueue.delete(peerId);
  }

  private bumpConferenceUI(): void {
    this.conferenceChanged$.next(
      this.conferenceChanged$.getValue() + 1);
  }

  /** Live participant tiles (id, name, photo, stream). */
  getConferenceTiles(): Array<ConferenceParticipant & {
    stream: MediaStream | null
  }> {
    const list: Array<ConferenceParticipant & {
      stream: MediaStream | null
    }> = [];
    this.participants.forEach((p, id) => {
      if (this.myUserId && id === this.myUserId) return;
      list.push({ ...p, stream: this.remoteStreams.get(id) || null });
    });
    return list;
  }

  /** Send an in-call chat message to the room. Also echoes locally so
   *  the sender sees their own message instantly. */
  async sendCallMessage(text: string): Promise<void> {
    const msg = (text || '').trim();
    if (!msg || !this.currentCallLogId) return;
    // Local echo for instant feedback (server will also broadcast back,
    // but we filter dupes by ignoring the echo for `mine`).
    this.callMessages.push({
      fromId: this.myUserId || 'me',
      fromName: 'You',
      text: msg,
      at: new Date(),
      mine: true
    });
    this.callChatChanged$.next(this.callMessages.length);
    try {
      await this.chatSvc.sendCallMessage(this.currentCallLogId, msg);
    } catch (e) {
      console.error('sendCallMessage error', e);
    }
  }

  // ── PeerConnection ───────────────────────
  private createPeerConnection(): RTCPeerConnection {
    const pc = new RTCPeerConnection(this.buildRtcConfig());
    this.startDiagnostics(pc);

    pc.onicecandidate = (e) => {
      if (!e.candidate || !this.activeCallOtherId) return;
      this.chatSvc.sendIceCandidate(
        this.activeCallOtherId,
        JSON.stringify(e.candidate));
    };

    pc.ontrack = (e) => {
      this.remoteStream = this.ensureMainRemoteStream(e);
      this.ensureRemoteAudioSink(this.remoteStream);
      this.remoteStream$.next(this.remoteStream);
      if (this.activeCallOtherId) {
        this.attachAudioWatcher(
          this.activeCallOtherId, this.remoteStream);
      }
    };

    pc.onconnectionstatechange = () => {
      const state = pc.connectionState;
      this.rtcState = state;

      // Connected again after a short network/tab hiccup.
      if (state === 'connected') {
        clearTimeout(this.disconnectTimer);
        return;
      }

      // Browsers may briefly report disconnected on tab switch/background.
      // End only if it stays disconnected for a grace period.
      if (state === 'disconnected') {
        clearTimeout(this.disconnectTimer);
        this.disconnectTimer = setTimeout(() => {
          if (this.isCallActive && pc.connectionState === 'disconnected') {
            this.endCall(false);
          }
        }, 15000);
        return;
      }

      if (state === 'failed' || state === 'closed') {
        this.stopDiagnostics();
        clearTimeout(this.disconnectTimer);
        if (this.isCallActive) this.endCall(false);
      }
    };

    return pc;
  }

  private parseRtcJson(value: unknown): any | null {
    if (!value) return null;
    if (typeof value === 'string') {
      try { return JSON.parse(value); }
      catch { return null; }
    }
    if (typeof value === 'object') return value;
    return null;
  }

  private buildRtcConfig(): RTCConfiguration {
    const envCfg = environment as any;
    const configured = Array.isArray(envCfg.rtcIceServers)
      ? envCfg.rtcIceServers
      : [];
    const servers = (configured.length
      ? configured
      : this.fallbackIceServers) as RTCIceServer[];

    const cfg: RTCConfiguration = {
      iceServers: servers,
      iceCandidatePoolSize: 10
    };

    if (envCfg.rtcForceRelay === true) {
      cfg.iceTransportPolicy = 'relay';
    }
    return cfg;
  }

  private getMediaConstraints(type: 'audio' | 'video'): MediaStreamConstraints {
    return {
      audio: this.audioConstraints,
      video: type === 'video' ? this.videoConstraints : false
    };
  }

  private async openLocalMedia(type: 'audio' | 'video'): Promise<MediaStream> {
    try {
      return await navigator.mediaDevices.getUserMedia(
        this.getMediaConstraints(type));
    } catch {
      return navigator.mediaDevices.getUserMedia({
        audio: true,
        video: type === 'video'
      });
    }
  }

  private async attachLocalTracks(
    pc: RTCPeerConnection,
    stream: MediaStream
  ): Promise<void> {
    for (const track of stream.getTracks()) {
      try {
        (track as any).contentHint =
          track.kind === 'video' ? 'motion' : 'speech';
      } catch {}

      const sender = pc.addTrack(track, stream);
      await this.tuneSender(sender, track.kind);
    }
  }

  private async tuneSender(
    sender: RTCRtpSender,
    kind: string
  ): Promise<void> {
    if (!sender?.getParameters || !sender?.setParameters) return;
    if (kind !== 'audio' && kind !== 'video') return;
    try {
      const params = sender.getParameters() || {};
      const enc = (params.encodings && params.encodings.length)
        ? params.encodings
        : [{} as RTCRtpEncodingParameters];

      if (kind === 'video') {
        enc[0].maxBitrate = 1500000;
        enc[0].maxFramerate = 30;
        enc[0].scaleResolutionDownBy = 1;
        (params as any).degradationPreference = 'balanced';
      } else {
        enc[0].maxBitrate = 96000;
        (params as any).degradationPreference = 'balanced';
      }

      params.encodings = enc;
      await sender.setParameters(params);
    } catch {}
  }

  private ensureMainRemoteStream(e: RTCTrackEvent): MediaStream {
    if (!this.remoteStream) this.remoteStream = new MediaStream();
    const source = e.streams?.[0];
    if (source) {
      source.getTracks().forEach(t => {
        const exists = this.remoteStream!
          .getTracks()
          .some(x => x.id === t.id);
        if (!exists) this.remoteStream!.addTrack(t);
      });
    }

    const selfExists = this.remoteStream
      .getTracks()
      .some(t => t.id === e.track.id);
    if (!selfExists) this.remoteStream.addTrack(e.track);
    return this.remoteStream;
  }

  private getPeerRemoteStream(peerId: string, e: RTCTrackEvent): MediaStream {
    const current = this.remoteStreams.get(peerId) || new MediaStream();
    const source = e.streams?.[0];
    if (source) {
      source.getTracks().forEach(t => {
        const exists = current.getTracks().some(x => x.id === t.id);
        if (!exists) current.addTrack(t);
      });
    }

    const selfExists = current.getTracks().some(t => t.id === e.track.id);
    if (!selfExists) current.addTrack(e.track);
    return current;
  }

  private startDiagnostics(pc: RTCPeerConnection): void {
    this.stopDiagnostics();
    this.prevInboundBytes = 0;
    this.prevOutboundBytes = 0;
    this.prevDiagAt = 0;

    const tick = () => {
      this.collectDiagnostics(pc).catch(() => {});
    };

    tick();
    this.diagnosticsTimer = setInterval(tick, 3000);
  }

  private stopDiagnostics(): void {
    clearInterval(this.diagnosticsTimer);
    this.diagnosticsTimer = null;
    this.prevInboundBytes = 0;
    this.prevOutboundBytes = 0;
    this.prevDiagAt = 0;
  }

  private async collectDiagnostics(pc: RTCPeerConnection): Promise<void> {
    if (!pc || pc.connectionState === 'closed') return;

    const report = await pc.getStats();
    const statsById = new Map<string, any>();
    report.forEach((s: any) => statsById.set(s.id, s));

    const targetKind = this.callType === 'video' ? 'video' : 'audio';

    let selectedPair: any = null;
    let inboundBytes = 0;
    let outboundBytes = 0;
    let packetsLost = 0;
    let packetsReceived = 0;
    let jitterMs = 0;

    report.forEach((s: any) => {
      if (s.type === 'transport' && s.selectedCandidatePairId) {
        selectedPair = statsById.get(s.selectedCandidatePairId) || selectedPair;
      }
    });

    if (!selectedPair) {
      report.forEach((s: any) => {
        if (s.type === 'candidate-pair' &&
            (s.selected || (s.nominated && s.state === 'succeeded'))) {
          selectedPair = s;
        }
      });
    }

    let primaryInbound: any = null;
    let primaryOutbound: any = null;
    report.forEach((s: any) => {
      if (s.type === 'inbound-rtp' && !s.isRemote) {
        const kind = s.kind || s.mediaType;
        if (kind === targetKind) primaryInbound = s;
      }
      if (s.type === 'outbound-rtp' && !s.isRemote) {
        const kind = s.kind || s.mediaType;
        if (kind === targetKind) primaryOutbound = s;
      }
    });

    if (!primaryInbound) {
      report.forEach((s: any) => {
        if (!primaryInbound && s.type === 'inbound-rtp' && !s.isRemote) {
          primaryInbound = s;
        }
      });
    }
    if (!primaryOutbound) {
      report.forEach((s: any) => {
        if (!primaryOutbound && s.type === 'outbound-rtp' && !s.isRemote) {
          primaryOutbound = s;
        }
      });
    }

    if (primaryInbound) {
      inboundBytes = Number(primaryInbound.bytesReceived || 0);
      packetsLost = Number(primaryInbound.packetsLost || 0);
      packetsReceived = Number(primaryInbound.packetsReceived || 0);
      jitterMs = Number(primaryInbound.jitter || 0) * 1000;
    }
    if (primaryOutbound) {
      outboundBytes = Number(primaryOutbound.bytesSent || 0);
    }

    const now = Date.now();
    let inboundKbps = 0;
    let outboundKbps = 0;
    if (this.prevDiagAt > 0 && now > this.prevDiagAt) {
      const deltaMs = now - this.prevDiagAt;
      const inDelta = Math.max(0, inboundBytes - this.prevInboundBytes);
      const outDelta = Math.max(0, outboundBytes - this.prevOutboundBytes);
      inboundKbps = Math.round((inDelta * 8) / deltaMs);
      outboundKbps = Math.round((outDelta * 8) / deltaMs);
    }
    this.prevDiagAt = now;
    this.prevInboundBytes = inboundBytes;
    this.prevOutboundBytes = outboundBytes;

    const rttMs = Math.round(Number(selectedPair?.currentRoundTripTime || 0) * 1000);
    const availableOutgoingKbps = Math.round(
      Number(selectedPair?.availableOutgoingBitrate || 0) / 1000
    );
    const totalPackets = packetsReceived + packetsLost;
    const packetLossPct = totalPackets > 0
      ? Math.round((packetsLost * 10000) / totalPackets) / 100
      : 0;

    const localCandidateType = String(
      statsById.get(selectedPair?.localCandidateId)?.candidateType || ''
    );
    const remoteCandidateType = String(
      statsById.get(selectedPair?.remoteCandidateId)?.candidateType || ''
    );

    this.diagnostics$.next({
      quality: this.estimateQuality(rttMs, jitterMs, packetLossPct),
      rttMs: isFinite(rttMs) ? rttMs : 0,
      jitterMs: isFinite(jitterMs) ? Math.round(jitterMs) : 0,
      packetLossPct: isFinite(packetLossPct) ? packetLossPct : 0,
      inboundKbps,
      outboundKbps,
      availableOutgoingKbps: isFinite(availableOutgoingKbps)
        ? availableOutgoingKbps
        : 0,
      localCandidateType,
      remoteCandidateType,
      updatedAt: now
    });
  }

  private estimateQuality(
    rttMs: number,
    jitterMs: number,
    packetLossPct: number
  ): 'excellent' | 'good' | 'fair' | 'poor' {
    if (packetLossPct <= 1 && rttMs <= 120 && jitterMs <= 20) return 'excellent';
    if (packetLossPct <= 3 && rttMs <= 220 && jitterMs <= 35) return 'good';
    if (packetLossPct <= 7 && rttMs <= 350 && jitterMs <= 60) return 'fair';
    return 'poor';
  }

  private bindAudioUnlock(): void {
    if (this.audioUnlockBound) return;
    this.audioUnlockBound = true;
    const unlock = () => { this.ensureRingContext().catch(() => {}); };
    window.addEventListener('pointerdown', unlock, { passive: true, capture: true });
    window.addEventListener('keydown', unlock, { capture: true });
  }

  private async ensureRingContext(): Promise<AudioContext | null> {
    try {
      const Ctor = (window as any).AudioContext
        || (window as any).webkitAudioContext;
      if (!Ctor) return null;
      if (!this.ringCtx || this.ringCtx.state === 'closed') {
        this.ringCtx = new Ctor();
      }
      const ctx = this.ringCtx;
      if (!ctx) return null;
      if (ctx.state === 'suspended') {
        await ctx.resume();
      }
      return ctx;
    } catch {
      return null;
    }
  }

  private ensureRemoteAudioSink(stream: MediaStream | null): void {
    if (!stream) return;
    if (this.externalMediaConsumers > 0) {
      this.detachRemoteAudioSink();
      return;
    }
    if (this.isProbablyLocalStream(stream)) return;
    if (!this.remoteAudioEl) {
      const el = document.createElement('audio');
      el.autoplay = true;
      (el as any).playsInline = true;
      el.style.display = 'none';
      document.body.appendChild(el);
      this.remoteAudioEl = el;
    }

    if (this.remoteAudioEl.srcObject !== stream) {
      this.remoteAudioEl.srcObject = stream;
    }

    const p = this.remoteAudioEl.play();
    if (p && typeof p.catch === 'function') {
      p.catch(() => {
        const retry = () => {
          this.remoteAudioEl?.play().catch(() => {});
          window.removeEventListener('pointerdown', retry, true);
          window.removeEventListener('keydown', retry, true);
        };
        window.addEventListener('pointerdown', retry, true);
        window.addEventListener('keydown', retry, true);
      });
    }
  }

  private isProbablyLocalStream(stream: MediaStream): boolean {
    const local = this.localStream;
    if (!local) return false;
    if (stream === local) return true;

    const localTrackIds = new Set(local.getTracks().map(t => t.id));
    if (!localTrackIds.size) return false;
    return stream.getTracks().some(t => localTrackIds.has(t.id));
  }

  private detachRemoteAudioSink(): void {
    if (!this.remoteAudioEl) return;
    try {
      this.remoteAudioEl.srcObject = null;
      this.remoteAudioEl.remove();
    } catch {}
    this.remoteAudioEl = null;
  }

  private async flushIceCandidates() {
    if (!this.pc) return;
    while (this.iceCandidateQueue.length) {
      const c = this.iceCandidateQueue.shift()!;
      try {
        await this.pc.addIceCandidate(new RTCIceCandidate(c));
      } catch {}
    }
  }

  private startCallTimer() {
    clearInterval(this.callTimer);
    this.callTimer = setInterval(() => this.callDuration++, 1000);
  }

  private playRingtone() {
    this.ensureRingContext().then(ctx => {
      if (!ctx) return;
      try {
        this.stopRingtone();
        let stopped = false;
        const beep = () => {
          if (stopped) return;
          const now = ctx.currentTime;
          const o = ctx.createOscillator();
          const g = ctx.createGain();
          o.connect(g); g.connect(ctx.destination);
          o.frequency.setValueAtTime(440, now);
          o.type = 'sine';
          g.gain.setValueAtTime(0.0001, now);
          g.gain.exponentialRampToValueAtTime(0.20, now + 0.03);
          g.gain.exponentialRampToValueAtTime(0.0001, now + 0.45);
          o.start(now);
          o.stop(now + 0.48);
          if (!stopped) setTimeout(beep, 1200);
        };
        beep();
        this._stopRingFn = () => { stopped = true; };
      } catch {}
    }).catch(() => {});
  }

  private stopRingtone() {
    if (this._stopRingFn) { this._stopRingFn(); this._stopRingFn = null; }
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    this.stopRingtone();
    clearInterval(this.callTimer);
    clearTimeout(this.disconnectTimer);
  }
}
