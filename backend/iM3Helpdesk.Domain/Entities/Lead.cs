using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Domain.Entities;

public class Lead
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string OrganizationName { get; set; } = string.Empty;
  public string OwnerName { get; set; } = string.Empty;
  public string WorkEmail { get; set; } = string.Empty;
  public string? Phone { get; set; }
  public string? Notes { get; set; }
  public LeadStatus Status { get; set; } = LeadStatus.Pending;
  public Guid? RegistrationToken { get; set; }
  public DateTime? TokenExpiry { get; set; }
  public DateTime? TokenUsedAt { get; set; }
  public DateTime? ApprovedAt { get; set; }
  public DateTime? RejectedAt { get; set; }
  public string? RejectionReason { get; set; }
  public Guid? ApprovedByUserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // ── Domain Methods & Invariant Rules ────────────────────────────────
  public void Approve(Guid approvedByUserId, Guid registrationToken, TimeSpan? tokenValidity = null)
  {
    if (Status != LeadStatus.Pending)
      throw new Exceptions.DomainException($"Cannot approve lead with status '{Status}'. Only Pending leads can be approved.");

    if (approvedByUserId == Guid.Empty)
      throw new ArgumentException("ApprovedByUserId cannot be empty.", nameof(approvedByUserId));

    if (registrationToken == Guid.Empty)
      throw new ArgumentException("RegistrationToken cannot be empty.", nameof(registrationToken));

    Status = LeadStatus.Approved;
    ApprovedByUserId = approvedByUserId;
    ApprovedAt = DateTime.UtcNow;
    RegistrationToken = registrationToken;
    TokenExpiry = DateTime.UtcNow.Add(tokenValidity ?? TimeSpan.FromDays(7));
    UpdatedAt = DateTime.UtcNow;
  }

  public void Reject(string reason)
  {
    if (Status != LeadStatus.Pending)
      throw new Exceptions.DomainException($"Cannot reject lead with status '{Status}'. Only Pending leads can be rejected.");

    if (string.IsNullOrWhiteSpace(reason))
      throw new Exceptions.DomainException("Rejection reason is required.");

    Status = LeadStatus.Rejected;
    RejectionReason = reason.Trim();
    RejectedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public void MarkTokenUsed()
  {
    if (Status != LeadStatus.Approved)
      throw new Exceptions.DomainException($"Cannot mark registration token as used for lead with status '{Status}'. Only Approved leads can use tokens.");

    if (!TokenExpiry.HasValue || TokenExpiry.Value <= DateTime.UtcNow)
      throw new Exceptions.DomainException("Registration token has expired.");

    Status = LeadStatus.Completed;
    TokenUsedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public bool IsTokenValid(DateTime? now = null)
  {
    var current = now ?? DateTime.UtcNow;
    return Status == LeadStatus.Approved &&
           RegistrationToken.HasValue &&
           TokenExpiry.HasValue &&
           TokenExpiry.Value > current &&
           !TokenUsedAt.HasValue;
  }
}