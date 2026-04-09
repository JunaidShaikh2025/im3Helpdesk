using iM3Helpdesk.API.DTOs.Tickets;
using iM3Helpdesk.API.Services;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly INotificationService _notificationService;

    public TicketsController(
        ApplicationDbContext context,
        ICurrentTenantService tenantService,
        INotificationService notificationService)
    {
        _context = context;
        _tenantService = tenantService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Category = t.Category,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedBy = t.CreatedBy!.FullName,
                AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                CreatedAt = t.CreatedAt,
                CommentsCount = t.Comments.Count
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
            return NotFound(new { message = "Ticket not found" });

        return Ok(new
        {
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Category,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            createdBy = new
            {
                ticket.CreatedBy!.FullName,
                ticket.CreatedBy.Email
            },
            assignedTo = ticket.AssignedTo == null ? null : new
            {
                ticket.AssignedTo.FullName,
                ticket.AssignedTo.Email
            },
            comments = ticket.Comments.Select(c => new
            {
                c.Id,
                c.Comment,
                c.CreatedAt,
                user = new { c.User!.FullName }
            }).OrderBy(c => c.CreatedAt).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user" });

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = Enum.Parse<TicketPriority>(dto.Priority),
            OrganizationId = _tenantService.OrganizationId!.Value,
            CreatedByUserId = userId,
            Status = TicketStatus.Open
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        await _notificationService.CreateActivityAsync(
            userId, _tenantService.OrganizationId!.Value,
            "Created", $"New ticket: {ticket.Title}",
            "Ticket", ticket.Id);

        return Ok(new { message = "Ticket created successfully", id = ticket.Id });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id,
        [FromBody] UpdateStatusDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.Status = Enum.Parse<TicketStatus>(dto.Status);
        ticket.UpdatedAt = DateTime.UtcNow;

        if (ticket.Status == TicketStatus.Resolved)
            ticket.ResolvedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var uid))
        {
            await _notificationService.CreateActivityAsync(
                uid, ticket.OrganizationId,
                "StatusChanged",
                $"Ticket '{ticket.Title}' status → {dto.Status}",
                "Ticket", ticket.Id);
        }

        return Ok(new { message = "Status updated" });
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id,
        [FromBody] AddCommentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user" });

        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound(new { message = "Ticket not found" });

        var comment = new TicketComment
        {
            Comment = dto.Comment,
            TicketId = id,
            UserId = userId,
            OrganizationId = _tenantService.OrganizationId!.Value
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();

        await _notificationService.CreateActivityAsync(
            userId, _tenantService.OrganizationId!.Value,
            "Commented",
            $"Comment added on ticket: {ticket.Title}",
            "Ticket", ticket.Id);

        return Ok(new { message = "Comment added" });
    }

    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignTicket(Guid id,
    [FromBody] AssignTicketDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        ticket.AssignedToUserId = dto.AgentId;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var uid))
        {
            await _notificationService.CreateActivityAsync(
                uid, ticket.OrganizationId,
                "Assigned", $"Ticket '{ticket.Title}' assigned to agent",
                "Ticket", ticket.Id);

            if (dto.AgentId.HasValue)
            {
                await _notificationService.CreateAsync(
                    dto.AgentId.Value, ticket.OrganizationId,
                    "Ticket Assigned",
                    $"You have been assigned ticket: {ticket.Title}",
                    "info", ticket.Id);
            }
        }

        return Ok(new { message = "Ticket assigned successfully" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? category)
    {
        var tickets = _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
            tickets = tickets.Where(t =>
                t.Title.Contains(query) ||
                t.Description.Contains(query));

        if (!string.IsNullOrEmpty(status) && status != "All")
            tickets = tickets.Where(t =>
                t.Status == Enum.Parse<TicketStatus>(status));

        if (!string.IsNullOrEmpty(priority) && priority != "All")
            tickets = tickets.Where(t =>
                t.Priority == Enum.Parse<TicketPriority>(priority));

        if (!string.IsNullOrEmpty(category) && category != "All")
            tickets = tickets.Where(t => t.Category == category);

        var result = await tickets
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Category = t.Category,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedBy = t.CreatedBy!.FullName,
                AssignedTo = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                CreatedAt = t.CreatedAt,
                CommentsCount = t.Comments.Count
            })
            .ToListAsync();

        return Ok(result);
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class AddCommentDto
{
    public string Comment { get; set; } = string.Empty;
}

public class AssignTicketDto
{
    public Guid? AgentId { get; set; }
}