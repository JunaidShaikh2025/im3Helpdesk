import { TestBed } from '@angular/core/testing';
import { BehaviorSubject } from 'rxjs';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { GlobalCallNotificationService } from './global-call-notification.service';
import { ChatService } from './chat.service';

describe('GlobalCallNotificationService - Timer Leaks & Teardown (PERF-04)', () => {
  let service: GlobalCallNotificationService;
  let mockChatService: any;

  beforeEach(() => {
    vi.useFakeTimers();
    localStorage.clear();

    mockChatService = {
      incomingCall$: new BehaviorSubject<any>(null),
      callInitiated$: new BehaviorSubject<any>(null),
      callAccepted$: new BehaviorSubject<any>(null),
      callRejected$: new BehaviorSubject<any>(null),
      callEnded$: new BehaviorSubject<any>(null),
      iceCandidate$: new BehaviorSubject<any>(null),
      conferenceInvite$: new BehaviorSubject<any>(null),
      conferenceParticipantJoined$: new BehaviorSubject<any>(null),
      conferenceParticipantLeft$: new BehaviorSubject<any>(null),
      conferenceOffer$: new BehaviorSubject<any>(null),
      conferenceAnswer$: new BehaviorSubject<any>(null),
      conferenceIce$: new BehaviorSubject<any>(null),
      callMessage$: new BehaviorSubject<any>(null),
      leaveConference: vi.fn(),
      endCall: vi.fn(),
      rejectCall: vi.fn(),
      acceptCall: vi.fn().mockResolvedValue(undefined),
      initiateCall: vi.fn().mockResolvedValue(undefined),
      rejectConference: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        GlobalCallNotificationService,
        { provide: ChatService, useValue: mockChatService }
      ]
    });

    service = TestBed.inject(GlobalCallNotificationService);
  });

  afterEach(() => {
    service.ngOnDestroy();
    vi.useRealTimers();
  });

  it('should initialize with all background timers null and inactive', () => {
    expect(service.isCallTimerActive).toBe(false);
    expect(service.isDiagnosticsTimerActive).toBe(false);
    expect(service.isDisconnectTimerActive).toBe(false);
    expect(service.callDuration).toBe(0);
  });

  it('should start callTimer and increment callDuration reliably every second', () => {
    service.isCallActive = true;
    service.startCallTimer();

    expect(service.isCallTimerActive).toBe(true);
    expect(service.callDuration).toBe(0);

    vi.advanceTimersByTime(1000);
    expect(service.callDuration).toBe(1);

    vi.advanceTimersByTime(2500);
    expect(service.callDuration).toBe(3);
  });

  it('should prevent timer leaks on repeated startCallTimer calls', () => {
    service.isCallActive = true;
    service.startCallTimer();
    const firstTimer = (service as any).callTimer;

    service.startCallTimer();
    const secondTimer = (service as any).callTimer;

    expect(service.isCallTimerActive).toBe(true);
    expect(secondTimer).not.toBe(firstTimer);

    vi.advanceTimersByTime(1000);
    expect(service.callDuration).toBe(1);
  });

  it('should stop callTimer and nullify handle when stopCallTimer is called', () => {
    service.isCallActive = true;
    service.startCallTimer();
    expect(service.isCallTimerActive).toBe(true);

    service.stopCallTimer();
    expect(service.isCallTimerActive).toBe(false);

    vi.advanceTimersByTime(2000);
    expect(service.callDuration).toBe(0);
  });

  it('should auto-terminate callTimer if isCallActive transitions to false', () => {
    service.isCallActive = true;
    service.startCallTimer();
    expect(service.isCallTimerActive).toBe(true);

    service.isCallActive = false;
    vi.advanceTimersByTime(1000);

    expect(service.isCallTimerActive).toBe(false);
  });

  it('should start diagnosticsTimer and stop it cleanly via stopDiagnostics', () => {
    const mockPc: any = {
      connectionState: 'connected',
      getStats: vi.fn().mockResolvedValue(new Map())
    };
    service.pc = mockPc;

    (service as any).startDiagnostics(mockPc);
    expect(service.isDiagnosticsTimerActive).toBe(true);

    service.stopDiagnostics();
    expect(service.isDiagnosticsTimerActive).toBe(false);
  });

  it('should auto-terminate diagnosticsTimer if peer connection enters closed or failed state', () => {
    const mockPc: any = {
      connectionState: 'connected',
      getStats: vi.fn().mockResolvedValue(new Map())
    };
    service.pc = mockPc;

    (service as any).startDiagnostics(mockPc);
    expect(service.isDiagnosticsTimerActive).toBe(true);

    mockPc.connectionState = 'closed';
    vi.advanceTimersByTime(3000);

    expect(service.isDiagnosticsTimerActive).toBe(false);
  });

  it('should clear disconnectTimer and nullify handle cleanly', () => {
    service.clearDisconnectTimer();
    expect(service.isDisconnectTimerActive).toBe(false);

    (service as any).disconnectTimer = setTimeout(() => {}, 10000);
    expect(service.isDisconnectTimerActive).toBe(true);

    service.clearDisconnectTimer();
    expect(service.isDisconnectTimerActive).toBe(false);
  });

  it('should clear all timers and reset callDuration when endCall is executed', () => {
    service.isCallActive = true;
    service.startCallTimer();
    (service as any).disconnectTimer = setTimeout(() => {}, 15000);

    const mockPc: any = {
      connectionState: 'connected',
      getStats: vi.fn().mockResolvedValue(new Map()),
      close: vi.fn()
    };
    service.pc = mockPc;
    (service as any).startDiagnostics(mockPc);

    expect(service.isCallTimerActive).toBe(true);
    expect(service.isDiagnosticsTimerActive).toBe(true);
    expect(service.isDisconnectTimerActive).toBe(true);

    service.endCall(false);

    expect(service.isCallTimerActive).toBe(false);
    expect(service.isDiagnosticsTimerActive).toBe(false);
    expect(service.isDisconnectTimerActive).toBe(false);
    expect(service.callDuration).toBe(0);
    expect(service.isCallActive).toBe(false);
  });

  it('should clear all timers when rejectIncomingCall is invoked', () => {
    service.startCallTimer();
    (service as any).disconnectTimer = setTimeout(() => {}, 15000);

    expect(service.isCallTimerActive).toBe(true);
    expect(service.isDisconnectTimerActive).toBe(true);

    service.rejectIncomingCall();

    expect(service.isCallTimerActive).toBe(false);
    expect(service.isDisconnectTimerActive).toBe(false);
  });

  it('should clear all timers when rejectConferenceInvite is invoked', () => {
    service.startCallTimer();
    (service as any).disconnectTimer = setTimeout(() => {}, 15000);

    expect(service.isCallTimerActive).toBe(true);
    expect(service.isDisconnectTimerActive).toBe(true);

    service.rejectConferenceInvite();

    expect(service.isCallTimerActive).toBe(false);
    expect(service.isDisconnectTimerActive).toBe(false);
  });

  it('should clear all timers and close peer connection on ngOnDestroy', () => {
    service.isCallActive = true;
    service.startCallTimer();
    (service as any).disconnectTimer = setTimeout(() => {}, 15000);

    const mockPc: any = {
      connectionState: 'connected',
      getStats: vi.fn().mockResolvedValue(new Map()),
      close: vi.fn()
    };
    service.pc = mockPc;
    (service as any).startDiagnostics(mockPc);

    service.ngOnDestroy();

    expect(service.isCallTimerActive).toBe(false);
    expect(service.isDiagnosticsTimerActive).toBe(false);
    expect(service.isDisconnectTimerActive).toBe(false);
    expect(mockPc.close).toHaveBeenCalled();
    expect(service.pc).toBeNull();
  });
});
