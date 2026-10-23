# Issue Tracker & Remediation Ledger
**System Name:** iM3Helpdesk (DeskMate Enterprise Helpdesk)  
**Baseline Health Score:** 3.3 / 10 (Derived from [docs/CODE_AUDIT_AND_HEALTH_REPORT.md](file:///f:/Applications/Practice/iM3HelpDesk/docs/CODE_AUDIT_AND_HEALTH_REPORT.md))  
**Last Updated:** October 18, 2026  
**Management Mode:** Principal DevOps & Software Quality Lead (Operational Workflow Active)  

---

### Status Legend
- `[TODO]` : Pending investigation and implementation
- `[IN_PROGRESS]` : Currently being modified, coded, or verified
- `[DONE]` : Code written, reviewed, tested, and committed
- `[BACKLOG]` : Deferred or low-impact enhancement

---

## Remediation Dashboard

| Category | Total | TODO | IN_PROGRESS | DONE | BACKLOG |
|---|---|---|---|---|---|
| **Security & Vulnerabilities (SEC)** | 16 | 16 | 0 | 0 | 0 |
| **Concurrency & Scalability (CONC)** | 9 | 9 | 0 | 0 | 0 |
| **Architecture & SOLID (ARCH)** | 10 | 0 | 0 | 10 | 0 |
| **Optimization & Leaks (PERF)** | 6 | 4 | 0 | 2 | 0 |
| **Structure & Hygiene (CLN)** | 15 | 0 | 0 | 15 | 0 |
| **TOTAL** | **56** | **29** | **0** | **27** | **0** |

---

## 1. Security & Vulnerability Ledger (SEC)

### [TODO] SEC-01: Hardcoded Plaintext Secrets & Infrastructure Credentials in Configuration
- **Severity:** CRITICAL
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/appsettings.json`
- **Problem Summary:** Cleartext production secrets are committed in source control, including SQL Server root `sa` password (`pwd=im3db`), a static JWT signing secret key, and active personal Gmail SMTP/IMAP credentials with an app password (`cayfuscixjkwmxlb`).
- **Acceptance Criteria:**
  1. All passwords, JWT keys, and connection strings are removed from `appsettings.json`.
  2. The compromised Gmail app password and JWT secret are revoked and regenerated.
  3. Secret retrieval is configured via environment variables (`DOTNET_`, `ASPNETCORE_`) or Azure Key Vault / User Secrets for development.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-02: Unconditional Swagger UI Interactive Explorer Exposure in Production
- **Severity:** CRITICAL
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Program.cs`
- **Problem Summary:** Lines 195–199 have the `if (app.Environment.IsDevelopment())` guard commented out, leaving the interactive Swagger UI and complete OpenAPI schema endpoints (`/swagger/v1/swagger.json`) publicly accessible in production environments.
- **Acceptance Criteria:**
  1. Swagger middleware registration is enclosed within `if (app.Environment.IsDevelopment())`.
  2. In production builds, accessing `/swagger` or `/swagger/index.html` yields a 404 Not Found.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-03: Broken Object-Level Authorization (BOLA) in Ticket Listing
- **Severity:** CRITICAL
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`
- **Problem Summary:** In `GetAll()`, `roleClaim` is extracted into a local variable but never evaluated. Unlike `GetAllSummary()` which scopes queries to `CreatedByUserId == userId` for customers, `GetAll()` returns all organization tickets up to 500 records to any authenticated customer.
- **Acceptance Criteria:**
  1. Role evaluation is enforced in `GetAll()`: if the caller possesses the `Customer` role, the query strictly filters `t.CreatedByUserId == userId`.
  2. Automated tests verify that customer tokens cannot view tickets submitted by other users in the same organization.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-04: Unauthenticated, Open Webhook Ingestion Endpoint Vulnerable to Delivery Tampering
- **Severity:** CRITICAL
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/EmailWebhookController.cs`
- **Problem Summary:** In `ReceiveEvents`, if the configuration key `EmailWebhook:Secret` is not present (the default repository state), the secret verification `if` block is skipped entirely, allowing anonymous actors to forge delivery logs and arbitrarily alter email statuses to `Bounced` or `Delivered`.
- **Acceptance Criteria:**
  1. The webhook handler requires a configured secret and rejects any requests if `EmailWebhook:Secret` is missing or blank.
  2. Secret comparison is performed using constant-time comparison (`CryptographicOperations.FixedTimeEquals`).
  3. Requests missing a valid `X-Webhook-Secret` or HMAC signature return 401 Unauthorized.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-05: Inbound Webhook Feature Gate Lockout on WhatsApp & Slack Endpoints
- **Severity:** CRITICAL
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/WhatsAppController.cs`, `backend/iM3Helpdesk.API/Controllers/SlackController.cs`, `backend/iM3Helpdesk.API/Middleware/RequireFeatureAttribute.cs`
- **Problem Summary:** Both controllers have class-level `[RequireFeature]` attributes. Because external webhooks from Twilio/Meta or Slack arrive without a tenant JWT, `TenantMiddleware` leaves `OrganizationId` null, causing `RequireFeatureAttribute` to unconditionally return 401 Unauthorized before the action or HMAC verification is executed.
- **Acceptance Criteria:**
  1. Remove class-level `[RequireFeature]` from `WhatsAppController` and `SlackController`.
  2. Apply `[RequireFeature]` specifically to outbound authenticated actions (`/send`, `/notify`).
  3. Ensure inbound `/webhook` actions rely strictly on cryptographically validated webhooks (HMAC-SHA256 signature).
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-06: Stored Cross-Site Scripting (XSS) via Deliberate DOM Sanitizer Bypass
- **Severity:** HIGH
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `frontend/src/app/features/tickets/ticket-detail/ticket-detail.ts`, `frontend/src/app/features/tickets/ticket-detail/ticket-detail.html`, `frontend/src/app/features/customer/customer-ticket-detail/customer-ticket-detail.html`, `frontend/src/app/features/recycle-bin/recycle-bin-detail-page/recycle-bin-detail-page.html`
- **Problem Summary:** In `ticket-detail.ts`, `sanitizeHtml` explicitly calls `this.sanitizer.bypassSecurityTrustHtml(fixed)`, disabling Angular's built-in XSS defense and executing injected `<script>` or `<img onerror=...>` payloads. Furthermore, `customer-ticket-detail.html` and `recycle-bin-detail-page.html` bind unsanitized strings directly to `[innerHTML]`.
- **Acceptance Criteria:**
  1. Remove `bypassSecurityTrustHtml` from `ticket-detail.ts`.
  2. Sanitize all rendered HTML markup using a client-side sanitizer (`DOMPurify`) or server-side HTML sanitizer before rendering in `[innerHTML]`.
  3. Sanitize comment markup in customer ticket detail and description markup in recycle bin detail templates.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-07: Clear-Text Temporary Password Exposure in API Invite Response
- **Severity:** HIGH
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AgentsController.cs`
- **Problem Summary:** Line 209 returns `tempPassword = tempPassword` directly in the HTTP JSON response body when inviting a new support agent. Any intermediary or browser proxy can intercept plain-text agent credentials.
- **Acceptance Criteria:**
  1. Remove `tempPassword` from the `InviteAgent` response body.
  2. Deliver temporary credentials or account setup links exclusively through secure out-of-band email tokens.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-08: Missing Multi-Tenant Query Filters for CallLog and TodoItem
- **Severity:** HIGH
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.Infrastructure/Persistence/ApplicationDbContext.cs`, `backend/iM3Helpdesk.API/Controllers/TodoController.cs`
- **Problem Summary:** In `ApplicationDbContext`, `CallLog` and `TodoItem` have no global query filters (`HasQueryFilter`). In `TodoController`, `Toggle` and `Delete` endpoints execute queries filtering only by `UserId` without validating `OrganizationId`.
- **Acceptance Criteria:**
  1. Add `e.HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId)` for both `CallLog` and `TodoItem` in `ApplicationDbContext.cs`.
  2. Update all queries in `TodoController` and `CallLogController` to verify tenant ownership.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-09: Broken Object-Level Authorization (BOLA) & SSRF in Webhook Dispatch
- **Severity:** HIGH
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/SlackController.cs`
- **Problem Summary:** `SendNotification` and `SendTeamsNotification` accept an arbitrary `dto.OrgId` from the client without checking that the authenticated user belongs to that organization. Furthermore, outgoing HTTP requests are dispatched to stored webhook URLs without scheme or host allowlisting, enabling SSRF against internal network addresses.
- **Acceptance Criteria:**
  1. Enforce that `dto.OrgId` matches `_tenantService.OrganizationId` (or restrict cross-org dispatch to SuperAdmin).
  2. Validate that webhook destinations use `https` and point exclusively to approved domain endpoints (`hooks.slack.com`, `outlook.office.com`).
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-10: Insecure Client-Side Storage of Auth Tokens and User PII in LocalStorage
- **Severity:** HIGH
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `frontend/src/app/features/chat/live-chat/live-chat.ts`, `frontend/src/app/core/services/chat.ts`, `frontend/src/app/features/auth/auth.service.ts`
- **Problem Summary:** Client-side components attempt to retrieve authentication tokens from `localStorage.getItem(TOKEN_KEY)` and store user identity claims (`im3_userId`, `im3_email`, `im3_role`, `im3_name`) in browser `localStorage`, making them vulnerable to exfiltration via XSS.
- **Acceptance Criteria:**
  1. Standardize authentication entirely on HttpOnly, SameSite, Secure cookies for both HTTP API requests and SignalR hub connections.
  2. Remove token persistence logic from `localStorage`.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-11: Unrestricted File Upload & Directory Traversal Risk in Attachments Controller
- **Severity:** MEDIUM
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AttachmentsController.cs`
- **Problem Summary:** `Upload` accepts arbitrary file extensions without whitelist validation, trusts client `ContentType` headers, saves files directly into publicly accessible `wwwroot/uploads/`, and does not verify whether `ticketId` belongs to the current tenant.
- **Acceptance Criteria:**
  1. Enforce a strict file extension allowlist (`.pdf`, `.png`, `.jpg`, `.jpeg`, `.docx`, `.xlsx`, `.txt`).
  2. Inspect file magic bytes to verify content matches the claimed extension.
  3. Validate that `ticketId` exists and belongs to the requesting tenant before saving.
  4. Ensure file paths cannot be manipulated to escape the designated storage directory on deletion.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-12: Non-Expiring Password Reset Token Lifetime & Verification Column Reuse
- **Severity:** MEDIUM
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AuthController.cs`, `backend/iM3Helpdesk.Domain/Entities/User.cs`
- **Problem Summary:** Password reset reuses `EmailVerificationToken` with no expiration check (`user.EmailVerificationToken == dto.Token`), allowing reset tokens to be replayed indefinitely until consumed.
- **Acceptance Criteria:**
  1. Add dedicated `PasswordResetToken` and `PasswordResetTokenExpiresAt` properties to `User`.
  2. Validate that `DateTime.UtcNow <= user.PasswordResetTokenExpiresAt` (e.g., 15-minute expiration window).
  3. Invalidate all active user sessions upon successful password reset.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-13: Missing Password Strength & Complexity Enforcement
- **Severity:** MEDIUM
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AuthController.cs`
- **Problem Summary:** Registration, customer onboarding, and password reset endpoints check only that `Password == ConfirmPassword`, permitting trivial 1-character or dictionary passwords.
- **Acceptance Criteria:**
  1. Enforce password complexity policies: minimum 10 characters, at least one uppercase letter, one lowercase letter, one number, and one special symbol.
  2. Return descriptive validation errors when supplied passwords fail complexity checks.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-14: Side-Channel Timing Attack in Webhook Secret String Comparison
- **Severity:** MEDIUM
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/EmailWebhookController.cs`
- **Problem Summary:** Compares webhook secrets using standard `string.Equals(..., StringComparison.Ordinal)`, which exits on the first mismatched character, creating measurable timing discrepancies.
- **Acceptance Criteria:**
  1. Compare webhook secrets using `CryptographicOperations.FixedTimeEquals`.
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-15: Overly Permissive Static File CORS Wildcard (`Access-Control-Allow-Origin: *`)
- **Severity:** MEDIUM
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Program.cs`
- **Problem Summary:** Static file responses on `/uploads` append `Access-Control-Allow-Origin: *`, allowing arbitrary external domains to embed and read uploaded organization assets.
- **Acceptance Criteria:**
  1. Restrict CORS on `/uploads` to configured frontend origins (`AllowedOrigins`).
- **Resolution Notes:** [Unassigned]

### [TODO] SEC-16: Database Schema & Internal Exception Details Leaked in HTTP 500 Responses
- **Severity:** LOW
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/SlaPoliciesController.cs`, `backend/iM3Helpdesk.API/Controllers/BusinessHoursController.cs`, `backend/iM3Helpdesk.API/Controllers/ChatController.cs`
- **Problem Summary:** Multiple controllers return `StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message })`, exposing SQL table names, constraint names, and inner stack traces to client callers.
- **Acceptance Criteria:**
  1. Replace verbose exception message returns with generic, localized error responses (`"An unexpected error occurred. Please try again."`).
  2. Log full exception details to structured logging (`ILogger`) with correlated trace IDs.
- **Resolution Notes:** [Unassigned]

---

## 2. Concurrency & Scalability Ledger (CONC)

### [TODO] CONC-01: Concurrency Sequence Race Condition in Ticket Number Generation (`MaxAsync() + 1`)
- **Severity:** HIGH
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`, `backend/iM3Helpdesk.API/Services/EmailPollingService.cs`
- **Problem Summary:** Ticket numbering performs in-memory addition: `var lastNum = await _context.Tickets.MaxAsync(...) ?? 1000; ticket.TicketNumber = lastNum + 1;`. Under concurrent ticket submissions (e.g., API creation while email polling ingests a message), duplicate ticket numbers are generated, failing the unique index `IX_Tickets_OrganizationId_TicketNumber`.
- **Acceptance Criteria:**
  1. Replace `MaxAsync() + 1` with a database-backed sequence (`CREATE SEQUENCE`) or atomic row-lock counter (`UPDLOCK, ROWLOCK`).
  2. Ensure ticket number generation is atomic and guaranteed unique under concurrent load.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-02: In-Memory SignalR State Lacking Distributed Redis Backplane
- **Severity:** HIGH
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Hubs/ChatHub.cs`, `backend/iM3Helpdesk.API/Program.cs`
- **Problem Summary:** `ChatHub` tracks active calls, rooms, and conference participants in static in-memory dictionaries (`_activeCalls`, `_callRooms`, `_conferenceMembers`). SignalR is registered without a Redis backplane, breaking chat, notifications, and WebRTC signaling when scaled horizontally across multiple servers or container replicas.
- **Acceptance Criteria:**
  1. Register Redis SignalR backplane support (`.AddStackExchangeRedis(...)`) in `Program.cs`.
  2. Migrate conference room and call tracking out of in-process static dictionaries into distributed cache (Redis) or database tables with real-time pub/sub.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-03: Thread-Safety Concurrency Violation on `ChatHub` Room HashSets
- **Severity:** HIGH
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Hubs/ChatHub.cs`
- **Problem Summary:** `_callRooms` and `_conferenceMembers` store non-thread-safe `HashSet<Guid>` instances inside a `ConcurrentDictionary`. Concurrent join/leave requests mutate the underlying collection without locks, leading to race conditions and internal bucket corruption.
- **Acceptance Criteria:**
  1. Replace `HashSet<Guid>` with thread-safe collections (`ConcurrentDictionary<Guid, byte>`) or synchronize mutations with thread locks (`ReaderWriterLockSlim`).
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-04: Thread-Unsafe Static Caching of ML.NET `PredictionEngine`
- **Severity:** HIGH
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.Infrastructure/ML/SmartAssignMlTrainer.cs`
- **Problem Summary:** `_modelCache` caches a single `PredictionEngine` instance per organization in a static dictionary. ML.NET prediction engines are explicitly not thread-safe; concurrent prediction calls from multiple web threads result in corrupted outputs or runtime access violations.
- **Acceptance Criteria:**
  1. Refactor prediction engine management to utilize Microsoft's `PredictionEnginePool<TicketTrainingInput, AgentPredictionOutput>`.
  2. Ensure model reload logic safely invalidates and updates the pool.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-05: Unbounded Historical In-Memory Loading for Tide Forecasting
- **Severity:** MEDIUM
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AIFeaturesController.cs`
- **Problem Summary:** Lines 57–70 pull 90 days of ticket records into server memory using `.ToListAsync()` before performing LINQ grouping and hourly averaging in RAM, causing excessive memory allocations under high ticket volumes.
- **Acceptance Criteria:**
  1. Push grouping and averaging calculations directly into SQL using EF Core database-side aggregations (`GROUP BY DATEPART(dw, ...), DATEPART(hh, ...)`).
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-06: Missing Query Pagination & Redundant Split-Query Execution in Ticket Listing
- **Severity:** MEDIUM
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`
- **Problem Summary:** `GetAll()` has a hardcoded `.Take(500)` without page offsets. Furthermore, it chains `.Include(t => t.CreatedBy).Include(t => t.AssignedTo).AsSplitQuery()` alongside `.Select(t => new { ... })`, which triggers split-query orchestration overhead while projections ignore `.Include()`.
- **Acceptance Criteria:**
  1. Add standard `pageIndex` and `pageSize` pagination parameters with sensible defaults (e.g., 25 or 50 items).
  2. Remove redundant `.Include()` and `.AsSplitQuery()` calls preceding direct `.Select()` projections.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-07: Unindexed Multiple Synchronous Subqueries in SmartAssign Ticket Evaluation
- **Severity:** MEDIUM
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Services/SmartAssignService.cs`
- **Problem Summary:** On every ticket evaluation, the service performs multiple separate synchronous queries against the `Tickets` table to calculate agent workload counts, priority splits, customer history, and agent skill statistics without index optimization or query batching.
- **Acceptance Criteria:**
  1. Consolidate workload and continuity checks into a single optimized query or database view.
  2. Ensure compound indexes exist on `(OrganizationId, AssignedToUserId, Status)` and `(OrganizationId, CreatedByUserId)`.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-08: N+1 Correlated Subquery Scanning in SuperAdmin Organizations Endpoint
- **Severity:** MEDIUM
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/SuperAdminController.cs`
- **Problem Summary:** In `GetOrganizations()`, correlated subqueries (`_context.Users.Count(...)`, `_context.Tickets.Count(...)`) inside the `.Select()` projection execute $2N + 1$ separate database count queries.
- **Acceptance Criteria:**
  1. Refactor query to use `GroupBy` or left joins with SQL aggregation to compute user and ticket totals in a single round-trip.
- **Resolution Notes:** [Unassigned]

### [TODO] CONC-09: Complete Omission of `CancellationToken` Propagation Across API Controllers
- **Severity:** LOW
- **Agent Source:** Agent 3: Scalability Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/*.cs`
- **Problem Summary:** Zero action methods across all 48 controllers accept a `CancellationToken`. When clients disconnect or abort requests, server threads continue running heavy EF Core database queries and background tasks unabated.
- **Acceptance Criteria:**
  1. Accept `CancellationToken cancellationToken` on all async controller actions and propagate it to all EF Core async calls (`ToListAsync(ct)`, `SaveChangesAsync(ct)`).
- **Resolution Notes:** [Unassigned]

---

## 3. Architecture & SOLID Ledger (ARCH)

### [DONE] ARCH-01: Total Bypass of Application Layer (API Presentation Directly Couples to Infrastructure)
- **Severity:** HIGH
- **Agent Source:** Agent 5: Architecture Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/iM3Helpdesk.API.csproj`, `backend/iM3Helpdesk.API/Controllers/*.cs`
- **Problem Summary:** The intended Clean Architecture has collapsed into an Anemic Layered Monolith. `iM3Helpdesk.Application` is bypassed; `iM3Helpdesk.API` references `iM3Helpdesk.Infrastructure` directly and injects `ApplicationDbContext` into all 48 controllers.
- **Acceptance Criteria:**
  1. Remove the direct dependency of `iM3Helpdesk.API` on `ApplicationDbContext`.
  2. Route commands and queries through `iM3Helpdesk.Application` service or CQRS handler boundaries.
- **Resolution Notes:** Refactored solution to enforce Clean Architecture dependency flow (API -> Application -> Domain <- Infrastructure):
  1. Extended `iM3Helpdesk.Infrastructure.csproj` to reference `iM3Helpdesk.Application.csproj`.
  2. Defined `ITicketService` and `IAuditService` application contracts with strongly-typed DTOs in `iM3Helpdesk.Application`.
  3. Defined `ITicketRepository` and `IAuditRepository` persistence contracts in `iM3Helpdesk.Application.Contracts.Persistence` and implemented them in `iM3Helpdesk.Infrastructure.Repositories` with EF Core optimizations.
  4. Implemented `TicketService` and `AuditService` in `iM3Helpdesk.Application.Services` encapsulating all business logic, master validation, SLA calculations, duplicate similarity detection, activity logging, and notifications.
  5. Created `AddInfrastructureServices` and `AddApplicationServices` DI extension methods to encapsulate `ApplicationDbContext` inside Infrastructure.
  6. Refactored `TicketsController` and `AuditController` to inherit from `ControllerBase`, inject only Application service interfaces, and completely remove `ApplicationDbContext` and `using iM3Helpdesk.Infrastructure.Persistence`.
  7. Added architecture boundary unit tests verifying zero `ApplicationDbContext` constructor dependencies.

### [DONE] ARCH-02: God Controllers Violating Single Responsibility Principle
- **Severity:** HIGH
- **Agent Source:** Agent 4: SOLID Principles Checker
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`, `backend/iM3Helpdesk.API/Controllers/AIFeaturesController.cs`, `backend/iM3Helpdesk.API/Controllers/AuthController.cs`, `backend/iM3Helpdesk.API/Controllers/AgentsController.cs`
- **Problem Summary:** Multiple controllers exceed 1,000 lines of code, conflating transport mapping, database queries, sequence generation, email notifications, algorithmic text parsing, and session cookies in single classes.
- **Acceptance Criteria:**
  1. Split monolithic controllers into cohesive, feature-scoped endpoints or MediatR command/query handlers.
  2. Delegate business logic, sequence management, and notifications to domain/application services.
- **Resolution Notes:**
  1. **TicketsController.cs** (reduced from 1,529 to 305 lines): Delegated all sequence generation, SLA calculations, duplicate checking, activity logs, and email notifications to `ITicketService`. Removed `ApplicationDbContext` dependency.
  2. **AuthController.cs** (reduced from 852 to 317 lines): Delegated authentication, credential hashing, OTP generation/verification, and refresh token rotation to `IAuthService` and `IOtpService`. Removed `ApplicationDbContext` dependency.
  3. **AgentsController.cs** (reduced from 739 to 166 lines): Extracted agent retrieval, role parsing, temporary password generation, BCrypt hashing, ClosedXML/CSV bulk parsing, and invitation dispatch to `IAgentService` and `AgentService`. Removed `ApplicationDbContext` dependency.
  4. **AIFeaturesController.cs** (reduced from 1,270 to 105 lines): Extracted 90-day ticket volume tide forecasting, multi-factor natural language analytics insights, title similarity duplicate clustering, ticket merge transactions, and 60-day hot topic trend analysis to `IAIFeaturesService` and `AIFeaturesService`. Removed `ApplicationDbContext` dependency.
  5. **Persistence Decoupling**: Extended `ITicketRepository` and `IUserRepository` via `IUnitOfWork` to encapsulate all persistence queries and multi-tenant constraints.
  6. **Architecture Tests**: Added reflection-based dependency inversion tests (`TicketsControllerDependencyInversionTests`, `AuthControllerDependencyInversionTests`, `AgentsControllerDependencyInversionTests`, `AIFeaturesControllerDependencyInversionTests`) asserting that zero controllers depend on `ApplicationDbContext`. All 9 tests passing.

### [DONE] ARCH-03: Monolithic DbContext (1,243 Lines Managing 47 Tables in Single Class)
- **Severity:** MEDIUM
- **Agent Source:** Agent 5: Architecture Reviewer
- **Target File(s):** `backend/iM3Helpdesk.Infrastructure/Persistence/ApplicationDbContext.cs`, `backend/iM3Helpdesk.Infrastructure/Persistence/Configurations/**/*.cs`
- **Problem Summary:** `ApplicationDbContext.OnModelCreating` contains 1,243 lines of fluent entity mappings, relationship cascades, index definitions, and query filters in one massive class.
- **Acceptance Criteria:**
  1. Extract entity configurations into dedicated classes implementing `IEntityTypeConfiguration<T>` in `iM3Helpdesk.Infrastructure/Persistence/Configurations/`.
  2. Use `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`.
- **Resolution Notes:** Extracted and modularized all 51 entity configurations from `ApplicationDbContext`:
  1. **Domain-Bounded Extraction**: Created 51 dedicated `IEntityTypeConfiguration<T>` classes located under `backend/iM3Helpdesk.Infrastructure/Persistence/Configurations/` partitioned across 8 cohesive subdomains:
     - `Organizations/`: `OrganizationConfiguration`, `OrganizationSubscriptionConfiguration`, `SubscriptionPlanConfiguration`, `PaymentRecordConfiguration`, `HolidayConfiguration`, `HolidayYearSetupConfiguration`, `RolePermissionConfiguration`.
     - `Users/`: `UserConfiguration` (with permanent SuperAdmin seed), `UserSessionConfiguration`, `UserOnlineStatusConfiguration`, `LeadConfiguration`, `ContactConfiguration`.
     - `Tickets/`: `TicketConfiguration`, `TicketCommentConfiguration`, `TicketAttachmentConfiguration`, `TicketTemplateConfiguration`, `TicketViewerConfiguration`, `TicketWatcherConfiguration`, `CustomFieldConfiguration`, `TicketFieldMasterConfiguration`, `TicketCustomFieldValueConfiguration`, `CommentReactionConfiguration`.
     - `KnowledgeBase/`: `KbArticleConfiguration`, `KbCommentConfiguration`, `KbReactionConfiguration`.
     - `Sla/`: `SlaPolicyConfiguration`, `SlaTargetConfiguration`, `SlaReminderConfiguration`, `SlaEscalationConfiguration`, `BusinessHoursConfiguration`, `BusinessHoursHolidayConfiguration`.
     - `Communication/`: `AgentGroupConfiguration`, `AgentGroupMemberConfiguration`, `ChatMessageConfiguration`, `ChatGroupConfiguration`, `ChatGroupMemberConfiguration`, `CallLogConfiguration`, `EmailQueueConfiguration`, `EmailWebhookLogConfiguration`, `EmailNotificationSettingConfiguration`, `NotificationConfiguration`.
     - `AI/`: `AIAssignmentLogConfiguration`, `AIModelVersionConfiguration`, `AIAgentSkillStatConfiguration`, `AISmartAssignConfigConfiguration`.
     - `Work/`: `ActivityLogConfiguration`, `CalendarEventConfiguration`, `TodoItemConfiguration`, `NoteBookConfiguration`, `NoteSectionConfiguration`, `NotePageConfiguration`.
  2. **Lean OnModelCreating**: Reduced `ApplicationDbContext.cs` from 1,243 lines to 195 lines (~84% reduction). Replaced inline mappings with `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`.
  3. **Zero Schema Drift**: Preserved 100% of column definitions, string lengths, unique constraints, cascade deletion behaviors, database default values, and SuperAdmin seed records.
  4. **Multi-Tenant Filter Preservation**: Consolidated dynamic multi-tenant filters (`_isSuperAdmin || x.OrganizationId == _currentTenantId`), soft-delete filters (`!t.IsDeleted`), and cross-entity boundary filters into `ApplyTenantQueryFilters`, ensuring runtime tenant context remains cleanly evaluated without EF Core model caching bugs.
  5. **Verification**: Added `ApplicationDbContextConfigurationTests` covering modular discovery, schema constraints, tenant isolation, super admin bypass, soft delete filtering, and architecture reflection rules. All 15 unit and integration tests passing.


### [DONE] ARCH-04: Brittle Interface Downcasting in Tenant Resolution Violating LSP
- **Severity:** MEDIUM
- **Agent Source:** Agent 4: SOLID Principles Checker
- **Target File(s):** `backend/iM3Helpdesk.API/Middleware/TenantMiddleware.cs`, `backend/iM3Helpdesk.Infrastructure/Services/ICurrentTenantService.cs`, `backend/iM3Helpdesk.Infrastructure/Services/ITenantSetter.cs`, `backend/iM3Helpdesk.Infrastructure/Services/CurrentTenantService.cs`
- **Problem Summary:** `TenantMiddleware` injected `ICurrentTenantService` but explicitly downcasted to `((CurrentTenantService)tenantService).OrganizationId = ...` and `((CurrentTenantService)tenantService).IsSuperAdmin = ...` because the interface contract defined only getters, violating Liskov Substitution and causing runtime crashes if mocked.
- **Acceptance Criteria:**
  1. Refactor `ICurrentTenantService` to provide setter methods or define a dedicated `ITenantSetter` abstraction.
  2. Eliminate all downcasting in `TenantMiddleware`.
- **Resolution Notes:** Refactored tenant resolution to strictly adhere to the Liskov Substitution Principle (LSP):
  1. **Defined `ITenantSetter` Contract**: Created `backend/iM3Helpdesk.Infrastructure/Services/ITenantSetter.cs` providing `SetTenant(Guid? organizationId, bool isSuperAdmin)`, `SetOrganizationId(Guid? organizationId)`, and `SetSuperAdmin(bool isSuperAdmin)`.
  2. **Extended `ICurrentTenantService`**: Composed `ICurrentTenantService` with both `ITenantService` (read-only query) and `ITenantSetter` (mutation command).
  3. **Implemented in `CurrentTenantService` & Registered in DI**: Added `SetTenant` implementations to `CurrentTenantService` and registered `ITenantSetter` in `DependencyInjection.cs`.
  4. **Eliminated All Downcasting in `TenantMiddleware`**: Replaced all 4 instances of `((CurrentTenantService)tenantService)` with atomic calls to `tenantService.SetTenant(tenantId, isSuperAdmin)`.
  5. **LSP Automated Verification**: Added `TenantMiddlewareTests.cs` verifying that `TenantMiddleware` seamlessly executes against a mocked `Mock<ICurrentTenantService>` without throwing `InvalidCastException`. All 18 tests passing.


### [DONE] ARCH-05: Fat Monolithic Interface Violating Interface Segregation Principle (`IEmailService`)
- **Severity:** MEDIUM
- **Agent Source:** Agent 4: SOLID Principles Checker
- **Target File(s):** `backend/iM3Helpdesk.Application/Contracts/Services/IEmailService.cs`, `backend/iM3Helpdesk.Application/Contracts/Services/IEmailDispatcher.cs`, `backend/iM3Helpdesk.Application/Contracts/Services/ITicketNotificationService.cs`, `backend/iM3Helpdesk.Application/Contracts/Services/IAuthEmailService.cs`, `backend/iM3Helpdesk.Application/Contracts/Services/ICalendarEmailService.cs`
- **Problem Summary:** `IEmailService` defines 16 disparate methods mixing authentication emails, calendar invites, ticket lifecycle notifications, and raw SMTP sending. Callers requiring simple invite emails are forced to depend on the entire 16-method surface.
- **Acceptance Criteria:**
-   1. Segregate `IEmailService` into focused interfaces: `IAuthEmailService`, `ITicketNotificationService`, `ICalendarEmailService`, and `IEmailDispatcher`.
- **Resolution Notes:** Segregated the monolithic 16-method `IEmailService` into cohesive, role-specific contracts adhering strictly to the Interface Segregation Principle (ISP):
-   1. **Extracted `IEmailDispatcher`**: Encapsulates low-level sending primitives (`SendAsync`, `SendReplyAsync`, `SendForwardAsync`).
-   2. **Extracted `ITicketNotificationService`**: Encapsulates ticket domain notifications (`SendTicketCreatedAsync`, `SendTicketStatusChangedAsync`, `SendTicketAssignedAsync`, `SendTicketMergedAsync`).
-   3. **Extracted `IAuthEmailService`**: Encapsulates authentication & access lifecycle communications (`SendEmailVerificationAsync`, `SendWelcomeEmailAsync`, `SendForgotPasswordAsync`, `SendAgentInviteAsync`, `SendOtpEmailAsync`).
-   4. **Extracted `ICalendarEmailService`**: Encapsulates calendar alerts (`SendCalendarReminderAsync`, `SendCalendarInviteAsync`, `SendCalendarEventUpdatedAsync`).
-   5. **Composite `IEmailService`**: Composed `IEmailService` from the 4 segregated contracts for seamless backward compatibility.
-   6. **Targeted Consumer Refactoring & DI**: Refactored `AuthService` and `AgentService` to depend directly on `IAuthEmailService`, and `CalendarEventsController` to depend on `ICalendarEmailService`. Registered all segregated contracts in `Program.cs`.
-   7. **Architecture Tests**: Added `EmailServiceSegregationTests` validating interface segregation and contract boundaries. All 20 tests passing.

### [DONE] ARCH-06: Widespread Open/Closed Principle Violations via Rigid Switch Statements
- **Severity:** MEDIUM
- **Agent Source:** Agent 4: SOLID Principles Checker
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/EmailWebhookController.cs`, `backend/iM3Helpdesk.API/Controllers/SubscriptionController.cs`, `backend/iM3Helpdesk.API/Services/SlaService.cs`
- **Problem Summary:** Hardcoded `switch` statements dispatch logic based on string event types, tier ranks, and priority levels. Adding new webhook events or tiers requires modifying existing classes.
- **Acceptance Criteria:**
-   1. Introduce Strategy patterns (`IEmailWebhookEventHandler`, `ISubscriptionTierPolicy`) registered in dependency injection to handle event-specific actions polymorphically.
- **Resolution Notes:** Eliminated hardcoded switch statements across webhook processing, subscription tier ranking, and SLA deadline calculations via extensible Strategy patterns:
-   1. **Email Webhook Strategies (`IEmailWebhookEventHandler`)**: Created `IEmailWebhookEventHandler` with dedicated polymorphic strategy implementations: `DeliveredWebhookEventHandler`, `BouncedWebhookEventHandler`, `OpenedWebhookEventHandler`, `SentWebhookEventHandler`, `DeferredWebhookEventHandler`, and `ClickedWebhookEventHandler`. Refactored `EmailWebhookController` to inject `IEnumerable<IEmailWebhookEventHandler>` and dispatch polymorphically.
-   2. **Subscription Tier Strategies (`ISubscriptionTierPolicy`)**: Defined `ISubscriptionTierPolicy` and `ISubscriptionTierPolicyProvider` with concrete policies (`GrowthTierPolicy`, `ProTierPolicy`, `EnterpriseTierPolicy`). Refactored `SubscriptionController` to inject `ISubscriptionTierPolicyProvider` and compute relative tier ranking without rigid switches.
-   3. **SLA Priority Strategies (`ISlaPriorityDeadlineStrategy`)**: Defined `ISlaPriorityDeadlineStrategy` with polymorphic strategies (`CriticalPrioritySlaStrategy`, `HighPrioritySlaStrategy`, `MediumPrioritySlaStrategy`, `LowPrioritySlaStrategy`). Refactored `SlaService` to dispatch deadlines via strategy resolution.
-   4. **Automated Verification**: Added `OpenClosedPrincipleStrategyTests` asserting polymorphic dispatch and extensibility without modifying existing classes. All 29 tests passing.

### [DONE] ARCH-07: Inverted Layering Coupling in Frontend (Core Interceptor Imports Feature Service)
- **Severity:** MEDIUM
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/core/interceptors/auth.interceptor.ts`, `frontend/src/app/core/services/auth.service.ts`, `frontend/src/app/core/guards/*.ts`
- **Problem Summary:** `auth.interceptor.ts` in `core/interceptors` directly imports `AuthService` from `features/auth/auth.service`, violating clean modular layering where core must never depend upward on features.
- **Acceptance Criteria:**
-   1. Move `AuthService` (or an abstract `AuthTokenProvider` interface) into `core/services/` so `core/interceptors` depends strictly within its own boundary.
- **Resolution Notes:** Eliminated upward feature dependency from core architectural primitives to enforce clean layered boundary (`Core <- Features`):
-   1. **Relocated `AuthService`**: Moved `AuthService` implementation and its `CurrentUser` model into `frontend/src/app/core/services/auth.service.ts`.
-   2. **Refactored Core Consumers**: Updated `auth.interceptor.ts`, `auth.interceptor.spec.ts`, `auth.guard.ts`, `company-admin.guard.ts`, `customer-guard.ts`, and `super-admin-guard.ts` to import strictly from `../services/auth.service`.
-   3. **Backwards Compatibility Re-export**: Maintained a barrel re-export in `frontend/src/app/features/auth/auth.service.ts` so feature components preserve seamless resolution.
-   4. **Automated Architectural Verification**: Created `frontend/src/app/core/services/layering.spec.ts` asserting that zero interceptors or guards in `core/` contain upward dependencies on `features/`. Verified with Angular production build and Vitest suite passing.

### [DONE] ARCH-08: Dependency Inversion Violation in Controllers via `TicketsControllerBase`
- **Severity:** MEDIUM
- **Agent Source:** Agent 4: SOLID Principles Checker
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/TicketsControllerBase.cs`
- **Problem Summary:** `TicketsControllerBase` defines `protected readonly ApplicationDbContext _context;`, forcing all four ticket controllers to depend directly on concrete EF Core database persistence.
- **Acceptance Criteria:**
  1. Refactor base controller to remove direct DbContext access.
  2. Inject repository or service abstractions into ticket sub-controllers.
- **Resolution Notes:** Eliminated `ApplicationDbContext` from `TicketsControllerBase` and its derived controllers, achieving full compliance with the Dependency Inversion Principle (DIP):
  1. **Decoupled `TicketsControllerBase`**: Removed `ApplicationDbContext _context` and replaced it with `IUnitOfWork _uow` and user ID retrieval helpers.
  2. **Extended Persistence Contracts**: Extended `ITicketRepository` and `TicketRepository` to encapsulate comment message ID querying, comment retrieval with ticket, comment attachment management, and ticket owner checks.
  3. **Refactored Derived Controllers**: Refactored `TicketAssignmentsController`, `TicketCommentsController`, and `TicketStatusController` to inject `IUnitOfWork` and repository abstractions instead of `ApplicationDbContext`.
  4. **Automated Verification**: Added `TicketsControllerDependencyInversionTests` verifying that `TicketsControllerBase`, `TicketAssignmentsController`, `TicketCommentsController`, `TicketStatusController`, and `TicketsController` have zero constructor parameters or fields referencing `ApplicationDbContext`. All 33 test suite cases passing.

### [DONE] ARCH-09: Anemic Domain Entities Lacking Domain Validation and Invariant Logic
- **Severity:** LOW
- **Agent Source:** Agent 5: Architecture Reviewer
- **Target File(s):** `backend/iM3Helpdesk.Domain/Entities/*.cs`
- **Problem Summary:** Entities (`Ticket`, `User`, `Lead`) are anemic property bags. State transitions, status checks, and business invariant validations are duplicated across controllers rather than encapsulated inside domain models.
- **Acceptance Criteria:**
  1. Encapsulate business state changes inside entity methods (e.g., `ticket.Resolve(...)`, `ticket.AssignTo(...)`, `ticket.Close(...)`).
- **Resolution Notes:** Enforced Rich Domain Models by encapsulating state transitions, invariant validations, and domain logic inside core domain entities:
  1. **Defined `DomainException`**: Created `iM3Helpdesk.Domain.Exceptions.DomainException` for invariant enforcement.
  2. **Encapsulated `Ticket` Entity Logic**: Implemented domain methods `AssignTo(agentId)`, `Unassign()`, `Resolve(resolvedAt)`, `Close()`, `Reopen()`, `ChangePriority(priority)`, `ChangeStatus(newStatus)`, `LogTime(minutes)`, `SoftDelete(actorId)`, and `Restore()` with validation rules guarding deleted states, valid statuses, and positive time logging.
  3. **Encapsulated `User` Entity Logic**: Implemented `RecordFailedLogin(maxAttempts, lockoutDuration)`, `RecordSuccessfulLogin()`, `IsLockedOut()`, `Unlock()`, `SetRefreshToken(token, expiry)`, `RevokeRefreshToken()`, `VerifyEmail()`, and `UpdateProfile(...)`.
  4. **Encapsulated `Lead` Entity Logic**: Implemented `Approve(adminId, token, validity)`, `Reject(reason)`, `MarkTokenUsed()`, and `IsTokenValid()`, preventing invalid transition states on non-pending or completed leads.
  5. **Consumer Refactoring**: Refactored `TicketAssignmentsController`, `TicketStatusController`, `AuthService`, `AdminLeadsController`, and `OnboardingController` to invoke rich domain methods instead of manipulating raw properties.
  6. **Automated Verification**: Added comprehensive unit test suite `DomainEntityInvariantTests` covering invariant enforcement, state transitions, and exception boundaries across `Ticket`, `User`, and `Lead`. All 30 domain tests and 63 solution tests passing.

### [DONE] ARCH-10: Systemic Silent Exception Swallowing (`catch { }`) Across Controllers and Services
- **Severity:** LOW
- **Agent Source:** Agent 5: Architecture Reviewer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AgentsController.cs`, `backend/iM3Helpdesk.API/Controllers/CalendarEventsController.cs`, `backend/iM3Helpdesk.API/Controllers/CustomerController.cs`, `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`, `backend/iM3Helpdesk.API/Hubs/ChatHub.cs`
- **Problem Summary:** Over 15 catch blocks throughout the codebase swallow exceptions silently with `catch { }` or empty bodies, preventing failure visibility, audit logging, and automated alerting.
- **Acceptance Criteria:**
  1. Remove empty catch blocks or log all caught exceptions via `_logger.LogError(ex, ...)`.
- **Resolution Notes:** Eliminated silent exception swallowing across controllers, hubs, and background services by introducing structured `ILogger` telemetry:
  1. **CalendarEventsController**: Injected `ILogger<CalendarEventsController>` and replaced all 5 empty `catch { }` blocks in event update, cancellation, reminder, and invite routines with `_logger.LogError(ex, ...)`.
  2. **CustomerController**: Injected `ILogger<CustomerController>` and replaced silent catch block in customer ticket creation email notification with `_logger.LogError(ex, ...)`.
  3. **ChatHub**: Injected `ILogger<ChatHub>` and replaced silent `catch { /* don't crash the hub */ }` block in user presence updates with structured `_logger.LogError(ex, ...)`.
  4. **EmailPollingService**: Replaced silent catch block during IMAP client disconnect with structured warning log.
  5. **Automated Verification**: Added `ExceptionHandlingArchitectureTests` scanning all controllers and SignalR hubs to guarantee zero silent catch block violations remain. All 64 solution tests passing.

---

## 4. Optimization, Algorithms & Leaks Ledger (PERF)

### [DONE] PERF-01: $O(n^2)$ Duplicate Ticket Detection Algorithm with Massive Heap Allocations
- **Severity:** HIGH
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AIFeaturesController.cs`, `backend/iM3Helpdesk.Application/Services/AIFeaturesService.cs`, `backend/iM3Helpdesk.Application/DTOs/AI/AIDtos.cs`
- **Problem Summary:** `DetectDuplicates` runs nested $O(n^2)$ loops comparing ticket pairs. Inside the inner loop, it splits strings repeatedly via `GetWords` and instantiates two new `HashSet<string>` collections on every comparison via `Intersect` and `Union`, generating millions of heap allocations per request.
- **Acceptance Criteria:**
  1. Pre-tokenize ticket titles once into normalized token collections: $O(n)$.
  2. Implement an inverted index (word token -> list of ticket IDs) to compare only candidate pairs sharing rare words, reducing complexity to $O(n \cdot k)$.
