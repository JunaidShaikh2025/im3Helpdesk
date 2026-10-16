using iM3Helpdesk.Domain.Interfaces;

namespace iM3Helpdesk.Domain.Entities;

public class AIModelVersion : IMustHaveTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Algorithm { get; set; } = "SdcaMaximumEntropy";
    public double Accuracy { get; set; }
    public double Top3Accuracy { get; set; }
    public int TrainingSamplesCount { get; set; }
    public string ModelFilePath { get; set; } = string.Empty;
    public string MetricsJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
}

