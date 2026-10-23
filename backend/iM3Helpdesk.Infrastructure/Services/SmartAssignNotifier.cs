using iM3Helpdesk.Infrastructure.Hubs;
using iM3Helpdesk.Application.Contracts.Services;
using Microsoft.AspNetCore.SignalR;

namespace iM3Helpdesk.Infrastructure.Services;

public class SmartAssignNotifier : ISmartAssignNotifier
{
    private readonly IHubContext<SmartAssignHub> _hubContext;
    private readonly ILogger<SmartAssignNotifier> _logger;

    public SmartAssignNotifier(IHubContext<SmartAssignHub> hubContext, ILogger<SmartAssignNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifySmartAssignedAsync(Guid orgId, Guid ticketId, int ticketNumber, string title, Guid agentId, string agentName, double confidence, bool isAuto)
    {
        try
        {
            await _hubContext.Clients.Group($"org_{orgId}").SendAsync("TicketSmartAssigned", new
            {
                ticketId,
                ticketNumber,
                title,
                agentId,
                agentName,
                confidence = Math.Round(confidence * 100.0, 1),
                isAuto,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast TicketSmartAssigned signal for Org {OrgId}", orgId);
        }
    }

    public async Task NotifySuggestionGeneratedAsync(Guid orgId, Guid ticketId, int ticketNumber, string title, object recommendations)
    {
        try
        {
            await _hubContext.Clients.Group($"org_{orgId}").SendAsync("SmartAssignSuggestion", new
            {
                ticketId,
                ticketNumber,
                title,
                recommendations,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast SmartAssignSuggestion signal for Org {OrgId}", orgId);
        }
    }

    public async Task NotifyModelRetrainedAsync(Guid orgId, string version, double accuracy, DateTime trainedAt)
    {
        try
        {
            await _hubContext.Clients.Group($"org_{orgId}").SendAsync("ModelRetrained", new
            {
                version,
                accuracy = Math.Round(accuracy, 1),
                trainedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast ModelRetrained signal for Org {OrgId}", orgId);
        }
    }

    public async Task NotifyStatsUpdatedAsync(Guid orgId)
    {
        try
        {
            await _hubContext.Clients.Group($"org_{orgId}").SendAsync("StatsUpdated", new
            {
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast StatsUpdated signal for Org {OrgId}", orgId);
        }
    }
}