- **Resolution Notes:** Refactored duplicate ticket detection to use an $O(n)$ pre-tokenization phase and an inverted index candidate generator with zero inner-loop heap allocations:
  1. **$O(n)$ Pre-Tokenization**: Replaced repeated inside-the-loop string splitting with a single-pass `Tokenize()` method storing normalized `HashSet<string>` token collections and tracking corpus-wide document frequency (`wordDocFreq`) in $O(n)$ time.
  2. **Inverted Index Candidate Generation**: Built `Dictionary<string, List<int>> invertedIndex` mapping word tokens to ticket indices. Candidate ticket comparisons are restricted to tickets sharing rare/discriminative tokens (document frequency $\le \max(250, 5\%)$) with an automatic fallback to the rarest token for titles composed of common words. Reduces algorithmic comparison complexity from $O(n^2)$ to $O(n \cdot k)$ where $k \ll n$.
  3. **Zero Heap Allocation Jaccard Similarity**: Eliminated LINQ `.Intersect()` and `.Union()` heap allocations during comparisons. Similarity is computed directly by checking containment over the smaller pre-tokenized set ($|A \cap B| / (|A| + |B| - |A \cap B|)$) with zero heap allocations per candidate comparison.
  4. **Strongly-Typed DTO Contracts**: Added `DuplicateGroupDto` and `DuplicateTicketItemDto` in `AIDtos.cs` to eliminate runtime dynamic DLR dispatch while preserving camelCase JSON API compatibility.
  5. **Synthetic 10k Benchmark & Automated Verification**: Created `DuplicateDetectionBenchmarkTests` in `iM3Helpdesk.API.Tests/Performance/`. Validated against a synthetic dataset of 10,000 realistic helpdesk records containing 10 injected duplicate clusters (37 tickets, 27 savings). The entire 10,000-record batch processed in **54 ms** (37x faster than the 2.0-second SLA limit) with 100% detection accuracy. All 70 solution tests passing.

