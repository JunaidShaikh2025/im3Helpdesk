using System.Security.Claims;
using iM3Helpdesk.API.Middleware;
using iM3Helpdesk.Infrastructure.Services;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.DTOs.Agents;
using iM3Helpdesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(
        IAgentService agentService,
        ICurrentTenantService tenantService,
        ILogger<AgentsController> logger)
    {
        _agentService = agentService;
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var orgId = await ResolveOrganizationIdFromContextAsync(ct);
        var isSuperAdmin = _tenantService.IsSuperAdmin || User.IsInRole(nameof(UserRole.SuperAdmin));

        var agents = await _agentService.GetAllAsync(orgId, isSuperAdmin, ct);
        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var orgId = await ResolveOrganizationIdFromContextAsync(ct);
        var isSuperAdmin = _tenantService.IsSuperAdmin || User.IsInRole(nameof(UserRole.SuperAdmin));

        var result = await _agentService.GetByIdAsync(id, orgId, isSuperAdmin, ct);
        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Data);
    }

    [HttpPost("invite")]
    [Authorize(Roles = nameof(UserRole.CompanyAdmin) + "," + nameof(UserRole.SuperAdmin))]
    [RequirePermission("agents", PermissionAction.Add)]
    public async Task<IActionResult> InviteAgent([FromBody] InviteAgentDto dto, CancellationToken ct = default)
    {
        var result = await _agentService.InviteAgentAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = result.Data!.Message,
            tempPassword = result.Data.TempPassword,
            agentId = result.Data.AgentId
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRole.CompanyAdmin) + "," + nameof(UserRole.SuperAdmin))]
    [RequirePermission("agents", PermissionAction.Edit)]
    public async Task<IActionResult> UpdateAgent(Guid id, [FromBody] UpdateAgentDto dto, CancellationToken ct = default)
    {
        var result = await _agentService.UpdateAgentAsync(id, dto, ct);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Agent updated" });
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = nameof(UserRole.CompanyAdmin) + "," + nameof(UserRole.SuperAdmin))]
    [RequirePermission("agents", PermissionAction.Edit)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct = default)
    {
        var result = await _agentService.ToggleActiveAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new
        {
            message = result.Data!.Message,
            isActive = result.Data.IsActive
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.CompanyAdmin) + "," + nameof(UserRole.SuperAdmin))]
    [RequirePermission("agents", PermissionAction.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await _agentService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Agent deleted" });
    }

    [HttpPost("bulk-import")]
    [Authorize(Roles = nameof(UserRole.CompanyAdmin))]
    public async Task<IActionResult> BulkImport([FromForm] BulkImportUsersDto dto, CancellationToken ct = default)
    {
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest(new { message = "Please upload an Excel or CSV file." });

        var ext = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
            return BadRequest(new { message = "Only .xlsx and .csv files are supported." });

        await using var stream = dto.File.OpenReadStream();
        var result = await _agentService.BulkImportAsync(stream, ext, dto.SendInviteEmail, ct);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    private async Task<Guid?> ResolveOrganizationIdFromContextAsync(CancellationToken ct = default)
    {
        var claimValue =
            User.FindFirstValue("organizationId") ??
            User.FindFirstValue("OrganizationId") ??
            User.FindFirstValue("orgId") ??
            User.FindFirstValue("organizationid");

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(ClaimTypes.Sid) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("nameid");

        Guid? userId = null;
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        return await _agentService.ResolveOrganizationIdAsync(_tenantService.OrganizationId, claimValue, userId, ct);
    }
}

public class BulkImportUsersDto
{
    public IFormFile? File { get; set; }
    public bool SendInviteEmail { get; set; } = true;
}
