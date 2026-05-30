// Auto-generated plan catalog for the "Explore your plans" screen.
// Each feature has: key, title, icon (emoji), tagline, description (paragraphs),
// optional bullets and an SVG illustration id used by the template.

export type PlanTier = 'growth' | 'pro' | 'enterprise';

export interface PlanFeature {
  key: string;
  title: string;
  icon: string;             // emoji used in the sidebar list
  illustration: string;     // svg id rendered in detail panel
  tagline: string;
  paragraphs: string[];
  bullets?: string[];
  learnMoreUrl?: string;
}

export interface PlanCategory {
  key: string;
  title: string;
  features: PlanFeature[];
}

export interface Plan {
  tier: PlanTier;
  label: string;
  priceTagline: string;
  accent: string;           // hex used for ribbons/active tab
  categories: PlanCategory[];
}

// ────────────────────────────────────────────────────────────────────────────
// Feature library (defined once, reused across tiers so Pro = Growth + extras,
// Enterprise = Pro + extras).
// ────────────────────────────────────────────────────────────────────────────
const F = {
  // ── Respond faster ─────────────────────────────────────────────────
  tickets: {
    key: 'tickets',
    title: 'Smart ticket management',
    icon: '🎫',
    illustration: 'tickets',
    tagline: 'Track every customer issue from open to resolved in one shared workspace.',
    paragraphs: [
      'Capture, prioritize and assign tickets from email, chat or a manual form. Each ticket carries its full conversation, attachments, voice notes and audit trail so any agent can pick it up without context loss.',
      'Inline status, priority and category dropdowns let teams move fast — no extra clicks, no spreadsheet workarounds.'
    ],
    bullets: ['Email-to-ticket', 'Voice note attachments', 'Inline edit', 'Soft delete & recovery']
  } as PlanFeature,

  emailIntegration: {
    key: 'email-integration',
    title: 'Email-to-ticket automation',
    icon: '📧',
    illustration: 'email',
    tagline: 'Turn your support inbox into a structured ticket queue automatically.',
    paragraphs: [
      'IMAP polls your support mailbox every 30 seconds and converts new mail into tickets. Replies sent from the app go out through SMTP with proper In-Reply-To and References headers, so conversations stay threaded in Gmail and Outlook.',
      'Customers keep emailing the way they always have — your team works in a real helpdesk.'
    ],
    bullets: ['Auto ticket creation', 'Threaded replies', 'PDF & file attachments', 'Cc / Bcc support']
  } as PlanFeature,

  cannedResponses: {
    key: 'canned-responses',
    title: 'Canned responses & templates',
    icon: '⚡',
    illustration: 'canned',
    tagline: 'Reply in seconds with reusable templates and dynamic variables.',
    paragraphs: [
      'Build a library of approved answers for the questions your team gets every day. Insert variables like agent name, customer name or ticket number so each reply still feels personal.'
    ],
    bullets: ['Variable placeholders', 'Per-group templates', 'One-click insert']
  } as PlanFeature,

  globalSearch: {
    key: 'search',
    title: 'Unified global search',
    icon: '🔎',
    illustration: 'search',
    tagline: 'Find any ticket, contact or KB article from the top bar in milliseconds.',
    paragraphs: [
      'A single search box covers tickets, contacts, users and knowledge base articles. Recently viewed items and recent queries surface first so agents reach the right record without remembering where it lives.'
    ]
  } as PlanFeature,

  customFields: {
    key: 'custom-fields',
    title: 'Custom fields for tickets',
    icon: '🧩',
    illustration: 'fields',
    tagline: 'Capture the data your workflow needs — without database changes.',
    paragraphs: [
      'Add text, dropdown, date or checkbox fields per organization. Mark them required, set default values and they flow into reports automatically.'
    ]
  } as PlanFeature,

  ticketMasters: {
    key: 'ticket-masters',
    title: 'Status, priority & category masters',
    icon: '🗂️',
    illustration: 'masters',
    tagline: 'Define your own ticket lifecycle and keep reporting clean.',
    paragraphs: [
      'Configure the exact statuses, priorities, categories and types your business uses. Standardised dropdowns prevent typos and keep dashboards accurate.'
    ]
  } as PlanFeature,

  emailNotifications: {
    key: 'email-notifications',
    title: 'Smart email notifications',
    icon: '🔔',
    illustration: 'notify',
    tagline: 'Right alert, right person, right cadence — no inbox noise.',
    paragraphs: [
      'Choose immediate, daily or weekly digests per notification type. Agents control their own preferences while admins set the org-wide rules.'
    ]
  } as PlanFeature,

  // ── Resolve faster ─────────────────────────────────────────────────
  knowledgeBase: {
    key: 'knowledge-base',
    title: 'Knowledge base',
    icon: '📚',
    illustration: 'kb',
    tagline: 'A self-service library that cuts repetitive tickets.',
    paragraphs: [
      'Publish articles, FAQs and solutions in a searchable library. Customers self-serve common questions, agents stop typing the same answer twice, and holiday announcements auto-post here.'
    ],
    bullets: ['Full-text search', 'Article versions', 'Customer portal', 'Auto holiday posts']
  } as PlanFeature,

  chat: {
    key: 'chat',
    title: 'Real-time team chat',
    icon: '💬',
    illustration: 'chat',
    tagline: 'Loop in a teammate without leaving the ticket.',
    paragraphs: [
      'WebSocket-powered chat with presence, missed-call tracking and direct voice/video calls. Bring SMEs into a customer issue in seconds instead of switching to a different tool.'
    ]
  } as PlanFeature,

  callLogs: {
    key: 'call-logs',
    title: 'Call logs & recordings',
    icon: '📞',
    illustration: 'calls',
    tagline: 'Every call answered, missed or made — recorded and searchable.',
    paragraphs: [
      'CDR-style call logs with duration, status (answered, missed, rejected) and recordings. Great for coaching, QA and compliance audits.'
    ]
  } as PlanFeature,

  whatsapp: {
    key: 'whatsapp',
    title: 'WhatsApp Business integration',
    icon: '🟢',
    illustration: 'whatsapp',
    tagline: 'Meet customers on the channel they actually use.',
    paragraphs: [
      'Send and receive WhatsApp messages, deliver template-based notifications and capture conversations as tickets — all from the same agent console.'
    ]
  } as PlanFeature,

  slack: {
    key: 'slack',
    title: 'Slack notifications',
    icon: '🪝',
    illustration: 'slack',
    tagline: 'Route the right alerts to the right Slack channel.',
    paragraphs: [
      'Push new-ticket, mention and SLA alerts to chosen Slack channels via webhooks. Selective routing keeps notifications useful instead of noisy.'
    ]
  } as PlanFeature,

  // ── Handle more tickets ────────────────────────────────────────────
  contacts: {
    key: 'contacts',
    title: 'Contacts & companies',
    icon: '👥',
    illustration: 'contacts',
    tagline: 'A 360° view of every customer you talk to.',
    paragraphs: [
      'Linked contacts, companies, phone numbers and full ticket history. Open a contact to see every conversation — no jumping between screens.'
    ]
  } as PlanFeature,

  todo: {
    key: 'todo',
    title: 'Personal to-do',
    icon: '✅',
    illustration: 'todo',
    tagline: 'Keep your day organised right next to your tickets.',
    paragraphs: [
      'A lightweight to-do list with priorities, due dates and an unread badge in the top bar. No more switching to another app to remember what to do next.'
    ]
  } as PlanFeature,

  calendar: {
    key: 'calendar',
    title: 'Team calendar',
    icon: '🗓️',
    illustration: 'calendar',
    tagline: 'See events, holidays and ticket deadlines on one calendar.',
    paragraphs: [
      'A unified month/week/day view that mixes events, birthdays, fixed and floating holidays — plus the tickets due that day. Timezone-aware so distributed teams see the right hours.'
    ]
  } as PlanFeature,

  holidaySetup: {
    key: 'holiday-setup',
    title: 'Holiday setup with PDF parsing',
    icon: '🎉',
    illustration: 'holidays',
    tagline: 'Upload your holiday list as a PDF — we extract the dates.',
    paragraphs: [
      'Drop in the official company holiday policy and the parser pulls out fixed and floating dates automatically. Holidays appear on the calendar and avoid getting routed work.'
    ]
  } as PlanFeature,

  customerPortal: {
    key: 'customer-portal',
    title: 'Customer self-service portal',
    icon: '🌐',
    illustration: 'portal',
    tagline: 'Let customers raise and track their own tickets.',
    paragraphs: [
      'A branded portal where customers create tickets, check status, read replies and browse the knowledge base — without needing an agent.'
    ]
  } as PlanFeature,

  recycleBin: {
    key: 'recycle-bin',
    title: 'Recycle bin & soft delete',
    icon: '🗑️',
    illustration: 'recycle',
    tagline: 'Recover from accidental deletes — for up to 30 days.',
    paragraphs: [
      'Deleted tickets sit in a recycle bin for a configurable retention window (days, months or years). A background worker purges expired items on schedule, keeping your data hygienic and recoverable.'
    ]
  } as PlanFeature,

  // ── Manage team better ─────────────────────────────────────────────
  agents: {
    key: 'agents',
    title: 'Agents & onboarding',
    icon: '🧑‍💼',
    illustration: 'agents',
    tagline: 'Invite, verify and onboard new agents in minutes.',
    paragraphs: [
      'Manage the full agent lifecycle — invite by email, verify mailboxes, track last-login and deactivate when needed. Bulk invites get a new team up and running fast.'
    ]
  } as PlanFeature,

  agentGroups: {
    key: 'agent-groups',
    title: 'Agent groups & routing',
    icon: '👨‍👩‍👧',
    illustration: 'groups',
    tagline: 'Segment your team by skill, region or product line.',
    paragraphs: [
      'Create groups like Billing, Tier-2 or VIP and route tickets to the right squad. Workload distributes evenly across the group.'
    ]
  } as PlanFeature,

  rolePermissions: {
    key: 'role-rights',
    title: 'Granular role & permissions',
    icon: '🔐',
    illustration: 'roles',
    tagline: '27 modules × 5 actions per role — least-privilege by design.',
    paragraphs: [
      'A clean matrix lets admins decide who can view, add, edit, delete or export each module. Permissions instantly hide unauthorized buttons and routes across the entire app.'
    ],
    bullets: ['View / Add / Edit / Delete / Export', 'Per-role overrides', 'Real-time UI gating']
  } as PlanFeature,

  organizationProfile: {
    key: 'organization-profile',
    title: 'Organization profile & branding',
    icon: '🏢',
    illustration: 'org',
    tagline: 'One place for workspace identity, SMTP/IMAP, timezone and integrations.',
    paragraphs: [
      'Configure your branding, mail server, default timezone, webhooks (Slack, Teams), Twilio and WhatsApp accounts — all from a single workspace settings page.'
    ]
  } as PlanFeature,

  dashboard: {
    key: 'dashboard',
    title: 'Real-time dashboard',
    icon: '📊',
    illustration: 'dashboard',
    tagline: 'KPIs at a glance — workload, trends, SLAs and CSAT.',
    paragraphs: [
      'Status counts, priority mix, agent workload, 7-day trend, response time and customer satisfaction in one view. Managers spot bottlenecks before they grow.'
    ]
  } as PlanFeature,

  reports: {
    key: 'reports',
    title: 'Advanced reports',
    icon: '📈',
    illustration: 'reports',
    tagline: 'Slice ticket data by date, agent, category, priority.',
    paragraphs: [
      'Build summary and performance reports with flexible filters. Export to CSV or Excel for finance, HR or executive reviews.'
    ]
  } as PlanFeature,

  analyticsHeatmap: {
    key: 'analytics-heatmap',
    title: 'Volume heatmap',
    icon: '🌡️',
    illustration: 'heatmap',
    tagline: 'See your peak support hours at a glance.',
    paragraphs: [
      'A 7-day or 30-day heatmap of ticket creation by hour and day. Use it to plan shifts and balance coverage.'
    ]
  } as PlanFeature,

  auditLog: {
    key: 'audit-log',
    title: 'Audit log',
    icon: '🛡️',
    illustration: 'audit',
    tagline: 'An immutable record of every sensitive action.',
    paragraphs: [
      'Permission changes, role assignments, password reveals and other compliance-relevant events are written to an immutable audit trail — ready for SOC 2 and GDPR reviews.'
    ]
  } as PlanFeature,

  aiInsights: {
    key: 'ai-insights',
    title: 'AI tide forecast & insights',
    icon: '🤖',
    illustration: 'ai',
    tagline: 'Predict ticket volume 7 days ahead and staff with confidence.',
    paragraphs: [
      'ML models trained on your historical ticket patterns forecast volume by hour and day, suggest categories and flag anomalies. Plan capacity proactively instead of firefighting.'
    ],
    bullets: ['Hourly + daily forecast', 'Category suggestion', 'Anomaly detection']
  } as PlanFeature,

  multiOrg: {
    key: 'multi-org',
    title: 'Multi-organization management',
    icon: '🏛️',
    illustration: 'multi-org',
    tagline: 'Run many helpdesks from a single super-admin console.',
    paragraphs: [
      'BPO and MSP teams can manage many client organizations from one console — provisioning, monitoring and intervening across tenants without juggling logins.'
    ]
  } as PlanFeature,

  leadsManagement: {
    key: 'leads',
    title: 'Leads & onboarding pipeline',
    icon: '🚀',
    illustration: 'leads',
    tagline: 'Capture sign-ups, approve them and convert to paying orgs.',
    paragraphs: [
      'A super-admin pipeline that captures landing-page leads, lets you approve or reject them and triggers the self-serve organization setup wizard automatically.'
    ]
  } as PlanFeature,
} as const;