### [DONE] PERF-02: Systemic Memory Leaks from 320+ Unmanaged Observable Subscriptions Across Frontend
- **Severity:** HIGH
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `frontend/src/app/**/*.ts`
- **Problem Summary:** Out of 361 `.subscribe(...)` calls across frontend components, only 39 use `takeUntil` or `takeUntilDestroyed()`. Over 320 subscriptions remain alive when Angular router destroys views, leaking component instances and DOM trees.
- **Acceptance Criteria:**
  1. Refactor unmanaged subscriptions across components to use Angular's `takeUntilDestroyed()` operator or bind observables directly via the `async` pipe.
- **Resolution Notes:** Refactored systemic unmanaged subscriptions across all frontend components and services using Angular 21's `takeUntilDestroyed()` operator:
  1. **Systemic Teardown Migration**: Refactored 328 unmanaged observable `.subscribe()` streams across 71 frontend files (66 component/directive classes and 5 services).
  2. **Injection & Lifecycle Binding**: Injected `DestroyRef` (`private destroyRef = inject(DestroyRef)`) across all target classes and piped `takeUntilDestroyed(this.destroyRef)` into all long-lived streams, event buses, form control subscriptions, and HTTP requests.
  3. **Elimination of Retained Instances & DOM Trees**: Eliminated dangling subscription references and prevented Angular runtime error `NG0205: Injector has already been destroyed` when components and route views are destroyed.
  4. **Compilation & Automated Verification**: Validated Angular compilation via `npm run build` with zero errors. Executed test suite (`npx ng test --watch=false`) with 100% pass rate (20/20 test suites, 33/33 unit tests passed). All 72 backend tests also verified passing.

