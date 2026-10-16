namespace iM3Helpdesk.Application.DTOs.SmartAssign;

public class AgentRecommendationDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public string? AgentPhotoUrl { get; set; }
    public double CompositeScore { get; set; }
    public double MlConfidenceScore { get; set; }
    public double WorkloadScore { get; set; }
    public double AvailabilityScore { get; set; }
    public int MatchRank { get; set; }
    public List<string> Reasons { get; set; } = new();
    public int ActiveTicketsCount { get; set; }
    public bool IsOnline { get; set; }
    public int CategoryResolvedCount { get; set; }
    public double CategoryAvgHours { get; set; }
    public double CategorySlaRate { get; set; }
}

public class TicketSmartAssignResultDto
{
    public Guid TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Priority { get; set; } = "Medium";
    public string TicketType { get; set; } = "Support";
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid? CurrentAssignedAgentId { get; set; }
    public string? CurrentAssignedAgentName { get; set; }
    public string Mode { get; set; } = "Suggest";
    public bool IsAutoAssigned { get; set; }
    public List<AgentRecommendationDto> Recommendations { get; set; } = new();
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

public class SmartAssignStatsDto
{
    public int UnassignedTicketsCount { get; set; }
    public int AutoAssignedToday { get; set; }
    public int AutoAssignedTotal { get; set; }
    public double ModelAccuracy { get; set; }
    public double ModelTop3Accuracy { get; set; }
    public double OverrideRate { get; set; }
    public int TotalDecisions { get; set; }
    public int OverrideCount { get; set; }
    public string ActiveModelVersion { get; set; } = "None";
    public DateTime? LastTrainedAt { get; set; }
    public int TrainingSamplesCount { get; set; }
    public string Mode { get; set; } = "Suggest";
    public double ConfidenceThreshold { get; set; } = 0.75;
    public double WeightMl { get; set; } = 0.60;
    public double WeightWorkload { get; set; } = 0.20;
    public double WeightAvailability { get; set; } = 0.20;
    public List<AgentWorkloadSummaryDto> AgentWorkloads { get; set; } = new();
}

public class AgentWorkloadSummaryDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int ActiveTicketCount { get; set; }
    public int HighPriorityTicketCount { get; set; }
    public int AssignedTodayCount { get; set; }
    public bool IsOnline { get; set; }
    public string? TopCategory { get; set; }
}

public class ApplySmartAssignDto
{
    public Guid AgentId { get; set; }
    public string? Reason { get; set; }
}

public class UpdateSmartAssignConfigDto
{
    public string? Mode { get; set; } // "Auto" or "Suggest"
    public double? ConfidenceThreshold { get; set; }
    public double? WeightMl { get; set; }
    public double? WeightWorkload { get; set; }
    public double? WeightAvailability { get; set; }
}

public class RetrainResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Top3Accuracy { get; set; }
    public int SamplesCount { get; set; }
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
}

