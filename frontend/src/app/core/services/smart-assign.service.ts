import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { TOKEN_KEY } from '../constants/auth.constants';

export interface AgentRecommendation {
  agentId: string;
  agentName: string;
  agentEmail?: string;
  photoUrl?: string;
  isOnline: boolean;
  mlConfidence: number;
  finalScore: number;
  rank: number;
  reasons: string[];
  currentActiveTickets: number;
  highPriorityTickets: number;
  avgResolutionHours: number;
  slaComplianceRate: number;
}

export interface TicketSmartAssignResult {
  ticketId: string;
  ticketNumber: number;
  title: string;
  category: string;
  priority: number | string;
  customerName?: string;
  mode: 'Suggest' | 'Auto';
  autoAssigned: boolean;
  assignedToAgentId?: string;
  assignedToAgentName?: string;
  confidenceScore: number;
  topRecommendations: AgentRecommendation[];
  decisionExplanation: string;
  modelEvaluatedAt: string;
}

export interface AgentWorkloadStat {
  agentId: string;
  agentName: string;
  agentEmail?: string;
  photoUrl?: string;
  role: string;
  isOnline: boolean;
  lastSeen?: string;
  activeTicketCount: number;
  highPriorityTicketCount: number;
  assignedTodayCount: number;
}

export interface SmartAssignStats {
  unassignedTicketsCount: number;
  autoAssignedToday: number;
  autoAssignedTotal: number;
  modelAccuracy: number;
  modelTop3Accuracy: number;
  overrideRate: number;
  totalDecisions: number;
  overrideCount: number;
  activeModelVersion: string;
  lastTrainedAt?: string;
  trainingSamplesCount: number;
  mode: string;
  confidenceThreshold: number;
  weightMl: number;
  weightWorkload: number;
  weightAvailability: number;
  agentWorkloads: AgentWorkloadStat[];
}

export interface SmartAssignConfig {
  id: string;
  organizationId: string;
  mode: 'Suggest' | 'Auto';
  confidenceThreshold: number;
  weightMl: number;
  weightWorkload: number;
  weightAvailability: number;
  autoRetrainFrequencyDays: number;
  minTrainingSamples: number;
}

@Injectable({ providedIn: 'root' })
export class SmartAssignService {
  private http = inject(HttpClient);
  readonly BASE = environment.baseUrl;

  private hub?: signalR.HubConnection;

  // Real-time SignalR streams
  public smartAssigned$ = new BehaviorSubject<{
    ticketId: string;
    ticketNumber: number;
    title: string;
    agentId: string;
    agentName: string;
    confidence: number;
    isAuto: boolean;
  } | null>(null);

  public suggestionGenerated$ = new BehaviorSubject<{
    ticketId: string;
    ticketNumber: number;
    title: string;
    recommendations: any;
  } | null>(null);

  public modelRetrained$ = new BehaviorSubject<{
    version: string;
    accuracy: number;
    top3Accuracy: number;
    sampleCount: number;
  } | null>(null);

  public workloadUpdated$ = new BehaviorSubject<{
    agentId: string;
    activeCount: number;
    highPriorityCount: number;
  } | null>(null);

  public isConnected$ = new BehaviorSubject<boolean>(false);

  // ── REST API Methods ───────────────────
  getStats(): Observable<SmartAssignStats> {
    return this.http.get<SmartAssignStats>(`${this.BASE}/api/ai/smart-assign/stats`);
  }

  getUnassignedQueue(limit = 20): Observable<TicketSmartAssignResult[]> {
    return this.http.get<TicketSmartAssignResult[]>(`${this.BASE}/api/ai/smart-assign/unassigned?limit=${limit}`);
  }

  getRecommendation(ticketId: string): Observable<TicketSmartAssignResult> {
    return this.http.get<TicketSmartAssignResult>(`${this.BASE}/api/ai/smart-assign/${ticketId}`);
  }

  applyAssignment(ticketId: string, agentId: string, reason?: string): Observable<any> {
    return this.http.post<any>(`${this.BASE}/api/ai/smart-assign/${ticketId}/apply`, {
      agentId,
      reason
    });
  }

  retrainModel(): Observable<{
    success: boolean;
    version: string;
    accuracy: number;
    top3Accuracy: number;
    samplesCount: number;
    message: string;
  }> {
    return this.http.post<any>(`${this.BASE}/api/ai/smart-assign/retrain`, {});
  }

  getConfig(): Observable<SmartAssignConfig> {
    return this.http.get<SmartAssignConfig>(`${this.BASE}/api/ai/smart-assign/config`);
  }

  updateConfig(config: Partial<SmartAssignConfig>): Observable<SmartAssignConfig> {
    return this.http.post<SmartAssignConfig>(`${this.BASE}/api/ai/smart-assign/config`, config);
  }

  // ── SignalR Hub Connection ─────────────
  connectHub(organizationId?: string): void {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${this.BASE}/hubs/smart-assign`, {
        withCredentials: true,
        accessTokenFactory: () => localStorage.getItem(TOKEN_KEY) || ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.hub.on('SmartAssigned', (data) => {
      this.smartAssigned$.next(data);
    });

    this.hub.on('SuggestionGenerated', (data) => {
      this.suggestionGenerated$.next(data);
    });

    this.hub.on('ModelRetrained', (data) => {
      this.modelRetrained$.next(data);
    });

    this.hub.on('WorkloadUpdated', (data) => {
      this.workloadUpdated$.next(data);
    });

    this.hub
      .start()
      .then(() => {
        this.isConnected$.next(true);
        if (organizationId) {
          this.hub?.invoke('JoinOrgRoom', organizationId).catch(err => {
            console.warn('[SmartAssignHub] JoinOrgRoom error:', err);
          });
        }
      })
      .catch((err) => {
        console.warn('[SmartAssignHub] Connection error:', err);
        this.isConnected$.next(false);
      });

    this.hub.onclose(() => {
      this.isConnected$.next(false);
    });
  }

  disconnectHub(): void {
    if (this.hub) {
      this.hub.stop();
      this.isConnected$.next(false);
    }
  }
}