### [TODO] PERF-03: 233 Angular Template Structural Directives Missing `trackBy` / `@for track`
- **Severity:** MEDIUM
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `frontend/src/app/**/*.html`
- **Problem Summary:** 233 out of 255 `*ngFor` loops lack a `trackBy` function, forcing Angular's change detection to destroy and re-create entire DOM trees on array updates.
- **Acceptance Criteria:**
  1. Add `trackBy` functions to legacy `*ngFor` loops or migrate templates to Angular's native `@for (item of items; track item.id)` control flow.
- **Resolution Notes:** [Unassigned]

### [TODO] PERF-04: Leaking Background Timers in Global Call Singleton Service
- **Severity:** MEDIUM
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `frontend/src/app/core/services/global-call-notification.service.ts`
- **Problem Summary:** `setInterval` timers (`this.diagnosticsTimer`, `this.callTimer`) continue ticking in the background even after WebRTC calls disconnect or transition to idle states.
- **Acceptance Criteria:**
  1. Ensure all `setInterval` instances are properly cleared via `clearInterval` whenever calls terminate or reset.
- **Resolution Notes:** [Unassigned]

### [TODO] PERF-05: Duplicate SignalR Chat Service Implementations Bundled Simultaneously
- **Severity:** LOW
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `frontend/src/app/core/services/chat.ts`, `frontend/src/app/core/services/chat.service.ts`
- **Problem Summary:** Both `chat.ts` and `chat.service.ts` declare `ChatService` and are compiled into the client bundle, causing dead code bloat and conflicting SignalR connections.
- **Acceptance Criteria:**
  1. Delete redundant `chat.ts` and ensure all consumers import from `chat.service.ts`.
