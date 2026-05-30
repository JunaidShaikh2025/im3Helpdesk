using iM3Helpdesk.API.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ISubscriptionService _sub;

    public SubscriptionController(
        ApplicationDbContext db,
        ICurrentTenantService tenant,
        ISubscriptionService sub)
    {
        _db = db;
        _tenant = tenant;
        _sub = sub;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }

    private static int TierRank(string? tierKey) => (tierKey ?? "").ToLowerInvariant() switch
    {
        "growth" => 0,
        "pro" => 1,
        "enterprise" => 2,
        _ => 0,
    };

    // ── Public-to-tenant: list plans (read-only) ────────────────────
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _db.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id,
                p.TierKey,
                p.Name,
                p.Tagline,
                p.Accent,
                p.SortOrder,
                p.Currency,
                p.MonthlyPricePerAgent,
                p.AnnualDiscountPct,
                AnnualPricePerAgent = Math.Round(p.MonthlyPricePerAgent * 12m * (1m - p.AnnualDiscountPct / 100m), 2),
                FeatureKeys = SubscriptionService.SplitFeatures(p.FeatureKeysCsv).ToArray()
            })
            .ToListAsync();
        return Ok(plans);
    }

    // ── Get my org's current subscription + feature keys ───────────
    [HttpGet("me")]
    public async Task<IActionResult> GetMy()
    {
        var orgId = _tenant.OrganizationId;
        if (!orgId.HasValue) return Unauthorized();

        var sub = await _sub.GetActiveSubscriptionAsync(orgId.Value);
        var plan = sub == null ? null : await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == sub.PlanId);
        var features = await _sub.GetActiveFeatureKeysAsync(orgId.Value);

        var daysRemaining = sub == null ? 0 : (int)Math.Max(0, (sub.CurrentPeriodEnd - DateTime.UtcNow).TotalDays);

        return Ok(new
        {
            subscription = sub == null ? null : new
            {
                sub.Id,
                sub.PlanId,
                sub.Status,
                sub.BillingCycle,
                sub.AgentSeats,
                sub.Amount,
                sub.Currency,
                sub.StartedAt,
                sub.CurrentPeriodEnd,
                DaysRemaining = daysRemaining,
                IsTrial = sub.Status == SubscriptionStatus.Trial,
            },
            plan = plan == null ? null : new
            {
                plan.Id, plan.TierKey, plan.Name, plan.Tagline, plan.Accent,
                plan.Currency, plan.MonthlyPricePerAgent, plan.AnnualDiscountPct,
            },
            features = features.ToArray()
        });
    }

    // ── Org admin submits payment to switch/extend plan ────────────
    public class SubmitPaymentDto
    {
        public Guid PlanId { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public int AgentSeats { get; set; } = 1;
        public string? BillingName { get; set; }
        public string? BillingEmail { get; set; }
        public string? BillingAddress { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("payments")]
    public async Task<IActionResult> SubmitPayment([FromBody] SubmitPaymentDto dto)
    {
        var orgId = _tenant.OrganizationId;
        var userId = GetUserId();
        if (!orgId.HasValue || userId == Guid.Empty) return Unauthorized();

        // Only CompanyAdmin can purchase / change plan
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != nameof(UserRole.CompanyAdmin) && role != nameof(UserRole.SuperAdmin))
            return Forbid();

        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == dto.PlanId && p.IsActive);
        if (plan == null) return NotFound(new { error = "Plan not found" });

        // Block downgrades — they must go through support (refund/proration).
        var currentSub = await _sub.GetActiveSubscriptionAsync(orgId.Value);
        if (currentSub != null)
        {
            var currentPlan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == currentSub.PlanId);
            if (currentPlan != null && TierRank(plan.TierKey) < TierRank(currentPlan.TierKey))
            {
                return BadRequest(new { error = "Plan downgrade is not allowed. Please contact support." });
            }
        }

        if (dto.AgentSeats < SubscriptionService.MinAgentSeats)
            return BadRequest(new { error = $"All plans require a minimum of {SubscriptionService.MinAgentSeats} agent seats." });
        var amount = _sub.ComputeAmount(plan, dto.BillingCycle, dto.AgentSeats);

        var sub = currentSub;

        var payment = new PaymentRecord
        {
            OrganizationId = orgId.Value,
            PlanId = plan.Id,
            SubscriptionId = sub?.Id,
            BillingCycle = dto.BillingCycle,
            AgentSeats = dto.AgentSeats,
            Amount = amount,
            Currency = plan.Currency,
            Status = PaymentStatus.Pending,
            BillingName = dto.BillingName,
            BillingEmail = dto.BillingEmail,
            BillingAddress = dto.BillingAddress,
            CardLast4 = dto.CardLast4,
            CardBrand = dto.CardBrand,
            Notes = dto.Notes,
            SubmittedByUserId = userId,
            SubmittedAt = DateTime.UtcNow,
        };
        _db.PaymentRecords.Add(payment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = payment.Id,
            status = payment.Status.ToString(),
            amount = payment.Amount,
            currency = payment.Currency,
            message = "Payment submitted. Waiting for SuperAdmin approval."
        });
    }

    // ── List org's own payments ────────────────────────────────────
    [HttpGet("payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        var orgId = _tenant.OrganizationId;
        if (!orgId.HasValue) return Unauthorized();

        var rows = await _db.PaymentRecords
            .Where(p => p.OrganizationId == orgId.Value)
            .OrderByDescending(p => p.SubmittedAt)
            .Select(p => new
            {
                p.Id,
                p.PlanId,
                PlanName = _db.SubscriptionPlans.Where(pl => pl.Id == p.PlanId).Select(pl => pl.Name).FirstOrDefault(),
                p.BillingCycle,
                p.AgentSeats,
                p.Amount,
                p.Currency,
                p.Status,
                p.CardLast4,
                p.CardBrand,
                p.SubmittedAt,
                p.ReviewedAt,
            })
            .ToListAsync();
        return Ok(rows);
    }
}
