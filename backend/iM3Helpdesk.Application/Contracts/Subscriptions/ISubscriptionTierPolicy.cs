namespace iM3Helpdesk.Application.Contracts.Subscriptions;

public interface ISubscriptionTierPolicy
{
    string TierKey { get; }
    int Rank { get; }
}

public interface ISubscriptionTierPolicyProvider
{
    ISubscriptionTierPolicy GetPolicy(string? tierKey);
    int GetRank(string? tierKey);
}
