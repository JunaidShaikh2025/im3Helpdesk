using Microsoft.ML.Data;

namespace iM3Helpdesk.Infrastructure.ML;

public class TicketTrainingInput
{
    [LoadColumn(0)]
    public string Title { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string Description { get; set; } = string.Empty;

    [LoadColumn(2)]
    public string Category { get; set; } = "General";

    [LoadColumn(3)]
    public string Priority { get; set; } = "Medium";

    [LoadColumn(4)]
    public string TicketType { get; set; } = "Support";

    [LoadColumn(5)]
    public string Tags { get; set; } = string.Empty;

    [LoadColumn(6)]
    public string AssignedToUserId { get; set; } = string.Empty;
}

public class AgentPredictionOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedAgentId { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();
}

public class AgentClassProbability
{
    public string AgentId { get; set; } = string.Empty;
    public double Probability { get; set; }
}

