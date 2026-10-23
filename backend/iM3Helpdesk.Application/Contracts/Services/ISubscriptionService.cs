using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface ISubscriptionService
{
    Task<OrganizationSubscription?> GetActiveSubscriptionAsync(Guid organizationId);
    Task<SubscriptionPlan?> GetActivePlanAsync(Guid organizationId);
    Task<bool> HasFeatureAsync(Guid organizationId, string featureKey);
    Task<HashSet<string>> GetActiveFeatureKeysAsync(Guid organizationId);

    /// <summary>Compute total billable amount for given plan + cycle + seats.</summary>
    decimal ComputeAmount(SubscriptionPlan plan, BillingCycle cycle, int seats);
}
