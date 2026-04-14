using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Hubs;

public class ChatHub : Hub
{
  private readonly ApplicationDbContext _context;

  public ChatHub(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task JoinTicketRoom(string ticketId)
  {
    await Groups.AddToGroupAsync(
        Context.ConnectionId, $"ticket-{ticketId}");
  }

  public async Task LeaveTicketRoom(string ticketId)
  {
    await Groups.RemoveFromGroupAsync(
        Context.ConnectionId, $"ticket-{ticketId}");
  }

  public async Task SendMessage(
      string ticketId, string message,
      string senderName, bool isAgent)
  {
    var msg = new
    {
      ticketId,
      message,
      senderName,
      isAgent,
      timestamp = DateTime.UtcNow
    };

    await Clients.Group($"ticket-{ticketId}")
        .SendAsync("ReceiveMessage", msg);
  }

  public async Task JoinOrgRoom(string orgId)
  {
    await Groups.AddToGroupAsync(
        Context.ConnectionId, $"org-{orgId}");
  }

  public async Task NotifyNewTicket(string orgId, object ticket)
  {
    await Clients.Group($"org-{orgId}")
        .SendAsync("NewTicket", ticket);
  }
}
