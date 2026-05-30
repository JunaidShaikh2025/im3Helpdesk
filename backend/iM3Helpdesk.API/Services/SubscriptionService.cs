using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Services;

public interface ISubscriptionService
{
    Task<OrganizationSubscription?> GetActiveSubscriptionAsync(Guid organizationId);
    Task<SubscriptionPlan?> GetActivePlanAsync(Guid organizationId);
    Task<bool> HasFeatureAsync(Guid organizationId, string featureKey);
    Task<HashSet<string>> GetActiveFeatureKeysAsync(Guid organizationId);

    /// <summary>Compute total billable amount for given plan + cycle + seats.</summary>
    decimal ComputeAmount(SubscriptionPlan plan, BillingCycle cycle, int seats);
}

public class SubscriptionService : ISubscriptionService
{
    /// <summary>Freshdesk-style minimum: all plans are sold in 10-agent baseline units.</summary>
    public const int MinAgentSeats = 10;

    private readonly ApplicationDbContext _db;
    public SubscriptionService(ApplicationDbContext db) { _db = db; }

    public Task<OrganizationSubscription?> GetActiveSubscriptionAsync(Guid organizationId)
        => _db.OrganizationSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.OrganizationId == organizationId
                        && (s.Status == SubscriptionStatus.Active
                            || s.Status == SubscriptionStatus.Trial
                            || s.Status == SubscriptionStatus.PastDue))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<SubscriptionPlan?> GetActivePlanAsync(Guid organizationId)
    {
        var sub = await GetActiveSubscriptionAsync(organizationId);
        if (sub == null) return null;
        return await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == sub.PlanId);
    }

    public async Task<HashSet<string>> GetActiveFeatureKeysAsync(Guid organizationId)
    {
        var plan = await GetActivePlanAsync(organizationId);
        if (plan == null) return new HashSet<string>();
        return SplitFeatures(plan.FeatureKeysCsv);
    }

    public async Task<bool> HasFeatureAsync(Guid organizationId, string featureKey)
    {
        var keys = await GetActiveFeatureKeysAsync(organizationId);
        return keys.Contains(featureKey.Trim().ToLowerInvariant());
    }

    public decimal ComputeAmount(SubscriptionPlan plan, BillingCycle cycle, int seats)
    {
        if (seats < 1) seats = 1;
        if (cycle == BillingCycle.Monthly)
            return Math.Round(plan.MonthlyPricePerAgent * seats, 2);
        // Annual = monthly*12 * (1 - discount%)
        var annualPerAgent = plan.MonthlyPricePerAgent * 12m * (1m - plan.AnnualDiscountPct / 100m);
        return Math.Round(annualPerAgent * seats, 2);
    }

    public static HashSet<string> SplitFeatures(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new HashSet<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                  .Select(x => x.ToLowerInvariant())
                  .ToHashSet();
    }
}
