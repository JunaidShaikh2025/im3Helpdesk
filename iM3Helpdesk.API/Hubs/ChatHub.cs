// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace iM3Helpdesk.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
  public async Task JoinTicket(string ticketId)
  {
    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        $"ticket-{ticketId}");
  }

  public async Task LeaveTicket(string ticketId)
  {
    await Groups.RemoveFromGroupAsync(
        Context.ConnectionId,
        $"ticket-{ticketId}");
  }

  public async Task SendMessage(
      string ticketId, string message)
  {
    var userId = Context.UserIdentifier
        ?? "Unknown";
    await Clients
        .Group($"ticket-{ticketId}")
        .SendAsync("ReceiveMessage", new
        {
          senderId = userId,
          message,
          timestamp = DateTime.UtcNow
                .ToString("o")
        });
  }
}
