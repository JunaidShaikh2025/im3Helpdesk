using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace iM3Helpdesk.API.Hubs;

[Authorize]
public class SmartAssignHub : Hub
{
    public async Task JoinOrgGroup(string orgId)
    {
        if (!string.IsNullOrWhiteSpace(orgId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"org_{orgId}");
        }
    }

    public async Task LeaveOrgGroup(string orgId)
    {
        if (!string.IsNullOrWhiteSpace(orgId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"org_{orgId}");
        }
    }
}