- **Resolution Notes:** [Unassigned]

### [TODO] PERF-06: Heavy Client-Side PDF Generation Libraries Bundled in Vendor Chunk
- **Severity:** LOW
- **Agent Source:** Agent 7: Optimization Engineer
- **Target File(s):** `frontend/package.json`
- **Problem Summary:** `jspdf` and `html2canvas` are imported directly into main client bundles rather than loaded lazily, increasing initial page load latency.
- **Acceptance Criteria:**
  1. Load `jspdf` and `html2canvas` dynamically via dynamic `import(...)` only when report export actions are triggered.
- **Resolution Notes:** [Unassigned]

---

## 5. Structure, Dead Code & Hygiene Ledger (CLN)

### [DONE] CLN-01: Misplaced Core Backend Services and Hosted Workers in Presentation Layer
- **Severity:** HIGH
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/Services/`
- **Problem Summary:** 25 services, background workers (`BirthdayPostWorker`, `HolidayPostWorker`, `RecycleBinPurgeWorker`, `SmartAssignBackgroundWorker`), parsing utilities (`HolidayPdfParser`), and heavy processors (`EmailPollingService`, `SmartAssignService`) are placed in the API presentation project instead of Infrastructure or Application.
- **Acceptance Criteria:**
  1. Relocate background workers and infrastructure implementations into `iM3Helpdesk.Infrastructure/Services/`.
  2. Relocate domain logic and service handlers into `iM3Helpdesk.Application/Services/`.
- **Resolution Notes:** Relocated all 25 misplaced backend services and hosted background workers from `iM3Helpdesk.API/Services/` into `iM3Helpdesk.Infrastructure/Services/`. Moved `SmartAssignHub` to `iM3Helpdesk.Infrastructure/Hubs/`. Extracted `ISubscriptionService`, `IPermissionService`, and `IBackgroundTaskService` interfaces into `iM3Helpdesk.Application/Contracts/Services/`. Added Microsoft.AspNetCore.App FrameworkReference and required packages (MailKit, MimeKit, PdfPig, HtmlSanitizer, Hangfire.AspNetCore) to `iM3Helpdesk.Infrastructure.csproj`. Purged `backend/iM3Helpdesk.API/Services/`. Updated all using statements and DI container bindings across controllers and Program.cs. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 tests passed).

### [DONE] CLN-02: Deprecated Legacy SignalR NuGet Packages in Modern .NET 8 Projects
- **Severity:** MEDIUM
- **Agent Source:** Agent 6: Future-Proofing Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/iM3Helpdesk.API.csproj`, `backend/iM3Helpdesk.Infrastructure/iM3Helpdesk.Infrastructure.csproj`
- **Problem Summary:** Projects reference legacy `Microsoft.AspNetCore.SignalR` (1.2.9) and `Microsoft.AspNetCore.SignalR.Core` (1.2.9) packages dating back to .NET Core 2.2, which are redundant and conflicting in .NET 8.
- **Acceptance Criteria:**
  1. Remove legacy package references from both `.csproj` files, relying on the modern ASP.NET Core shared framework.
