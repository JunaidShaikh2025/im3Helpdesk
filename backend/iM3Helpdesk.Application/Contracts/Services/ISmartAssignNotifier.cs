namespace iM3Helpdesk.Application.Contracts.Services;

public interface ISmartAssignNotifier
{
    Task NotifySmartAssignedAsync(Guid orgId, Guid ticketId, int ticketNumber, string title, Guid agentId, string agentName, double confidence, bool isAuto);
    Task NotifySuggestionGeneratedAsync(Guid orgId, Guid ticketId, int ticketNumber, string title, object recommendations);
    Task NotifyModelRetrainedAsync(Guid orgId, string version, double accuracy, DateTime trainedAt);
    Task NotifyStatsUpdatedAsync(Guid orgId);
}

