namespace iM3Helpdesk.Application.Contracts.Services;

/// <summary>
/// Background task queue for notifications, activities, and emails
/// Allows endpoints to return immediately while background work continues
/// </summary>
public interface IBackgroundTaskService
{
    Task QueueActivityAsync(Guid userId, Guid orgId, string action, 
        string description, string entityType, Guid? entityId = null);
    
    Task QueueNotificationAsync(Guid userId, Guid orgId, string title,
        string message, string type = "info", Guid? ticketId = null);

    Task ProcessQueueAsync();
}
