import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { ChatService } from './chat.service';
import { environment } from '../../../environments/environment';

describe('ChatService - PERF-05 Consolidated SignalR Chat Service', () => {
  let service: ChatService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        ChatService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ChatService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    service.disconnect();
  });

  it('should be created and initialized with expected stream defaults', () => {
    expect(service).toBeTruthy();
    expect(service.isConnected).toBe(false);
    expect(service.unreadCount$.getValue()).toBe(0);
    expect(service.messages$.getValue()).toEqual([]);
    expect(service.newMessage$.getValue()).toBeNull();
    expect(service.currentlyViewing$.getValue()).toBeNull();
    expect(service.missedCallCount$.getValue()).toBe(0);
  });

  it('should update and clear currently viewing chat thread', () => {
    service.setCurrentlyViewing('dm', 'user-42');
    expect(service.currentlyViewing$.getValue()).toEqual({ kind: 'dm', id: 'user-42' });

    service.setCurrentlyViewing('group', 'group-99');
    expect(service.currentlyViewing$.getValue()).toEqual({ kind: 'group', id: 'group-99' });

    service.clearCurrentlyViewing();
    expect(service.currentlyViewing$.getValue()).toBeNull();

    service.setCurrentlyViewing('dm', null);
    expect(service.currentlyViewing$.getValue()).toBeNull();
  });

  it('should clear messages stream and new message pointer', () => {
    service.messages$.next([{ id: 1, text: 'Hello' }]);
    service.newMessage$.next({ id: 1, text: 'Hello' });

    service.clearMessages();

    expect(service.messages$.getValue()).toEqual([]);
    expect(service.newMessage$.getValue()).toBeNull();
  });

  it('should fetch chat users via HTTP GET', () => {
    const mockUsers = [{ id: 'u1', name: 'Alice' }, { id: 'u2', name: 'Bob' }];

    service.getChatUsers().subscribe(users => {
      expect(users).toEqual(mockUsers);
    });

    const req = httpMock.expectOne(`${environment.baseUrl}/api/Chat/users`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);
  });

  it('should fetch user messages with pagination', () => {
    const mockMessages = [{ id: 'm1', content: 'Hi' }];

    service.getMessages('user-1', 2).subscribe(msgs => {
      expect(msgs).toEqual(mockMessages);
    });

    const req = httpMock.expectOne(`${environment.baseUrl}/api/Chat/messages/user-1?page=2&pageSize=50`);
    expect(req.request.method).toBe('GET');
    req.flush(mockMessages);
  });

  it('should fetch chat groups via HTTP GET', () => {
    const mockGroups = [{ id: 'g1', name: 'Support Team' }];

    service.getGroups().subscribe(groups => {
      expect(groups).toEqual(mockGroups);
    });

    const req = httpMock.expectOne(`${environment.baseUrl}/api/Chat/groups`);
    expect(req.request.method).toBe('GET');
    req.flush(mockGroups);
  });

  it('should create group via HTTP POST', () => {
    const payload = { name: 'DevOps', memberIds: ['u1', 'u2'] };

    service.createGroup(payload).subscribe(res => {
      expect(res.success).toBe(true);
    });

    const req = httpMock.expectOne(`${environment.baseUrl}/api/Chat/groups`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ success: true });
  });

  it('should fetch unread count and update unreadCount$ BehaviorSubject', () => {
    service.loadUnreadCount();

    const req = httpMock.expectOne(`${environment.baseUrl}/api/Chat/unread-count`);
    expect(req.request.method).toBe('GET');
    req.flush({ count: 5 });

    expect(service.unreadCount$.getValue()).toBe(5);
  });

  it('should mark call logs read and reset missedCallCount$ to zero', () => {
    service.missedCallCount$.next(3);

    service.markCallsRead().subscribe();

    const req = httpMock.expectOne(`${environment.baseUrl}/api/CallLog/mark-read`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });

    expect(service.missedCallCount$.getValue()).toBe(0);
  });

  it('should emit callback request on startCallFromLog', () => {
    service.startCallFromLog('user-123', 'video');
    expect(service.startCallRequest$.getValue()).toEqual({
      userId: 'user-123',
      type: 'video'
    });
  });

  it('should safely resolve ticket room methods when disconnected', async () => {
    await expect(service.joinTicketRoom('ticket-101')).resolves.toBeUndefined();
    await expect(service.leaveTicketRoom('ticket-101')).resolves.toBeUndefined();
    await expect(service.joinOrgRoom('org-202')).resolves.toBeUndefined();
    await expect(service.sendTicketMessage('ticket-101', 'msg', 'Agent', true)).resolves.toBeUndefined();
  });

  it('should reject send and sendGroup when hub connection is not established', async () => {
    await expect(service.sendMessage('user-1', 'hello')).rejects.toThrow('Chat connection is unavailable');
    await expect(service.sendGroupMessage('group-1', 'hello')).rejects.toThrow('Chat connection is unavailable');
  });
});
