using iM3Helpdesk.API.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/superadmin/subscription")]
[Authorize(Roles = nameof(UserRole.SuperAdmin))]
public class SuperAdminSubscriptionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISubscriptionService _sub;
    private readonly IEmailQueueService _email;

    public SuperAdminSubscriptionController(ApplicationDbContext db, ISubscriptionService sub, IEmailQueueService email)
    {
        _db = db;
        _sub = sub;
        _email = email;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }

    // ── Plans CRUD (edit pricing / features) ───────────────────────
    [HttpGet("plans")]
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await _db.SubscriptionPlans
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id, p.TierKey, p.Name, p.Tagline, p.Accent, p.SortOrder,
                p.Currency, p.MonthlyPricePerAgent, p.AnnualDiscountPct,
                p.IsActive, p.UpdatedAt,
                AnnualPricePerAgent = Math.Round(p.MonthlyPricePerAgent * 12m * (1m - p.AnnualDiscountPct / 100m), 2),
                FeatureKeys = SubscriptionService.SplitFeatures(p.FeatureKeysCsv).ToArray()
            })
            .ToListAsync();
        return Ok(plans);
    }

    public class UpdatePlanDto
    {
        public string? Name { get; set; }
        public string? Tagline { get; set; }
        public string? Accent { get; set; }
        public decimal? MonthlyPricePerAgent { get; set; }
        public decimal? AnnualDiscountPct { get; set; }
        public string? Currency { get; set; }
        public bool? IsActive { get; set; }
        public string[]? FeatureKeys { get; set; }
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanDto dto)
    {
        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return NotFound();

        if (dto.Name != null) plan.Name = dto.Name;
        if (dto.Tagline != null) plan.Tagline = dto.Tagline;
        if (dto.Accent != null) plan.Accent = dto.Accent;
        if (dto.MonthlyPricePerAgent.HasValue && dto.MonthlyPricePerAgent.Value >= 0)
            plan.MonthlyPricePerAgent = dto.MonthlyPricePerAgent.Value;
        if (dto.AnnualDiscountPct.HasValue && dto.AnnualDiscountPct.Value >= 0 && dto.AnnualDiscountPct.Value <= 90)
            plan.AnnualDiscountPct = dto.AnnualDiscountPct.Value;
        if (!string.IsNullOrWhiteSpace(dto.Currency)) plan.Currency = dto.Currency;
        if (dto.IsActive.HasValue) plan.IsActive = dto.IsActive.Value;
        if (dto.FeatureKeys != null)
        {
            plan.FeatureKeysCsv = string.Join(',', dto.FeatureKeys
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct());
        }
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id = plan.Id, message = "Plan updated" });
    }

    // ── Pending payments queue ─────────────────────────────────────
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] string? status = null)
    {
        var query = _db.PaymentRecords.IgnoreQueryFilters().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var st))
            query = query.Where(p => p.Status == st);

        var rows = await query
            .OrderByDescending(p => p.SubmittedAt)
            .Select(p => new
            {
                p.Id,
                p.OrganizationId,
                OrgName = _db.Organizations.IgnoreQueryFilters().Where(o => o.Id == p.OrganizationId).OrderBy(o => o.Id).Select(o => o.Name).FirstOrDefault(),
                p.PlanId,
                PlanName = _db.SubscriptionPlans.Where(pl => pl.Id == p.PlanId).OrderBy(pl => pl.Id).Select(pl => pl.Name).FirstOrDefault(),
                p.BillingCycle,
                p.AgentSeats,
                p.Amount,
                p.Currency,
                p.Status,
                p.CardLast4,
                p.CardBrand,
                p.BillingName,
                p.BillingEmail,
                p.GatewayReference,
                p.Notes,
                p.SubmittedAt,
                p.ReviewedAt,
                p.ReviewNotes
            })
            .ToListAsync();
        return Ok(rows);
    }

    public class ReviewPaymentDto
    {
        public string? Notes { get; set; }
    }

    // ── Approve → activate / extend OrganizationSubscription ────────
    [HttpPost("payments/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePayment(Guid id, [FromBody] ReviewPaymentDto? dto)
    {
        var payment = await _db.PaymentRecords.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return NotFound();
        if (payment.Status != PaymentStatus.Pending) return BadRequest(new { error = $"Payment already {payment.Status}" });

        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == payment.PlanId);
        if (plan == null) return BadRequest(new { error = "Plan no longer exists" });

        // Expire any existing active/trial subscription for this org
        var existing = await _db.OrganizationSubscriptions.IgnoreQueryFilters()
            .Where(s => s.OrganizationId == payment.OrganizationId &&
                       (s.Status == SubscriptionStatus.Active
                     || s.Status == SubscriptionStatus.Trial
                     || s.Status == SubscriptionStatus.PastDue))
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var s in existing)
        {
            s.Status = SubscriptionStatus.Cancelled;
            s.CancelledAt = now;
            s.UpdatedAt = now;
        }

        var periodEnd = payment.BillingCycle == BillingCycle.Monthly
            ? now.AddMonths(1)
            : now.AddYears(1);

        var newSub = new OrganizationSubscription
        {
            OrganizationId = payment.OrganizationId,
            PlanId = plan.Id,
            BillingCycle = payment.BillingCycle,
            AgentSeats = payment.AgentSeats,
            Status = SubscriptionStatus.Active,
            Amount = payment.Amount,
            Currency = payment.Currency,
            StartedAt = now,
            CurrentPeriodEnd = periodEnd,
        };
        _db.OrganizationSubscriptions.Add(newSub);

        payment.Status = PaymentStatus.Approved;
        payment.ReviewedAt = now;
        payment.ReviewedByUserId = GetUserId();
        payment.ReviewNotes = dto?.Notes;
        payment.SubscriptionId = newSub.Id;

        await _db.SaveChangesAsync();

        // Email the org's CompanyAdmin about activation
        try
        {
            var admin = await _db.Users.IgnoreQueryFilters()
                .Where(u => u.OrganizationId == payment.OrganizationId && u.Role == UserRole.CompanyAdmin)
                .Select(u => new { u.Email, u.FullName })
                .FirstOrDefaultAsync();
            if (admin != null)
            {
                var endDateLabel = newSub.CurrentPeriodEnd.ToString("dd MMM yyyy");
                var body = $"""
                    <div style="font-family:sans-serif;max-width:520px;margin:auto;padding:24px">
                      <h2 style="color:#16a34a">Your {plan.Name} Plan is Now Active!</h2>
                      <p>Hi {admin.FullName},</p>
                      <p>Your iM3 Helpdesk subscription has been activated by our team.</p>
                      <table style="width:100%;border-collapse:collapse;margin-top:12px">
                        <tr><td style="padding:6px 0;color:#6b7280">Plan</td><td><strong>{plan.Name}</strong></td></tr>
                        <tr><td style="padding:6px 0;color:#6b7280">Agent Seats</td><td>{newSub.AgentSeats}</td></tr>
                        <tr><td style="padding:6px 0;color:#6b7280">Billing Cycle</td><td>{newSub.BillingCycle}</td></tr>
                        <tr><td style="padding:6px 0;color:#6b7280">Valid Until</td><td><strong>{endDateLabel}</strong></td></tr>
                        <tr><td style="padding:6px 0;color:#6b7280">Amount Charged</td><td>{newSub.Currency} {newSub.Amount:N0}</td></tr>
                      </table>
                      {(dto?.Notes != null ? $"<p style='margin-top:12px'><em>Note: {dto.Notes}</em></p>" : "")}
                      <p style="margin-top:24px;color:#6b7280;font-size:12px">iM3 Helpdesk — Billing Team</p>
                    </div>
                    """;
                await _email.QueueEmailAsync(admin.Email, $"Your {plan.Name} plan is now active — iM3 Helpdesk", body, payment.OrganizationId);
            }
        }
        catch { /* non-critical */ }

        return Ok(new
        {
            paymentId = payment.Id,
            subscriptionId = newSub.Id,
            planName = plan.Name,
            currentPeriodEnd = newSub.CurrentPeriodEnd,
            message = "Payment approved. Subscription activated."
        });
    }

    [HttpPost("payments/{id:guid}/reject")]
    public async Task<IActionResult> RejectPayment(Guid id, [FromBody] ReviewPaymentDto? dto)
    {
        var payment = await _db.PaymentRecords.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return NotFound();
        if (payment.Status != PaymentStatus.Pending) return BadRequest(new { error = $"Payment already {payment.Status}" });

        payment.Status = PaymentStatus.Rejected;
        payment.ReviewedAt = DateTime.UtcNow;
        payment.ReviewedByUserId = GetUserId();
        payment.ReviewNotes = dto?.Notes;
        await _db.SaveChangesAsync();

        // Email the org's CompanyAdmin about rejection
        try
        {
            var admin = await _db.Users.IgnoreQueryFilters()
                .Where(u => u.OrganizationId == payment.OrganizationId && u.Role == UserRole.CompanyAdmin)
                .Select(u => new { u.Email, u.FullName })
                .FirstOrDefaultAsync();
            if (admin != null)
            {
                var planName = (await _db.SubscriptionPlans.Where(p => p.Id == payment.PlanId).Select(p => p.Name).FirstOrDefaultAsync()) ?? "Selected";
                var noteHtml = dto?.Notes != null ? $"<p><strong>Reason:</strong> {dto.Notes}</p>" : "";
                var body = $"""
                    <div style="font-family:sans-serif;max-width:520px;margin:auto;padding:24px">
                      <h2 style="color:#dc2626">Plan Request Not Approved</h2>
                      <p>Hi {admin.FullName},</p>
                      <p>Your request to activate the <strong>{planName}</strong> plan has not been approved at this time.</p>
                      {noteHtml}
                      <p>Please contact our support team if you have questions or need assistance.</p>
                      <p style="margin-top:24px;color:#6b7280;font-size:12px">iM3 Helpdesk — Billing Team</p>
                    </div>
                    """;
                await _email.QueueEmailAsync(admin.Email, "Your iM3 Helpdesk plan request was not approved", body, payment.OrganizationId);
            }
        }
        catch { /* non-critical */ }

        return Ok(new { paymentId = payment.Id, message = "Payment rejected." });
    }
}
