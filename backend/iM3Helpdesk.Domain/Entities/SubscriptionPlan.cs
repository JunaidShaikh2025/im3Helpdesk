namespace iM3Helpdesk.Domain.Entities;

/// <summary>
/// A globally-defined subscription plan (Growth, Pro, Enterprise).
/// Pricing is per-agent. Annual price is auto-computed as monthly*12*(1-AnnualDiscountPct)
/// at API layer unless explicitly overridden via AnnualPricePerAgent.
/// SuperAdmin can edit price and which features each tier unlocks.
/// </summary>
public class SubscriptionPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Stable key: "growth" | "pro" | "enterprise".</summary>
    public string TierKey { get; set; } = string.Empty;

    /// <summary>Display name e.g. "Growth", "Pro", "Enterprise".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short marketing tagline shown on plans page.</summary>
    public string? Tagline { get; set; }

    /// <summary>Sort order in tiers list (0,1,2).</summary>
    public int SortOrder { get; set; }

    /// <summary>Accent colour shown on cards/tabs (hex).</summary>
    public string Accent { get; set; } = "#2563eb";

    /// <summary>Currency code (e.g. INR, USD).</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Price per agent per month.</summary>
    public decimal MonthlyPricePerAgent { get; set; }

    /// <summary>Annual discount percentage applied to MonthlyPricePerAgent*12 (e.g. 20 = 20% off). 0..90.</summary>
    public decimal AnnualDiscountPct { get; set; } = 20m;

    /// <summary>If true, this plan can be selected by orgs.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Comma-separated feature keys included in this tier. Edited by SuperAdmin.</summary>
    public string FeatureKeysCsv { get; set; } = string.Empty;
}
