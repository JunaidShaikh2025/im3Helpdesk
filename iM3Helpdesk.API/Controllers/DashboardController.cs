using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public DashboardController(
        ApplicationDbContext context,
        ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var tickets = await _context.Tickets.ToListAsync();
        var users = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.OrganizationId == _tenantService.OrganizationId)
            .ToListAsync();

        var stats = new
        {
            totalTickets = tickets.Count,
            openTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            inProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
            resolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
            closedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
            totalAgents = users.Count(u => u.Role == UserRole.Agent),
            totalAdmins = users.Count(u => u.Role == UserRole.CompanyAdmin),
            recentTickets = await _context.Tickets
            .Include(t => t.CreatedBy)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new
            {
                t.Id,
                t.Title,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                t.CreatedAt,
                CreatedBy = t.CreatedBy!.FullName
            })
            .ToListAsync()
        };

        return Ok(stats);
    }
}