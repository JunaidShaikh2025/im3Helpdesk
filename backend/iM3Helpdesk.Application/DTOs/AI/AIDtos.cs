namespace iM3Helpdesk.Application.DTOs.AI;

public class MergeTicketsDto
{
    public Guid OriginalTicketId { get; set; }
    public List<Guid> TicketIdsToClose { get; set; } = new();
    public string? Note { get; set; }
}

public class MergeTicketsResultDto
{
    public string Message { get; set; } = string.Empty;
    public Guid OriginalId { get; set; }
    public int MergedCount { get; set; }
}

public class TideForecastDto
{
    public object Forecast { get; set; } = new();
    public object Historical { get; set; } = new();
    public double AvgDailyActual { get; set; }
    public int TotalLast90Days { get; set; }
}

public class AiInsightsDto
{
    public object Insights { get; set; } = new();
    public object Summary { get; set; } = new();
}

public class DuplicateDetectionDto
{
    public List<DuplicateGroupDto> Groups { get; set; } = new();
    public int TotalDuplicates { get; set; }
    public int PotentialSavings { get; set; }
}

public class DuplicateGroupDto
{
    public Guid GroupId { get; set; }
    public int Count { get; set; }
    public List<DuplicateTicketItemDto> Tickets { get; set; } = new();
    public string Confidence { get; set; } = string.Empty;
}

public class DuplicateTicketItemDto
{
    public Guid Id { get; set; }
    public int TicketNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOriginal { get; set; }
    public int Similarity { get; set; }
}

public class TicketSummaryDto
{
    public Guid TicketId { get; set; }
    public string OneLiner { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public List<string> KeyFacts { get; set; } = new();
    public object Stats { get; set; } = new();
    public string? AssignedTo { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class HotTopicsDto
{
    public object Period { get; set; } = new();
    public object Totals { get; set; } = new();
    public object TopCategories { get; set; } = new();
    public object TopTicketTypes { get; set; } = new();
    public object TopPriorities { get; set; } = new();
    public object TopTags { get; set; } = new();
    public object TopKeywords { get; set; } = new();
    public object SlowestCategories { get; set; } = new();
    public object AiInsights { get; set; } = new();
}
