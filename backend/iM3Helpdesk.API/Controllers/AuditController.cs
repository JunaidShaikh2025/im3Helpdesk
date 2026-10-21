using iM3Helpdesk.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[iM3Helpdesk.API.Middleware.RequireFeature("audit-log")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        CancellationToken ct = default)
    {
        var result = await _auditService.GetLogsAsync(page, pageSize, entityType, ct);
        return Ok(new
        {
            logs = result.Logs,
            total = result.Total,
            page = result.Page,
            totalPages = result.TotalPages
        });
    }
}