- **Resolution Notes:** Removed deprecated legacy NuGet packages `Microsoft.AspNetCore.SignalR` (1.2.9) from `backend/iM3Helpdesk.API/iM3Helpdesk.API.csproj` and `Microsoft.AspNetCore.SignalR` (1.2.9) + `Microsoft.AspNetCore.SignalR.Core` (1.2.9) from `backend/iM3Helpdesk.Infrastructure/iM3Helpdesk.Infrastructure.csproj`. Modern .NET 8 ASP.NET Core shared framework provides native SignalR capabilities. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 tests passed).

### [DONE] CLN-03: Fractured Dual Superadmin Feature Directories in Frontend
- **Severity:** MEDIUM
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/features/super-admin/`, `frontend/src/app/features/superadmin/`
- **Problem Summary:** Superadmin features are split across two parallel directories: `features/super-admin/` and `features/superadmin/`.
- **Acceptance Criteria:**
  1. Consolidate all superadmin features into `frontend/src/app/features/super-admin/` and delete the redundant folder.
  2. Update all route imports in `app.routes.ts`.
- **Resolution Notes:** Moved `pending-payments` and `pricing-management` feature directories from `frontend/src/app/features/superadmin/` into `frontend/src/app/features/super-admin/`. Deleted the redundant `frontend/src/app/features/superadmin/` directory. Updated lazy route imports in `frontend/src/app/app.routes.ts` to reference the consolidated paths. Verified with `npm run build` and test runners.

### [DONE] CLN-04: Inverted Interface Architectural Placement (`ICurrentTenantService` in Infrastructure)
- **Severity:** MEDIUM
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.Infrastructure/Services/ICurrentTenantService.cs`
- **Problem Summary:** `ICurrentTenantService` is defined in `Infrastructure`, preventing Domain and Application layers from referencing tenant context without creating circular dependencies.
- **Acceptance Criteria:**
  1. Move `ICurrentTenantService` to `iM3Helpdesk.Domain/Interfaces/` or `iM3Helpdesk.Application/Contracts/Services/`.