// ────────────────────────────────────────────────────────────────────────────
// Tier composition. Growth → Pro adds features; Pro → Enterprise adds AI/admin.
// ────────────────────────────────────────────────────────────────────────────
const growthRespond: PlanFeature[] = [F.tickets, F.emailIntegration, F.cannedResponses, F.globalSearch];
const growthResolve: PlanFeature[] = [F.knowledgeBase, F.chat];
const growthHandle:  PlanFeature[] = [F.contacts, F.todo, F.calendar];
const growthReduce:  PlanFeature[] = [F.knowledgeBase];
const growthTeam:    PlanFeature[] = [F.agents, F.agentGroups, F.emailNotifications];

const proRespond: PlanFeature[] = [...growthRespond, F.customFields, F.ticketMasters];
const proResolve: PlanFeature[] = [...growthResolve, F.callLogs, F.whatsapp, F.slack];
const proHandle:  PlanFeature[] = [...growthHandle, F.holidaySetup, F.customerPortal, F.recycleBin];
const proReduce:  PlanFeature[] = [F.knowledgeBase, F.customerPortal];
const proTeam:    PlanFeature[] = [...growthTeam, F.rolePermissions, F.organizationProfile, F.dashboard, F.reports, F.analyticsHeatmap];

const entRespond: PlanFeature[] = [...proRespond];
const entResolve: PlanFeature[] = [...proResolve, F.aiInsights];
const entHandle:  PlanFeature[] = [...proHandle];
const entReduce:  PlanFeature[] = [...proReduce, F.aiInsights];
const entTeam:    PlanFeature[] = [...proTeam, F.auditLog, F.multiOrg, F.leadsManagement];

