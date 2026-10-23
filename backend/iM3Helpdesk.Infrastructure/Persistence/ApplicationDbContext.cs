using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
  private readonly Guid? _currentTenantId;
  private readonly bool _isSuperAdmin;

  public Guid? CurrentTenantId => _currentTenantId;
  public bool IsSuperAdmin => _isSuperAdmin;

  public ApplicationDbContext(
      DbContextOptions<ApplicationDbContext> options,
      ICurrentTenantService tenantService)
          : base(options)
  {
    _currentTenantId = tenantService.OrganizationId;
    _isSuperAdmin = tenantService.IsSuperAdmin;
  }

  // ════════════════════════════════════
  // DbSets — All Tables
  // ════════════════════════════════════
  public DbSet<Organization> Organizations => Set<Organization>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Lead> Leads => Set<Lead>();
  public DbSet<Ticket> Tickets => Set<Ticket>();
  public DbSet<TicketComment> TicketComments => Set<TicketComment>();
  public DbSet<Notification> Notifications => Set<Notification>();
  public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
  public DbSet<KbArticle> KbArticles => Set<KbArticle>();
  public DbSet<KbReaction> KbReactions => Set<KbReaction>();
  public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
  public DbSet<KbComment> KbComments => Set<KbComment>();
  public DbSet<TicketTemplate> TicketTemplates => Set<TicketTemplate>();
  public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();
  public DbSet<EmailWebhookLog> EmailWebhookLogs => Set<EmailWebhookLog>();
  public DbSet<AgentGroup> AgentGroups => Set<AgentGroup>();
  public DbSet<AgentGroupMember> AgentGroupMembers => Set<AgentGroupMember>();
  public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
  public DbSet<CustomField> CustomFields => Set<CustomField>();
  public DbSet<TicketFieldMaster> TicketFieldMasters => Set<TicketFieldMaster>();
  public DbSet<TicketCustomFieldValue> TicketCustomFieldValues => Set<TicketCustomFieldValue>();
  public DbSet<TicketViewer> TicketViewers => Set<TicketViewer>();
  public DbSet<TicketWatcher> TicketWatchers => Set<TicketWatcher>();
  public DbSet<EmailNotificationSetting> EmailNotificationSettings => Set<EmailNotificationSetting>();
  public DbSet<Contact> Contacts => Set<Contact>();
  public DbSet<TodoItem> TodoItems => Set<TodoItem>();
  public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
  public DbSet<UserOnlineStatus> UserOnlineStatuses => Set<UserOnlineStatus>();
  public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
  public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();
  public DbSet<CallLog> CallLogs => Set<CallLog>();
  public DbSet<CalendarEvent> CalendarEvents { get; set; }
  public DbSet<Holiday> Holidays => Set<Holiday>();
  public DbSet<HolidayYearSetup> HolidayYearSetups => Set<HolidayYearSetup>();
  public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
  public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
  public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();
  public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
  public DbSet<NoteBook> NoteBooks => Set<NoteBook>();
  public DbSet<NoteSection> NoteSections => Set<NoteSection>();
  public DbSet<NotePage> NotePages => Set<NotePage>();
  public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
  public DbSet<SlaTarget> SlaTargets => Set<SlaTarget>();
  public DbSet<SlaReminder> SlaReminders => Set<SlaReminder>();
  public DbSet<SlaEscalation> SlaEscalations => Set<SlaEscalation>();
  public DbSet<BusinessHours> BusinessHours => Set<BusinessHours>();
  public DbSet<BusinessHoursHoliday> BusinessHoursHolidays => Set<BusinessHoursHoliday>();
  public DbSet<AIAssignmentLog> AIAssignmentLogs => Set<AIAssignmentLog>();
  public DbSet<AIModelVersion> AIModelVersions => Set<AIModelVersion>();
  public DbSet<AIAgentSkillStat> AIAgentSkillStats => Set<AIAgentSkillStat>();
  public DbSet<AISmartAssignConfig> AISmartAssignConfigs => Set<AISmartAssignConfig>();
  public DbSet<UserSession> UserSessions => Set<UserSession>();

  // ════════════════════════════════════
  // OnModelCreating
  // ════════════════════════════════════
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Modular entity type configurations extracted into IEntityTypeConfiguration<T> classes
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    // Dynamic Multi-Tenant & Soft-Delete Global Query Filters
    ApplyTenantQueryFilters(modelBuilder);
  }

  /// <summary>
  /// Configures global query filters for multi-tenant isolation and soft deletion.
  /// Filters are evaluated dynamically per DbContext instance based on the calling tenant and SuperAdmin status.
  /// </summary>
  private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
  {
    // ── Organizations Domain ──
    modelBuilder.Entity<OrganizationSubscription>()
        .HasQueryFilter(s => _isSuperAdmin || s.OrganizationId == _currentTenantId);
    modelBuilder.Entity<PaymentRecord>()
        .HasQueryFilter(p => _isSuperAdmin || p.OrganizationId == _currentTenantId);
    modelBuilder.Entity<Holiday>()
        .HasQueryFilter(h => _isSuperAdmin || h.OrganizationId == _currentTenantId);
    modelBuilder.Entity<HolidayYearSetup>()
        .HasQueryFilter(h => _isSuperAdmin || h.OrganizationId == _currentTenantId);
    modelBuilder.Entity<RolePermission>()
        .HasQueryFilter(rp => _isSuperAdmin || rp.OrganizationId == _currentTenantId);

    // ── Users Domain ──
    modelBuilder.Entity<User>()
        .HasQueryFilter(u => _isSuperAdmin || u.OrganizationId == _currentTenantId);
    modelBuilder.Entity<Contact>()
        .HasQueryFilter(c => _isSuperAdmin || c.OrganizationId == _currentTenantId);

    // ── Tickets Domain ──
    modelBuilder.Entity<Ticket>()
        .HasQueryFilter(t => (_isSuperAdmin || t.OrganizationId == _currentTenantId) && !t.IsDeleted);
    modelBuilder.Entity<TicketComment>()
        .HasQueryFilter(c => _isSuperAdmin || c.OrganizationId == _currentTenantId);
    modelBuilder.Entity<TicketTemplate>()
        .HasQueryFilter(t => _isSuperAdmin || t.OrganizationId == _currentTenantId);
    modelBuilder.Entity<TicketAttachment>()
        .HasQueryFilter(a => _isSuperAdmin || a.OrganizationId == _currentTenantId);
    modelBuilder.Entity<CustomField>()
        .HasQueryFilter(f => _isSuperAdmin || f.OrganizationId == _currentTenantId);
    modelBuilder.Entity<TicketFieldMaster>()
        .HasQueryFilter(m => _isSuperAdmin || m.OrganizationId == _currentTenantId);
    modelBuilder.Entity<TicketCustomFieldValue>()
        .HasQueryFilter(v => _isSuperAdmin || v.OrganizationId == _currentTenantId);
    modelBuilder.Entity<TicketWatcher>()
        .HasQueryFilter(w => _isSuperAdmin || w.OrganizationId == _currentTenantId);
    modelBuilder.Entity<CommentReaction>()
        .HasQueryFilter(r => _isSuperAdmin || r.OrganizationId == _currentTenantId);

    // ── KnowledgeBase Domain ──
    modelBuilder.Entity<KbArticle>()
        .HasQueryFilter(a => _isSuperAdmin || a.OrganizationId == _currentTenantId);
    modelBuilder.Entity<KbReaction>()
        .HasQueryFilter(r => _isSuperAdmin || r.OrganizationId == _currentTenantId);
    modelBuilder.Entity<KbComment>()
        .HasQueryFilter(c => _isSuperAdmin || c.OrganizationId == _currentTenantId);

    // ── Communication Domain ──
    modelBuilder.Entity<Notification>()
        .HasQueryFilter(n => _isSuperAdmin || n.OrganizationId == _currentTenantId);
    modelBuilder.Entity<AgentGroup>()
        .HasQueryFilter(g => _isSuperAdmin || g.OrganizationId == _currentTenantId);
    modelBuilder.Entity<AgentGroupMember>()
        .HasQueryFilter(m => _isSuperAdmin || (m.Group != null && m.Group.OrganizationId == _currentTenantId));
    modelBuilder.Entity<EmailNotificationSetting>()
        .HasQueryFilter(s => _isSuperAdmin || s.OrganizationId == _currentTenantId);
    modelBuilder.Entity<ChatGroup>()
        .HasQueryFilter(g => _isSuperAdmin || g.OrganizationId == _currentTenantId);

    // ── SLA Domain ──
    modelBuilder.Entity<SlaPolicy>()
        .HasQueryFilter(p => _isSuperAdmin || p.OrganizationId == _currentTenantId);
    modelBuilder.Entity<SlaTarget>()
        .HasQueryFilter(t => _isSuperAdmin || t.OrganizationId == _currentTenantId);
    modelBuilder.Entity<SlaReminder>()
        .HasQueryFilter(r => _isSuperAdmin || r.OrganizationId == _currentTenantId);
    modelBuilder.Entity<SlaEscalation>()
        .HasQueryFilter(es => _isSuperAdmin || es.OrganizationId == _currentTenantId);
    modelBuilder.Entity<BusinessHours>()
        .HasQueryFilter(b => _isSuperAdmin || b.OrganizationId == _currentTenantId);
    modelBuilder.Entity<BusinessHoursHoliday>()
        .HasQueryFilter(h => _isSuperAdmin || h.OrganizationId == _currentTenantId);

    // ── Work / Productivity Domain ──
    modelBuilder.Entity<ActivityLog>()
        .HasQueryFilter(a => _isSuperAdmin || a.OrganizationId == _currentTenantId);
    modelBuilder.Entity<NoteBook>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<NoteSection>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<NotePage>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<CalendarEvent>()
        .HasQueryFilter(c => _isSuperAdmin || c.OrganizationId == _currentTenantId);

    // ── AI Domain ──
    modelBuilder.Entity<AIAssignmentLog>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<AIModelVersion>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<AIAgentSkillStat>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
    modelBuilder.Entity<AISmartAssignConfig>()
        .HasQueryFilter(x => _isSuperAdmin || x.OrganizationId == _currentTenantId);
  }
}
