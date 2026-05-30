using iM3Helpdesk.API.Services;
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

    public SuperAdminSubscriptionController(ApplicationDbContext db, ISubscriptionService sub)
    {
        _db = db;
        _sub = sub;
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
                OrgName = _db.Organizations.IgnoreQueryFilters().Where(o => o.Id == p.OrganizationId).Select(o => o.Name).FirstOrDefault(),
                p.PlanId,
                PlanName = _db.SubscriptionPlans.Where(pl => pl.Id == p.PlanId).Select(pl => pl.Name).FirstOrDefault(),
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
        return Ok(new { paymentId = payment.Id, message = "Payment rejected." });
    }
}
