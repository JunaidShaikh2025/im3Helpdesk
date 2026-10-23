using System;
using System.Collections.Generic;
using System.Linq;
using iM3Helpdesk.Application.Contracts.Subscriptions;

namespace iM3Helpdesk.Application.Services.Subscriptions;

public class GrowthTierPolicy : ISubscriptionTierPolicy
{
    public string TierKey => "growth";
    public int Rank => 0;
}

public class ProTierPolicy : ISubscriptionTierPolicy
{
    public string TierKey => "pro";
    public int Rank => 1;
}

public class EnterpriseTierPolicy : ISubscriptionTierPolicy
{
    public string TierKey => "enterprise";
    public int Rank => 2;
}

public class DefaultTierPolicy : ISubscriptionTierPolicy
{
    public string TierKey => "default";
    public int Rank => 0;
}

public class SubscriptionTierPolicyProvider : ISubscriptionTierPolicyProvider
{
    private readonly IEnumerable<ISubscriptionTierPolicy> _policies;
    private readonly ISubscriptionTierPolicy _defaultPolicy;

    public SubscriptionTierPolicyProvider(IEnumerable<ISubscriptionTierPolicy> policies)
    {
        _policies = policies ?? Enumerable.Empty<ISubscriptionTierPolicy>();
        _defaultPolicy = new DefaultTierPolicy();
    }

    public ISubscriptionTierPolicy GetPolicy(string? tierKey)
    {
        if (string.IsNullOrWhiteSpace(tierKey))
            return _defaultPolicy;

        return _policies.FirstOrDefault(p =>
            string.Equals(p.TierKey, tierKey.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? _defaultPolicy;
    }

    public int GetRank(string? tierKey) => GetPolicy(tierKey).Rank;
}
