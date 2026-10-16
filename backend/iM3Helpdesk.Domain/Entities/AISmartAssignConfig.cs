using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class AISmartAssignConfig : IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Mode { get; set; } = "Suggest"; // "Auto" or "Suggest"
    public double ConfidenceThreshold { get; set; } = 0.75;
    public double WeightMl { get; set; } = 0.60;
    public double WeightWorkload { get; set; } = 0.20;
    public double WeightAvailability { get; set; } = 0.20;
    public int AutoRetrainFrequencyDays { get; set; } = 7;
    public int MinTrainingSamples { get; set; } = 50;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
}

