using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Application.Contracts.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/superadmin")]
// Only SuperAdmin can access any endpoint in this controller
[Authorize(Roles = nameof(UserRole.SuperAdmin))]
public class SuperAdminController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly IEmailQueueService _emailQueueService;

  public SuperAdminController(
      ApplicationDbContext context,
      IEmailQueueService emailQueueService)
  {
    _context = context;
    _emailQueueService = emailQueueService;
  }

  [HttpGet("organizations")]
  public async Task<IActionResult> GetOrganizations()
  {
    var orgs = await _context.Organizations
        .Select(o => new
        {
          o.Id,
          o.Name,
          o.Slug,
          o.SupportEmail,
          o.IsActive,
          o.TrialEndsAt,
          o.CreatedAt,
          totalUsers = _context.Users
                .IgnoreQueryFilters()
                .Count(u => u.OrganizationId == o.Id),
          totalTickets = _context.Tickets
                .IgnoreQueryFilters()
                .Count(t => t.OrganizationId == o.Id)
        })
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    return Ok(orgs);
  }

  [HttpPut("organizations/{id}/toggle")]
  public async Task<IActionResult> ToggleOrganization(Guid id)
  {
    var org = await _context.Organizations.FindAsync(id);
    if (org == null) return NotFound();

    org.IsActive = !org.IsActive;
    await _context.SaveChangesAsync();

    return Ok(new
    {
      message = $"Organization {(org.IsActive ? "activated" : "deactivated")}",
      isActive = org.IsActive
    });
  }

  [HttpGet("users")]
  public async Task<IActionResult> GetAllUsers()
  {
    var users = await _context.Users
        .IgnoreQueryFilters()
        .Include(u => u.Organization)
        .OrderByDescending(u => u.CreatedAt)
        .Select(u => new
        {
          u.Id,
          u.FullName,
          u.Email,
          u.PhoneNumber,
          Role = u.Role.ToString(),
          u.IsEmailVerified,
          u.CreatedAt,
          u.LastLoginAt,
          organization = u.Organization == null ? null : new
          {
            u.Organization.Name,
            u.Organization.IsActive
          }
        })
        .ToListAsync();

    return Ok(users);
  }

  [HttpGet("users/{id:guid}")]
  public async Task<IActionResult> GetUserById(Guid id)
  {
    var user = await _context.Users
      .IgnoreQueryFilters()
      .Include(u => u.Organization)
      .Where(u => u.Id == id)
      .Select(u => new
      {
        u.Id,
        u.FullName,
        u.Email,
        u.PhoneNumber,
        Role = u.Role.ToString(),
        u.IsEmailVerified,
        u.CreatedAt,
        u.LastLoginAt,
        u.PhotoUrl,
        u.Department,
        u.Designation,
        u.Location,
        u.DateOfBirth,
        u.DateOfJoining,
        u.Gender,
        u.IsTwoFactorEnabled,
        u.Signature,
        isLocked = u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow,
        u.LockedUntil,
        organization = u.Organization == null ? null : new
        {
          u.Organization.Id,
          u.Organization.Name,
          u.Organization.Slug,
          u.Organization.IsActive,
          u.Organization.LogoUrl,
          u.Organization.BrandColor,
          u.Organization.SupportEmail
        }
      })
      .FirstOrDefaultAsync();

    if (user == null) return NotFound();
    return Ok(user);
  }

  [HttpGet("organizations/{id:guid}")]
  public async Task<IActionResult> GetOrganizationById(Guid id)
  {
    var org = await _context.Organizations
      .Where(o => o.Id == id)
      .Select(o => new
      {
        o.Id,
        o.Name,
        o.Slug,
        o.LogoUrl,
        o.BrandColor,
        o.SupportEmail,
        o.IsActive,
        o.TrialEndsAt,
        o.CreatedAt,
        o.Timezone,
        o.EmailPollingEnabled,
        o.EmailPollingOnboardedAt,
        o.EmailPollingIntervalSeconds,
        o.SmtpHost,
        o.SmtpPort,
        o.SmtpFromEmail,
        o.SmtpFromName,
        o.SmtpUsername,
        SmtpPassword = string.IsNullOrWhiteSpace(o.SmtpPassword) ? null : "********",
        o.ImapHost,
        o.ImapPort,
        o.WhatsAppNumber,
        o.TwilioAccountSid,
        hasTwilioAuthToken = !string.IsNullOrWhiteSpace(o.TwilioAuthToken),
        o.SlackWebhookUrl,
        o.TeamsWebhookUrl,
        o.RecycleBinRetentionValue,
        o.RecycleBinRetentionUnit,
        totalUsers = _context.Users
          .IgnoreQueryFilters()
          .Count(u => u.OrganizationId == o.Id),
        activeUsers = _context.Users
          .IgnoreQueryFilters()
          .Count(u => u.OrganizationId == o.Id && (!u.LockedUntil.HasValue || u.LockedUntil < DateTime.UtcNow)),
        totalTickets = _context.Tickets
          .IgnoreQueryFilters()
          .Count(t => t.OrganizationId == o.Id),
        recentUsers = _context.Users
          .IgnoreQueryFilters()
          .Where(u => u.OrganizationId == o.Id)
          .OrderByDescending(u => u.CreatedAt)
          .Take(5)
          .Select(u => new
          {
            u.Id,
            u.FullName,
            u.Email,
            Role = u.Role.ToString(),
            u.PhotoUrl,
            u.CreatedAt
          })
          .ToList()
      })
      .FirstOrDefaultAsync();

    if (org == null) return NotFound();
    return Ok(org);
  }

  [HttpGet("stats")]
  public async Task<IActionResult> GetStats()
  {
    var stats = new
    {
      totalOrganizations = await _context.Organizations.CountAsync(),
      activeOrganizations = await _context.Organizations
            .CountAsync(o => o.IsActive),
      totalUsers = await _context.Users
            .IgnoreQueryFilters().CountAsync(),
      totalTickets = await _context.Tickets
            .IgnoreQueryFilters().CountAsync(),
      newOrgsThisMonth = await _context.Organizations
            .CountAsync(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
      recentOrgs = await _context.Organizations
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
              o.Id,
              o.Name,
              o.IsActive,
              o.CreatedAt,
              o.TrialEndsAt
            })
            .ToListAsync()
    };

    return Ok(stats);
  }

  /// <summary>
  /// Platform-wide realised subscription revenue and each company's current
  /// subscription. This is deliberately SuperAdmin-only: tenants must never
  /// be able to inspect another company's billing data.
  /// </summary>
  [HttpGet("subscription-profit")]
  public async Task<IActionResult> GetSubscriptionProfit()
  {
    var approvedPayments = await _context.PaymentRecords
      .IgnoreQueryFilters()
      .Where(p => p.Status == Domain.Entities.PaymentStatus.Approved)
      .Select(p => new { p.OrganizationId, p.Amount, p.Currency, p.SubmittedAt })
      .ToListAsync();

    var revenueByCurrency = approvedPayments
      .GroupBy(p => string.IsNullOrWhiteSpace(p.Currency) ? "INR" : p.Currency.ToUpper())
      .Select(g => new { currency = g.Key, amount = g.Sum(p => p.Amount) })
      .OrderBy(x => x.currency)
      .ToList();

    var organizations = await _context.Organizations
      .IgnoreQueryFilters()
      .Select(o => new { o.Id, o.Name, o.IsActive })
      .ToListAsync();

    var activeSubscriptions = await _context.OrganizationSubscriptions
      .IgnoreQueryFilters()
      .Where(s => s.Status == Domain.Entities.SubscriptionStatus.Active ||
                  s.Status == Domain.Entities.SubscriptionStatus.Trial ||
                  s.Status == Domain.Entities.SubscriptionStatus.PastDue)
      .Select(s => new
      {
        s.OrganizationId, s.Amount, s.Currency, s.AgentSeats, s.BillingCycle,
        s.Status, s.StartedAt, s.CurrentPeriodEnd,
        PlanName = _context.SubscriptionPlans.Where(p => p.Id == s.PlanId)
          .Select(p => p.Name).FirstOrDefault()
      })
      .ToListAsync();

    var companies = organizations
      .Select(o => new
      {
        id = o.Id,
        name = o.Name,
        isActive = o.IsActive,
        subscription = activeSubscriptions.Where(s => s.OrganizationId == o.Id).Select(s => new
        {
          planName = s.PlanName ?? "Subscription",
          amount = s.Amount,
          currency = s.Currency,
          agentSeats = s.AgentSeats,
          billingCycle = s.BillingCycle.ToString(),
          status = s.Status.ToString(),
          startedAt = s.StartedAt,
          currentPeriodEnd = s.CurrentPeriodEnd
        }).FirstOrDefault(),
        totalPaid = approvedPayments.Where(p => p.OrganizationId == o.Id)
          .GroupBy(p => string.IsNullOrWhiteSpace(p.Currency) ? "INR" : p.Currency.ToUpper())
          .Select(g => new { currency = g.Key, amount = g.Sum(p => p.Amount) })
          .ToList()
      })
      .OrderBy(x => x.name)
      .ToList();

    return Ok(new
    {
      approvedRevenue = revenueByCurrency,
      approvedPaymentCount = approvedPayments.Count,
      subscribedCompanyCount = activeSubscriptions.Select(s => s.OrganizationId).Distinct().Count(),
      companies
    });
  }

  [HttpGet("email-queue")]
  public async Task<IActionResult> GetEmailQueueOverview(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      [FromQuery] string? status = null,
      [FromQuery] int webhookPage = 1,
      [FromQuery] int webhookPageSize = 10)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 5, 100);
    webhookPage = Math.Max(1, webhookPage);
    webhookPageSize = Math.Clamp(webhookPageSize, 5, 100);

    try
    {
      return await GetEmailQueueOverviewModernAsync(
        page, pageSize, status, webhookPage, webhookPageSize,
        includeWebhookLogs: true);
    }
    catch (SqlException ex) when (ex.Number == 207 || ex.Number == 208)
    {
      return await GetEmailQueueOverviewLegacyAsync(
        page, pageSize, status, webhookPage, webhookPageSize,
        includeWebhookLogs: false);
    }
    }

    private async Task<IActionResult> GetEmailQueueOverviewModernAsync(
      int page,
      int pageSize,
      string? status,
      int webhookPage,
      int webhookPageSize,
      bool includeWebhookLogs)
    {

    var queueQuery = _context.EmailQueues
      .IgnoreQueryFilters()
      .AsNoTracking();

    if (!string.IsNullOrWhiteSpace(status))
      queueQuery = queueQuery.Where(e => e.Status == status.Trim());

    var totalItems = await queueQuery.CountAsync();

    var logs = await queueQuery
      .OrderByDescending(e => e.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(e => new
      {
        e.Id,
        e.ToEmail,
        e.Subject,
        e.Status,
        e.CreatedAt,
        e.SentAt,
        e.DeliveredAt,
        e.RetryCount,
        e.NextRetryAt,
        e.ErrorMessage,
        e.MessageId,
        e.ProviderMessageId
      })
      .ToListAsync();

    var totalWebhookItems = 0;
    var webhookLogs = new List<object>();

    if (includeWebhookLogs)
    {
      var webhookQuery = _context.EmailWebhookLogs
        .IgnoreQueryFilters()
        .AsNoTracking();

      totalWebhookItems = await webhookQuery.CountAsync();
      webhookLogs = await webhookQuery
        .OrderByDescending(w => w.ReceivedAt)
        .Skip((webhookPage - 1) * webhookPageSize)
        .Take(webhookPageSize)
        .Select(w => (object)new
        {
          w.Id,
          w.EmailQueueId,
          w.Provider,
          w.EventType,
          w.Recipient,
          w.ProviderMessageId,
          w.Processed,
          w.ProcessingError,
          w.ReceivedAt,
          w.ProcessedAt,
          w.PayloadJson
        })
        .ToListAsync();
    }

    var kpis = new
    {
      totalQueued = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => e.Status == "Queued" || e.Status == "Processing"),
      totalSent = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => e.Status == "Sent" || e.Status == "Delivered" || e.Status == "Opened"),
      totalFailed = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => e.Status == "Failed"),
      totalBounced = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => e.Status == "Bounced")
    };

    return Ok(new
    {
      kpis,
      schemaVersion = "modern",
      emailLogs = new
      {
        page,
        pageSize,
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        items = logs
      },
      webhookLogs = new
      {
        page = webhookPage,
        pageSize = webhookPageSize,
        totalItems = totalWebhookItems,
        totalPages = (int)Math.Ceiling(totalWebhookItems / (double)webhookPageSize),
        items = webhookLogs
      }
    });
  }

  private async Task<IActionResult> GetEmailQueueOverviewLegacyAsync(
      int page,
      int pageSize,
      string? status,
      int webhookPage,
      int webhookPageSize,
      bool includeWebhookLogs)
  {
    var queueQuery = _context.EmailQueues
      .IgnoreQueryFilters()
      .AsNoTracking();

    var normalizedStatus = status?.Trim();
    if (!string.IsNullOrWhiteSpace(normalizedStatus))
    {
      if (normalizedStatus.Equals("Sent", StringComparison.OrdinalIgnoreCase) ||
          normalizedStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
          normalizedStatus.Equals("Opened", StringComparison.OrdinalIgnoreCase))
      {
        queueQuery = queueQuery.Where(e => e.IsSent);
      }
      else if (normalizedStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
               normalizedStatus.Equals("Bounced", StringComparison.OrdinalIgnoreCase))
      {
        queueQuery = queueQuery.Where(e => !e.IsSent && e.RetryCount >= 3);
      }
      else if (normalizedStatus.Equals("Queued", StringComparison.OrdinalIgnoreCase) ||
               normalizedStatus.Equals("Processing", StringComparison.OrdinalIgnoreCase))
      {
        queueQuery = queueQuery.Where(e => !e.IsSent && e.RetryCount < 3);
      }
      else
      {
        queueQuery = queueQuery.Where(e => false);
      }
    }

    var totalItems = await queueQuery.CountAsync();

    var logs = await queueQuery
      .OrderByDescending(e => e.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(e => new
      {
        e.Id,
        e.ToEmail,
        e.Subject,
        Status = e.IsSent ? "Sent" : (e.RetryCount >= 3 ? "Failed" : "Queued"),
        e.CreatedAt,
        e.SentAt,
        DeliveredAt = (DateTime?)null,
        e.RetryCount,
        e.NextRetryAt,
        e.ErrorMessage,
        MessageId = (string?)null,
        ProviderMessageId = (string?)null
      })
      .ToListAsync();

    var totalWebhookItems = 0;
    var webhookLogs = new List<object>();

    if (includeWebhookLogs)
    {
      var webhookQuery = _context.EmailWebhookLogs
        .IgnoreQueryFilters()
        .AsNoTracking();

      totalWebhookItems = await webhookQuery.CountAsync();
      webhookLogs = await webhookQuery
        .OrderByDescending(w => w.ReceivedAt)
        .Skip((webhookPage - 1) * webhookPageSize)
        .Take(webhookPageSize)
        .Select(w => (object)new
        {
          w.Id,
          w.EmailQueueId,
          w.Provider,
          w.EventType,
          w.Recipient,
          w.ProviderMessageId,
          w.Processed,
          w.ProcessingError,
          w.ReceivedAt,
          w.ProcessedAt,
          w.PayloadJson
        })
        .ToListAsync();
    }

    var kpis = new
    {
      totalQueued = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => !e.IsSent && e.RetryCount < 3),
      totalSent = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => e.IsSent),
      totalFailed = await _context.EmailQueues.IgnoreQueryFilters().CountAsync(e => !e.IsSent && e.RetryCount >= 3),
      totalBounced = 0
    };

    return Ok(new
    {
      kpis,
      schemaVersion = "legacy",
      emailLogs = new
      {
        page,
        pageSize,
        totalItems,
        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
        items = logs
      },
      webhookLogs = new
      {
        page = webhookPage,
        pageSize = webhookPageSize,
        totalItems = totalWebhookItems,
        totalPages = (int)Math.Ceiling(totalWebhookItems / (double)webhookPageSize),
        items = webhookLogs
      }
    });
  }


  [HttpPost("email-queue/{id:guid}/retry")]
  public async Task<IActionResult> RetryQueuedEmail(Guid id)
  {
    await _emailQueueService.RetryEmailAsync(id);
    return Ok(new { message = "Email queued for retry" });
  }

  [HttpPost("email-queue/{id:guid}/cancel")]
  public async Task<IActionResult> CancelQueuedEmail(Guid id)
  {
    await _emailQueueService.CancelQueuedEmailAsync(id);
    return Ok(new { message = "Queued email cancelled" });
  }
}