function cat(key: string, title: string, features: PlanFeature[]): PlanCategory {
  // De-duplicate while preserving order (some features appear in two categories).
  const seen = new Set<string>();
  const unique = features.filter(f => (seen.has(f.key) ? false : (seen.add(f.key), true)));
  return { key, title, features: unique };
}

export const PLANS: Plan[] = [
  {
    tier: 'growth',
    label: 'Growth',
    priceTagline: 'Everything a small support team needs to get started.',
    accent: '#2563eb',
    categories: [
      cat('respond', 'Respond faster',       growthRespond),
      cat('resolve', 'Resolve faster',       growthResolve),
      cat('handle',  'Handle more tickets',  growthHandle),
      cat('reduce',  'Reduce ticket volume', growthReduce),
      cat('manage',  'Manage team better',   growthTeam),
    ],
  },
  {
    tier: 'pro',
    label: 'Pro',
    priceTagline: 'For growing teams that need automation, reporting and governance.',
    accent: '#7c3aed',
    categories: [
      cat('respond', 'Respond faster',       proRespond),
      cat('resolve', 'Resolve faster',       proResolve),
      cat('handle',  'Handle more tickets',  proHandle),
      cat('reduce',  'Reduce ticket volume', proReduce),
      cat('manage',  'Manage team better',   proTeam),
    ],
  },
  {
    tier: 'enterprise',
    label: 'Enterprise',
    priceTagline: 'AI, multi-org and audit-grade controls for scaling support orgs.',
    accent: '#0f766e',
    categories: [
      cat('respond', 'Respond faster',       entRespond),
      cat('resolve', 'Resolve faster',       entResolve),
      cat('handle',  'Handle more tickets',  entHandle),
      cat('reduce',  'Reduce ticket volume', entReduce),
      cat('manage',  'Manage team better',   entTeam),
    ],
  },
];

export const CURRENT_PLAN_TIER: PlanTier = 'growth';
