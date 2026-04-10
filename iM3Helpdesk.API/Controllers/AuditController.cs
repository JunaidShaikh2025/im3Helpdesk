using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public AuditController(ApplicationDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<IActionResult> GetAuditLog(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      [FromQuery] string? entityType = null,
      [FromQuery] DateTime? from = null,
      [FromQuery] DateTime? to = null)
  {
    var query = _context.ActivityLogs
        .Include(a => a.User)
        .AsQueryable();

    if (!string.IsNullOrEmpty(entityType))
      query = query.Where(a => a.EntityType == entityType);

    if (from.HasValue)
      query = query.Where(a => a.CreatedAt >= from.Value);

    if (to.HasValue)
      query = query.Where(a => a.CreatedAt <= to.Value);

    var total = await query.CountAsync();

    var logs = await query
        .OrderByDescending(a => a.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(a => new
        {
          a.Id,
          a.Action,
          a.Description,
          a.EntityType,
          a.EntityId,
          a.CreatedAt,
          User = a.User == null ? "System" : a.User.FullName
        })
        .ToListAsync();

    return Ok(new
    {
      logs,
      total,
      page,
      pageSize,
      totalPages = (int)Math.Ceiling((double)total / pageSize)
    });
  }
}
