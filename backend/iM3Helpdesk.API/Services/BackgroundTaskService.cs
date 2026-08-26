using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Services;

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

public class BackgroundTaskService : IBackgroundTaskService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundTaskService> _logger;

    public BackgroundTaskService(
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundTaskService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task QueueActivityAsync(Guid userId, Guid orgId, string action,
        string description, string entityType, Guid? entityId = null)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var log = new ActivityLog
            {
                Action = action,
                Description = description,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                OrganizationId = orgId
            };
            context.ActivityLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to queue activity");
        }
    }

    public async Task QueueNotificationAsync(Guid userId, Guid orgId, string title,
        string message, string type = "info", Guid? ticketId = null)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                UserId = userId,
                OrganizationId = orgId,
                TicketId = ticketId
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to queue notification");
        }
    }

    public async Task ProcessQueueAsync()
    {
        // Notifications and activities are written directly to DB
        // This method can be extended for retry logic in future
    }
}
