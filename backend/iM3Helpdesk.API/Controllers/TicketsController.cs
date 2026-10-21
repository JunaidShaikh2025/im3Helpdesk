using System.Security.Claims;
using iM3Helpdesk.API.Middleware;
using iM3Helpdesk.API.Services;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Tickets;
using iM3Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        ITicketService ticketService,
        ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    [HttpGet("my-status-counts")]
    public async Task<IActionResult> GetMyStatusCounts(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.GetMyStatusCountsAsync(userId, GetRoleClaim(), ct);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var tickets = await _ticketService.GetAllAsync(ct);
        return Ok(tickets);
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarTickets(
        [FromQuery] DateOnly start,
        [FromQuery] DateOnly end,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _ticketService.GetCalendarTicketsAsync(start, end, userId, GetRoleClaim(), ct);
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var ticket = await _ticketService.GetByIdAsync(id, GetRoleClaim(), ct);
        if (ticket == null) return NotFound();

        return Ok(ticket);
    }

    [HttpPost]
    [RequirePermission("tickets", PermissionAction.Add)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketDto dto,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.CreateAsync(dto, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new
        {
            message = result.Data!.Message,
            id = result.Data.Id,
            ticketNumber = result.Data.TicketNumber
        });
    }

    [HttpPut("{id}")]
    [RequirePermission("tickets", PermissionAction.Edit)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTicketDto dto,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.UpdateAsync(id, dto, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Updated successfully" });
    }

    [HttpDelete("{id}")]
    [RequirePermission("tickets", PermissionAction.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _ticketService.DeleteAsync(id, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Moved to recycle bin" });
    }

    [HttpGet("{id}/duplicates")]
    public async Task<IActionResult> GetDuplicates(Guid id, CancellationToken ct = default)
    {
        var result = await _ticketService.GetDuplicatesAsync(id, ct);
        return ToActionResult(result);
    }

    [HttpPost("{id}/merge")]
    public async Task<IActionResult> Merge(
        Guid id,
        [FromBody] MergeIntoDto dto,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _ticketService.MergeAsync(id, dto, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new
        {
            message = result.Data!.Message,
            originalId = result.Data.OriginalId,
            duplicateId = result.Data.DuplicateId,
            duplicateTicketNumber = result.Data.DuplicateTicketNumber
        });
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct = default)
    {
        var logs = await _ticketService.GetTimelineAsync(id, ct);
        return Ok(logs);
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> RecordView(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Ok();

        await _ticketService.RecordViewAsync(id, userId, ct);
        return Ok();
    }

    [HttpGet("{id}/viewers")]
    public async Task<IActionResult> GetViewers(Guid id, CancellationToken ct = default)
    {
        var viewers = await _ticketService.GetViewersAsync(id, ct);
        return Ok(viewers);
    }

    [HttpGet("{id}/watchers")]
    public async Task<IActionResult> GetWatchers(Guid id, CancellationToken ct = default)
    {
        var result = await _ticketService.GetWatchersAsync(id, ct);
        return ToActionResult(result);
    }

    [HttpPost("{id}/watchers/me")]
    public async Task<IActionResult> AddMeAsWatcher(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.AddMeAsWatcherAsync(id, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Now watching ticket" });
    }

    [HttpPost("{id}/watchers")]
    public async Task<IActionResult> AddWatcher(
        Guid id,
        [FromBody] AddWatcherDto dto,
        CancellationToken ct = default)
    {
        var actorId = GetUserId();
        if (actorId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.AddWatcherAsync(id, dto, actorId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Watcher added" });
    }

    [HttpDelete("{id}/watchers/{userId}")]
    public async Task<IActionResult> RemoveWatcher(
        Guid id,
        Guid userId,
        CancellationToken ct = default)
    {
        var actorId = GetUserId();
        if (actorId == Guid.Empty) return Unauthorized();

        var result = await _ticketService.RemoveWatcherAsync(id, userId, actorId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = "Watcher removed" });
    }

    [HttpPost("bulk-update")]
    [RequirePermission("tickets", PermissionAction.Edit)]
    public async Task<IActionResult> BulkUpdate(
        [FromBody] BulkUpdateDto dto,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _ticketService.BulkUpdateAsync(dto, userId, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        return Ok(new { message = result.Data!.Message });
    }

    [HttpGet("export")]
    [RequirePermission("tickets", PermissionAction.Export)]
    public async Task<IActionResult> Export(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        CancellationToken ct = default)
    {
        var bytes = await _ticketService.ExportCsvAsync(status, priority, ct);
        return File(bytes, "text/csv", $"tickets-{DateTime.Now:yyyy-MM-dd}.csv");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? category,
        CancellationToken ct = default)
    {
        var results = await _ticketService.SearchAsync(query, status, priority, category, ct);
        return Ok(results);
    }

    [HttpGet("by-tag/{tag}")]
    public async Task<IActionResult> GetByTag(string tag, CancellationToken ct = default)
    {
        var tickets = await _ticketService.GetByTagAsync(tag, ct);
        return Ok(tickets);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }

    private string? GetRoleClaim()
    {
        return User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
            ?? User.FindFirst("role")?.Value;
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return Ok();

        return result.StatusCode switch
        {
            404 => NotFound(new { message = result.ErrorMessage }),
            401 => Unauthorized(),
            403 => Forbid(),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return result.StatusCode switch
        {
            404 => NotFound(new { message = result.ErrorMessage }),
            401 => Unauthorized(),
            403 => Forbid(),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }
}
