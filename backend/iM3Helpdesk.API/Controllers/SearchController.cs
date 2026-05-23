using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
  private readonly ApplicationDbContext _context;
  private readonly ICurrentTenantService _tenant;

  public SearchController(
      ApplicationDbContext context,
      ICurrentTenantService tenant)
  {
    _context = context;
    _tenant = tenant;
  }

  [HttpGet]
  [Authorize(Roles = nameof(UserRole.SuperAdmin) + "," + nameof(UserRole.CompanyAdmin) + "," + nameof(UserRole.Agent))]
  public async Task<IActionResult> GlobalSearch([FromQuery] string q)
  {
    if (!_tenant.OrganizationId.HasValue)
      return Forbid();

    if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
      return Ok(new
      {
        tickets = new List<object>(),
        agents = new List<object>(),
        articles = new List<object>()
      });

    var tickets = await _context.Tickets
        .Include(t => t.CreatedBy)
        .Where(t => t.Title.Contains(q) ||
            t.Description.Contains(q) ||
            t.Category.Contains(q))
        .Take(5)
        .Select(t => new
        {
          t.Id,
          t.Title,
          Status = t.Status.ToString(),
          Type = "ticket"
        })
        .ToListAsync();

    var agents = await _context.Users
        .Where(u => (u.FullName.Contains(q) ||
            u.Email.Contains(q)) &&
        u.OrganizationId == _tenant.OrganizationId &&
        (u.Role == UserRole.Agent ||
         u.Role == UserRole.CompanyAdmin))
        .Take(5)
        .Select(u => new
        {
          u.Id,
          Name = u.FullName,
          u.Email,
          Type = "agent"
        })
        .ToListAsync();

    var articles = await _context.KbArticles
        .Where(a => a.IsPublished &&
            (a.Title.Contains(q) ||
            a.Tags.Contains(q)))
        .Take(5)
        .Select(a => new
        {
          a.Id,
          a.Title,
          a.Category,
          Type = "article"
        })
        .ToListAsync();

    return Ok(new { tickets, agents, articles });
  }
}
