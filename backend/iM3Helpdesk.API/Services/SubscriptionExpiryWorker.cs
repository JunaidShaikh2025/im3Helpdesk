using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Services;

/// <summary>
/// Runs every hour. Marks subscriptions as Expired when CurrentPeriodEnd
/// has passed and sends a renewal reminder email to the org's CompanyAdmin.
/// Also sends a 3-day advance warning email.
/// </summary>
public class SubscriptionExpiryWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceProvider _services;
    private readonly ILogger<SubscriptionExpiryWorker> _logger;

    public SubscriptionExpiryWorker(IServiceProvider services, ILogger<SubscriptionExpiryWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small boot delay
        try { await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "SubscriptionExpiryWorker error"); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailQueueService>();

        var now = DateTime.UtcNow;

        // ── 1. Expire overdue subscriptions ──────────────────────────────
        var overdue = await db.OrganizationSubscriptions
            .IgnoreQueryFilters()
            .Where(s => (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                     && s.CurrentPeriodEnd <= now)
            .ToListAsync(ct);

        foreach (var sub in overdue)
        {
            sub.Status = SubscriptionStatus.Expired;
            sub.UpdatedAt = now;
            _logger.LogInformation("Subscription {Id} for org {OrgId} marked Expired", sub.Id, sub.OrganizationId);

            // Notify the org's CompanyAdmin
            await NotifyAdminAsync(db, email, sub.OrganizationId,
                subject: "Your iM3 Helpdesk subscription has expired",
                body: BuildExpiredEmail(sub));
        }

        if (overdue.Count > 0)
            await db.SaveChangesAsync(ct);

        // ── 2. Send 3-day advance warning (once per subscription) ─────────
        var threeDaysFromNow = now.AddDays(3);
        var expiringSoon = await db.OrganizationSubscriptions
            .IgnoreQueryFilters()
            .Where(s => (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                     && s.CurrentPeriodEnd > now
                     && s.CurrentPeriodEnd <= threeDaysFromNow
                     && s.RenewalReminderSentAt == null)
            .ToListAsync(ct);

        foreach (var sub in expiringSoon)
        {
            await NotifyAdminAsync(db, email, sub.OrganizationId,
                subject: "Your iM3 Helpdesk subscription expires in 3 days",
                body: BuildExpiringEmail(sub));

            sub.RenewalReminderSentAt = now;
            sub.UpdatedAt = now;
        }

        if (expiringSoon.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private static async Task NotifyAdminAsync(
        ApplicationDbContext db,
        IEmailQueueService emailQueue,
        Guid orgId,
        string subject,
        string body)
    {
        var admin = await db.Users
            .IgnoreQueryFilters()
            .Where(u => u.OrganizationId == orgId && u.Role == UserRole.CompanyAdmin)
            .Select(u => new { u.Email, u.FullName })
            .FirstOrDefaultAsync();

        if (admin != null)
            await emailQueue.QueueEmailAsync(admin.Email, subject, body, orgId);
    }

    private static string BuildExpiredEmail(OrganizationSubscription sub)
    {
        var endDate = sub.CurrentPeriodEnd.ToString("dd MMM yyyy");
        return $"""
            <div style="font-family:sans-serif;max-width:520px;margin:auto;padding:24px">
              <h2 style="color:#dc2626">Subscription Expired</h2>
              <p>Your iM3 Helpdesk subscription expired on <strong>{endDate}</strong>.</p>
              <p>Your team has lost access to premium features. To restore access, please log in and upgrade your plan.</p>
              <a href="#" style="display:inline-block;margin-top:16px;padding:10px 20px;background:#2563eb;color:#fff;border-radius:6px;text-decoration:none">Renew Now</a>
              <p style="margin-top:24px;color:#6b7280;font-size:12px">iM3 Helpdesk — Subscription Team</p>
            </div>
            """;
    }

    private static string BuildExpiringEmail(OrganizationSubscription sub)
    {
        var endDate = sub.CurrentPeriodEnd.ToString("dd MMM yyyy");
        return $"""
            <div style="font-family:sans-serif;max-width:520px;margin:auto;padding:24px">
              <h2 style="color:#d97706">Subscription Expiring Soon</h2>
              <p>Your iM3 Helpdesk subscription expires on <strong>{endDate}</strong> (3 days remaining).</p>
              <p>Renew now to avoid any interruption to your team's helpdesk access.</p>
              <a href="#" style="display:inline-block;margin-top:16px;padding:10px 20px;background:#2563eb;color:#fff;border-radius:6px;text-decoration:none">Renew Subscription</a>
              <p style="margin-top:24px;color:#6b7280;font-size:12px">iM3 Helpdesk — Subscription Team</p>
            </div>
            """;
    }
}
