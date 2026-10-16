using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class AIAgentSkillStat : IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid AgentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int TotalResolved { get; set; }
    public double AvgResolutionTimeHours { get; set; }
    public double SlaComplianceRate { get; set; }
    public double ReopenRate { get; set; }
    public DateTime? LastResolvedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? Agent { get; set; }
    public Organization? Organization { get; set; }
}

