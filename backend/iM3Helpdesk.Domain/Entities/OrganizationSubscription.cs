namespace iM3Helpdesk.Domain.Entities;

public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    PastDue = 2,
    Expired = 3,
    Cancelled = 4
}

public enum BillingCycle
{
    Monthly = 0,
    Annual = 1
}

/// <summary>
/// Tracks the currently-active subscription for an organization.
/// Exactly one ACTIVE row per org at any time. History rows kept (Status=Expired/Cancelled).
/// </summary>
public class OrganizationSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public int AgentSeats { get; set; } = 1;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    /// <summary>Total amount that was billed for the current cycle (snapshot of price * seats).</summary>
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
