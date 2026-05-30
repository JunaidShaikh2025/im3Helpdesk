namespace iM3Helpdesk.Domain.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Refunded = 3
}

/// <summary>
/// A single payment record. Created when an org admin submits payment for a plan.
/// SuperAdmin approves -> on approval the OrganizationSubscription is activated/extended.
/// </summary>
public class PaymentRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }
    public Guid? SubscriptionId { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public int AgentSeats { get; set; } = 1;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Billing snapshot
    public string? BillingName { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }

    // Reference returned by gateway (or manual reference for offline pay).
    public string? GatewayReference { get; set; }
    public string? Notes { get; set; }

    public Guid SubmittedByUserId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}
