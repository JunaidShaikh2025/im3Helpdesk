using System.Security.Claims;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.SmartAssign;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/ai/smart-assign")]
[Authorize]
public class SmartAssignController : ControllerBase
{
    private readonly ISmartAssignService _smartAssignService;
    private readonly ICurrentTenantService _tenantService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SmartAssignController> _logger;

    public SmartAssignController(
        ISmartAssignService smartAssignService,
        ICurrentTenantService tenantService,
        IWebHostEnvironment env,
        ILogger<SmartAssignController> logger)
    {
        _smartAssignService = smartAssignService;
        _tenantService = tenantService;
        _env = env;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var c = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(c, out var id);
        return id;
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetRecommendation(Guid ticketId)
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var result = await _smartAssignService.EvaluateTicketAsync(ticketId, orgId.Value, allowAutoAssign: false);
        if (result == null) return NotFound(new { message = "Ticket not found or no agents available" });

        return Ok(result);
    }

    [HttpPost("{ticketId:guid}/apply")]
    public async Task<IActionResult> ApplyAssignment(Guid ticketId, [FromBody] ApplySmartAssignDto dto)
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var success = await _smartAssignService.ApplyAssignmentAsync(ticketId, dto.AgentId, userId, orgId.Value, dto.Reason);
        if (!success) return BadRequest(new { message = "Failed to apply ticket assignment" });

        return Ok(new { message = "Ticket assigned successfully" });
    }

    [HttpPost("retrain")]
    public async Task<IActionResult> RetrainModel()
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var result = await _smartAssignService.RetrainModelAsync(orgId.Value, webRoot);

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var stats = await _smartAssignService.GetStatsAsync(orgId.Value);
        return Ok(stats);
    }

    [HttpGet("unassigned")]
    public async Task<IActionResult> GetUnassignedQueue([FromQuery] int limit = 20)
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var queue = await _smartAssignService.GetUnassignedQueueAsync(orgId.Value, limit);
        return Ok(queue);
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var config = await _smartAssignService.GetOrCreateConfigAsync(orgId.Value);
        return Ok(config);
    }

    [HttpPost("config")]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateSmartAssignConfigDto dto)
    {
        var orgId = _tenantService.OrganizationId;
        if (!orgId.HasValue) return BadRequest(new { message = "Tenant context required" });

        var config = await _smartAssignService.UpdateConfigAsync(orgId.Value, dto);
        return Ok(config);
    }
}