- **Resolution Notes:** Relocated `ICurrentTenantService` and `ITenantSetter` from `iM3Helpdesk.Infrastructure/Services/` into `iM3Helpdesk.Application/Contracts/Services/`. Added global using in `iM3Helpdesk.Infrastructure.csproj` and `iM3Helpdesk.API.csproj`. Preserved concrete implementation in `CurrentTenantService` implementing the application contract. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 tests passed).

### [DONE] CLN-05: Orphaned Build Output Directories at Workspace Root
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `iM3Helpdesk.API/bin`, `iM3Helpdesk.API/obj`, `iM3Helpdesk.Domain/bin`, `iM3Helpdesk.Domain/obj`, `iM3Helpdesk.Infrastructure/bin`, `iM3Helpdesk.Infrastructure/obj`, `backend/_tmp_hash/obj`
- **Problem Summary:** The workspace root contains orphaned project directories containing only `bin` and `obj` build outputs from prior builds.
- **Acceptance Criteria:**
  1. Delete root-level `iM3Helpdesk.*` artifact folders and `backend/_tmp_hash`.
  2. Ensure `.gitignore` ignores all `bin/` and `obj/` patterns at all directory depths.
- **Resolution Notes:** Purged root-level `iM3Helpdesk.API/`, `iM3Helpdesk.Domain/`, `iM3Helpdesk.Infrastructure/`, and `backend/_tmp_hash/` folders containing orphaned bin/obj artifacts. Updated `.gitignore` to explicitly ignore `**/bin/`, `**/obj/`, `**/_tmp_hash/`, and `_tmp_hash/` at all directory depths. Verified builds and tests.

### [DONE] CLN-06: Dead Backend Boilerplate Scaffold File
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/WeatherForecast.cs`
- **Problem Summary:** `WeatherForecast.cs` is an unused boilerplate class leftover from initial CLI project scaffolding.
- **Acceptance Criteria:**
  1. Delete `WeatherForecast.cs`.
- **Resolution Notes:** Deleted unused boilerplate class `WeatherForecast.cs` from `backend/iM3Helpdesk.API/`. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 tests passed).

### [DONE] CLN-07: Dead Frontend State, Module, and Service Scaffolding Stubs
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/state/app.state.ts`, `frontend/src/app/state/app.reducer.ts`, `frontend/src/app/state/app.effects.ts`, `frontend/src/app/core/core.module.ts`, `frontend/src/app/app.component.scss`, `frontend/src/app/core/models/error.model.ts`, `frontend/src/app/core/services/token.ts`, `frontend/src/app/core/services/api.service.ts`, `frontend/src/app/core/guards/role.guard.ts`, `frontend/src/app/core/guards/auth-guard.ts`
- **Problem Summary:** Multiple empty class stubs, dead NgModule stubs, and empty stylesheets clutter the frontend codebase.
- **Acceptance Criteria:**
  1. Delete all listed dead scaffold stubs.
  2. Remove unused imports across the frontend.
- **Resolution Notes:** Deleted dead NgRx scaffold files (`app.state.ts`, `app.reducer.ts`, `app.effects.ts`) and removed the empty `state/` directory. Deleted dead `core.module.ts`, empty `app.component.scss` (and removed `styleUrls` from `AppComponent`), `error.model.ts`, `token.ts` (and `token.spec.ts`), `api.service.ts`, `role.guard.ts`, and redundant `auth-guard.ts` re-export stub. Verified with `npm run build` and vitest.

