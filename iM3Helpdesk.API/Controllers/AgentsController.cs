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
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public AgentsController(
        ApplicationDbContext context,
        ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.OrganizationId == _tenantService.OrganizationId
                && (u.Role == UserRole.Agent || u.Role == UserRole.CompanyAdmin))
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                Role = u.Role.ToString(),
                u.IsEmailVerified,
                u.CreatedAt,
                u.LastLoginAt
            })
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var agent = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id
                && u.OrganizationId == _tenantService.OrganizationId);

        if (agent == null)
            return NotFound(new { message = "Agent not found" });

        return Ok(new
        {
            agent.Id,
            agent.FullName,
            agent.Email,
            agent.PhoneNumber,
            Role = agent.Role.ToString(),
            agent.IsEmailVerified,
            agent.CreatedAt,
            agent.LastLoginAt
        });
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteAgent([FromBody] InviteAgentDto dto)
    {
        var existing = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existing != null)
            return BadRequest(new { message = "Email already registered" });

        var tempPassword = "Agent@" + Guid.NewGuid().ToString()[..6];
        var agent = new iM3Helpdesk.Domain.Entities.User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            Role = UserRole.Agent,
            OrganizationId = _tenantService.OrganizationId,
            IsEmailVerified = true
        };

        _context.Users.Add(agent);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Agent invited successfully",
            tempPassword,
            agentId = agent.Id
        });
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var agent = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id
                && u.OrganizationId == _tenantService.OrganizationId);

        if (agent == null)
            return NotFound(new { message = "Agent not found" });

        agent.Role = Enum.Parse<UserRole>(dto.Role);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Role updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var agent = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id
                && u.OrganizationId == _tenantService.OrganizationId);

        if (agent == null)
            return NotFound(new { message = "Agent not found" });

        _context.Users.Remove(agent);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Agent removed successfully" });
    }
}

public class InviteAgentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}