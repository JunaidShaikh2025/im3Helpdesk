using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class AIAssignmentLog : IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Guid SuggestedAgentId { get; set; }
    public Guid? FinalAgentId { get; set; }
    public double ConfidenceScore { get; set; }
    public double MlScore { get; set; }
    public double WorkloadScore { get; set; }
    public double AvailabilityScore { get; set; }
    public string ReasonJson { get; set; } = "[]";
    public string Mode { get; set; } = "Suggest"; // "Auto" or "Suggest"
    public bool WasAutoAssigned { get; set; } = false;
    public bool IsOverridden { get; set; } = false;
    public Guid? OverriddenByUserId { get; set; }
    public DateTime? OverriddenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid OrganizationId { get; set; }

    public Ticket? Ticket { get; set; }
    public User? SuggestedAgent { get; set; }
    public User? FinalAgent { get; set; }
    public User? OverriddenByUser { get; set; }
    public Organization? Organization { get; set; }
}