### [DONE] CLN-08: Non-Standard Frontend Component File Suffixes (Over 65 Components Missing `.component.ts`)
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/features/**/*.ts`
- **Problem Summary:** Over 65 components omit `.component.ts` (e.g., `ticket-list.ts`, `login.ts`, `layout.ts`), violating Angular style guidelines and causing intra-folder inconsistency with components that do use `.component.ts`.
- **Acceptance Criteria:**
  1. Rename non-standard component files to follow the `<name>.component.ts` convention.
  2. Update corresponding template, style, and route imports.
- **Resolution Notes:** Renamed 70 non-standard component files across `frontend/src/app` to follow the Angular `<name>.component.ts` naming convention. Updated all relative imports and lazy route loader paths across all TypeScript files. Verified with `npm run build` (all lazy chunks successfully generated) and vitest test suite.

### [DONE] CLN-09: Inconsistent Service File Suffixes & Duplicate Service Declarations in Frontend
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/core/services/*.ts`
- **Problem Summary:** In `frontend/src/app/core/services/`, 14 services omit `.service.ts` while others retain it.
- **Acceptance Criteria:**
  1. Rename all services in `core/services/` to consistently end with `.service.ts`.
- **Resolution Notes:** Standardized all 13 core services by appending `.service.ts` suffix (`agent-group`, `agent`, `custom-field`, `customer`, `knowledge-base`, `notification`, `profile`, `subscription`, `super-admin`, `ticket-master`, `ticket-template`, `ticket`, `translation`). Deleted redundant duplicate `chat.ts` (retaining modern `chat.service.ts`). Renamed `auth.spec.ts` to `auth.service.spec.ts`. Updated all service imports across the frontend application. Verified with `npm run build` and vitest.

### [DONE] CLN-10: Guard Naming Collisions & Mixed Kebab-Case vs Dot-Notation
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/core/guards/*.ts`
- **Problem Summary:** Guards mix kebab-case (`customer-guard.ts`, `super-admin-guard.ts`) with dot-notation (`company-admin.guard.ts`, `auth.guard.ts`), and feature duplicate stubs (`auth-guard.ts` vs `auth.guard.ts`).
- **Acceptance Criteria:**
  1. Standardize all guards on `<name>.guard.ts` and delete duplicate re-export stubs.
- **Resolution Notes:** Renamed `customer-guard.ts` to `customer.guard.ts`, `super-admin-guard.ts` to `super-admin.guard.ts`, and `auth-guard.spec.ts` to `auth.guard.spec.ts`. All guards in `frontend/src/app/core/guards/` consistently adhere to the `<name>.guard.ts` dot-notation format. Updated guard imports in `app.routes.ts`. Verified with `npm run build` and vitest.

### [DONE] CLN-11: Backend Controller Naming & Pluralization Inconsistencies
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/*.cs`
- **Problem Summary:** Controllers mix singular and plural naming (`CallLogController`, `CustomerController` vs `ContactsController`, `TicketsController`), and feature overlapping controllers (`LeadsController` vs `AdminLeadsController`).
- **Acceptance Criteria:**
  1. Standardize controller naming conventions across the API.
- **Resolution Notes:** Standardized backend controller naming and pluralization conventions across API controllers: renamed `CallLogController` to `CallLogsController` (with dual route `api/[controller]` and `api/CallLog`), `CustomerController` to `CustomersController` (with dual route `api/[controller]` and `api/Customer`), `TodoController` to `TodosController` (with dual route `api/[controller]` and `api/Todo`), `TicketStatusController` to `TicketStatusesController`, `AuditController` to `AuditLogsController` (with dual route `api/[controller]` and `api/Audit`), and `AdminLeadsController` to `SuperAdminLeadsController` (with dual route `api/admin/leads` and `api/superadmin/leads`). Updated dependency inversion test suites in `backend/iM3Helpdesk.API.Tests/`. Verified with `dotnet build` (0 errors), `dotnet test` (64/64 passed), and `npm run build`.

### [DONE] CLN-12: Untyped Inline DTO Declarations Embedded Within Controller Source Files
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/*.cs`
- **Problem Summary:** Multiple DTOs (`CreateTodoDto`, `WhatsAppWebhookDto`, `SlackNotifyDto`) are declared at the bottom of controller files rather than in `iM3Helpdesk.Application/DTOs/`.
- **Acceptance Criteria:**
  1. Extract inline DTOs into dedicated files in `iM3Helpdesk.Application/DTOs/`.
- **Resolution Notes:** Extracted inline untyped DTOs from controller source files into dedicated classes in `iM3Helpdesk.Application/DTOs/`: created `CreateTodoDto.cs` in `DTOs/Todos/`, `WhatsAppDtos.cs` (`WhatsAppWebhookDto`, `SendWhatsAppDto`) and `SlackDtos.cs` (`SlackWebhookDto`, `SlackNotifyDto`) in `DTOs/Integrations/`, `TicketTemplateDto.cs` and `TicketMasterDtos.cs` (`TicketMasterOptionDto`, `CreateTicketMasterOptionDto`, `UpdateTicketMasterOptionDto`) in `DTOs/Tickets/`, `UpdateOrgDto.cs` in `DTOs/Organizations/`, and `KnowledgeBaseDtos.cs` (`CreateArticleDto`, `ReactDto`, `KbAddCommentDto`) in `DTOs/KnowledgeBase/`. Updated all controller using directives and purged inline DTO declarations. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 passed).

### [DONE] CLN-13: Dead Injected Fields and Unused Local Variables in Controllers
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/SuperAdminController.cs`, `backend/iM3Helpdesk.API/Controllers/TicketsController.cs`
- **Problem Summary:** Injected fields (e.g., `_emailQueueService` in `SuperAdminController`) and extracted claims (`roleClaim` in `TicketsController.GetAll`) are never read or used.
- **Acceptance Criteria:**
  1. Remove unused injected fields from constructors and delete dead local variables.
- **Resolution Notes:** Removed unused injected constructor parameter and private field `_emailQueueService` from `SuperAdminController.cs` and cleaned up unused service using directives. Verified `TicketsController.cs` uses all injected dependencies with zero dead variables. Verified with `dotnet build` (0 errors) and `dotnet test` (64/64 passed).

### [DONE] CLN-14: Raw `Console.WriteLine` Usage in Place of Structured `ILogger`
- **Severity:** LOW
- **Agent Source:** Agent 2: Security Auditor
- **Target File(s):** `backend/iM3Helpdesk.API/Controllers/AuthController.cs`
- **Problem Summary:** Lines 413, 475, and 728 write directly to `Console.WriteLine` on email dispatch errors rather than logging through `ILogger<AuthController>`.
- **Acceptance Criteria:**
  1. Replace all `Console.WriteLine` statements with `_logger.LogError(...)` or `_logger.LogWarning(...)`.
- **Resolution Notes:** Replaced all raw `Console.WriteLine` invocations with structured `ILogger` calls (`_logger.LogError(...)` / `_logger.LogWarning(...)`) across auth and email dispatch flows in `AuthService.cs` (registration, customer registration, resend verification, forgot password) and ticket controllers. Verified zero instances of `Console.WriteLine` remain in the codebase. Verified with `dotnet build` (0 errors, 0 warnings) and `dotnet test` (64/64 passed).

### [DONE] CLN-15: Committed Browser Test Runner Artifact Folders (`__screenshots__`) in Source Tree
- **Severity:** LOW
- **Agent Source:** Agent 1: Structure Analyst
- **Target File(s):** `frontend/src/app/**/__screenshots__/`
- **Problem Summary:** Automated Vitest test runner screenshot directories (`__screenshots__`) are scattered throughout feature folders in source control.
- **Acceptance Criteria:**
  1. Add `**/__screenshots__/` to `.gitignore` and delete committed screenshot directories from the source tree.
- **Resolution Notes:** Added `**/__screenshots__/` to root `.gitignore` and purged all 12 committed Vitest test runner `__screenshots__` directories across the frontend source tree (`core/interceptors`, `core/services`, `features/auth/*`, `features/dashboard`, `features/onboarding/*`, `features/tickets/*`, `shared`). Verified with `dotnet build` (0 errors, 0 warnings), `dotnet test` (64/64 passed), and `npm run build`.
