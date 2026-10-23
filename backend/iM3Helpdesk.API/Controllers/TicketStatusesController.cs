using iM3Helpdesk.API.Common.Helpers;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.DTOs.Tickets;
using iM3Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/Tickets")]
[Authorize]
public class TicketStatusesController : TicketsControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ISlaService _slaService;
    private readonly ILogger<TicketStatusesController> _logger;
    private readonly IBackgroundTaskService _backgroundTaskService;

    public TicketStatusesController(
        IUnitOfWork uow,
        INotificationService notificationService,
        IEmailService emailService,
        ISlaService slaService,
        ILogger<TicketStatusesController> logger,
        IBackgroundTaskService backgroundTaskService)
        : base(uow)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _slaService = slaService;
        _logger = logger;
        _backgroundTaskService = backgroundTaskService;
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateStatusDto dto)
    {
        var ticket = await _uow.Tickets.GetTicketWithCreatedByAsync(id);

        if (ticket == null) return NotFound();
        var statusStr = dto.Status?.Trim() ?? "";
        if (!TicketEnumHelpers.TryParseTicketStatus(
            statusStr, out var newStatus))
            return BadRequest(new
            {
                message = $"Invalid status: {statusStr}"
            });

        if (!await IsMasterValueAllowedAsync(
            TicketEnumHelpers.TicketStatusField,
            newStatus.ToString()))
        {
            return BadRequest(new
            {
                message = $"Status {newStatus} is not active in ticket master"
            });
        }

        ticket.ChangeStatus(newStatus);

        // Save core data immediately
        await _uow.SaveChangesAsync();

        // Queue background tasks (fire-and-forget)
        var userId = GetUserId();
        var ticketId = ticket.Id;
        var createdByEmail = ticket.CreatedBy?.Email;
        var orgId = ticket.OrganizationId;
        var ticketTitle = ticket.Title;
        var ticketNumber = ticket.TicketNumber;

        _ = Task.Run(async () =>
        {
            try
            {
                if (userId != Guid.Empty)
                {
                    await _backgroundTaskService.QueueActivityAsync(
                        userId, orgId,
                        "StatusChanged",
                        $"Status â†’ {newStatus}: {ticketTitle}",
                        "Ticket", ticketId);
                }

                if (createdByEmail != null)
                {
                    try
                    {
                        var html = $@"
        <div style='font-family:Arial;max-width:600px'>
          <p>Your ticket <strong>{ticketTitle}</strong>
          (#TN{ticketNumber}) status has been 
          updated to <strong>{newStatus}</strong>.</p>
        </div>";
                        await _emailService.SendAsync(
                            createdByEmail,
                            $"Ticket #{ticketNumber} Status: {newStatus}",
                            html,
                            organizationId: orgId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Status email failed for ticket {TicketId}", ticketId);
                    }
                }

                await NotifyWatchersAndAssigneeAsync(
                    ticket,
                    userId,
                    "Ticket status updated",
                    $"Status changed to {newStatus} on #TN{ticketNumber}: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background task failed for ticket status update {TicketId}", ticketId);
            }
        });

        return Ok(new { message = "Status updated" });
    }

    [HttpPut("{id}/priority")]
    public async Task<IActionResult> UpdatePriority(
        Guid id, [FromBody] UpdatePriorityDto dto)
    {
        var ticket = await _uow.Tickets.GetByIdAsync(id, false);
        if (ticket == null) return NotFound();

        if (!TicketEnumHelpers.TryParseTicketPriority(
            dto.Priority, out var newP))
            return BadRequest();

        if (!await IsMasterValueAllowedAsync(
            TicketEnumHelpers.TicketPriorityField,
            newP.ToString()))
        {
            return BadRequest(new
            {
                message = $"Priority {newP} is not active in ticket master"
            });
        }

        ticket.ChangePriority(newP);
        ticket.SlaDeadline = await _slaService
            .CalculateSlaDeadlineAsync(
                ticket.OrganizationId, ticket.Priority, ticket.CreatedAt, ticket.AgentGroupId);
        ticket.SlaStatus = _slaService
            .GetSlaStatus(
                ticket.SlaDeadline, ticket.Status);

        await _uow.SaveChangesAsync();
        return Ok(new { message = "Priority updated" });
    }

    [HttpPut("{id}/type")]
    public async Task<IActionResult> UpdateType(
        Guid id, [FromBody] UpdateTypeDto dto)
    {
        var ticket = await _uow.Tickets.GetByIdAsync(id, false);
        if (ticket == null) return NotFound();

        var normalizedTicketType = dto.TicketType.Trim();
        if (!await IsMasterValueAllowedAsync(
            TicketEnumHelpers.TicketTypeField,
            normalizedTicketType))
        {
            return BadRequest(new
            {
                message = $"Ticket Type {normalizedTicketType} is not active in ticket master"
            });
        }

        ticket.TicketType = normalizedTicketType;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return Ok(new { message = "Type updated" });
    }

    [HttpPut("{id}/tags")]
    public async Task<IActionResult> UpdateTags(
        Guid id, [FromBody] UpdateTagsDto dto)
    {
        var ticket = await _uow.Tickets.GetByIdAsync(id, false);
        if (ticket == null) return NotFound();

        ticket.Tags = string.Join(",",
            dto.Tags
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct());
        ticket.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return Ok(new
        {
            message = "Tags updated",
            tags = ticket.Tags
        });
    }

    [HttpPut("{id}/log-time")]
    public async Task<IActionResult> LogTime(
        Guid id, [FromBody] LogTimeDto dto)
    {
        var ticket = await _uow.Tickets.GetByIdAsync(id, false);
        if (ticket == null) return NotFound();

        ticket.LogTime(dto.Minutes);
        await _uow.SaveChangesAsync();

        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            await _notificationService.CreateActivityAsync(
                userId, ticket.OrganizationId,
                "TimeLogged",
                $"Logged {dto.Minutes} min: {ticket.Title}",
                "Ticket", ticket.Id);
        }

        return Ok(new
        {
            message = "Time logged",
            totalMinutes = ticket.TimeSpentMinutes,
            totalHours = Math.Round(
                ticket.TimeSpentMinutes / 60.0, 1)
        });
    }

    private async Task NotifyWatchersAndAssigneeAsync(
        Domain.Entities.Ticket ticket,
        Guid actorUserId,
        string title,
        string message)
    {
        var recipients = new HashSet<Guid>();
        if (ticket.AssignedToUserId.HasValue)
            recipients.Add(ticket.AssignedToUserId.Value);

        var watcherIds = await _uow.Tickets.GetWatcherUserIdsAsync(ticket.Id);

        foreach (var watcherId in watcherIds)
            recipients.Add(watcherId);

        recipients.Remove(actorUserId);
        if (recipients.Count == 0) return;

        foreach (var recipientId in recipients)
        {
            try
            {
                await _notificationService.CreateAsync(
                    recipientId,
                    ticket.OrganizationId,
                    title,
                    message,
                    "info",
                    ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Watcher notification failed for {UserId}", recipientId);
            }
        }

        var emailRecipients = await _uow.Tickets.GetUsersByIdsAsync(recipients);

        foreach (var recipient in emailRecipients)
        {
            if (string.IsNullOrWhiteSpace(recipient.Email)) continue;

            try
            {
                await _emailService.SendAsync(
                    recipient.Email,
                    title,
                    $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>",
                    organizationId: ticket.OrganizationId,
                    ticketNumberTag: $"#TN{ticket.TicketNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Watcher email failed for {Email}", recipient.Email);
            }
        }
    }
}
