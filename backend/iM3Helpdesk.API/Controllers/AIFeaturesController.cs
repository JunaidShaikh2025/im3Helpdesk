using System.Security.Claims;
using iM3Helpdesk.API.Middleware;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iM3Helpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireFeature("ai-insights")]
public class AIFeaturesController : ControllerBase
{
    private readonly IAIFeaturesService _aiService;
    private readonly ILogger<AIFeaturesController> _logger;

    public AIFeaturesController(
        IAIFeaturesService aiService,
        ILogger<AIFeaturesController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet("tide-forecast")]
    public async Task<IActionResult> GetTideForecast([FromQuery] int days = 7, CancellationToken ct = default)
    {
        var result = await _aiService.GetTideForecastAsync(days, ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("insights")]
    public async Task<IActionResult> GetInsights(CancellationToken ct = default)
    {
        var result = await _aiService.GetInsightsAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("duplicates")]
    public async Task<IActionResult> DetectDuplicates(CancellationToken ct = default)
    {
        var result = await _aiService.DetectDuplicatesAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeTickets([FromBody] MergeTicketsDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _aiService.MergeTicketsAsync(dto, userId, ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode == 404)
                return NotFound(result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        return Ok(new
        {
            message = result.Data!.Message,
            originalId = result.Data.OriginalId,
            mergedCount = result.Data.MergedCount
        });
    }

    [HttpGet("summary/{ticketId}")]
    public async Task<IActionResult> GetTicketSummary(Guid ticketId, CancellationToken ct = default)
    {
        var result = await _aiService.GetTicketSummaryAsync(ticketId, ct);
        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Data);
    }

    [HttpGet("hot-topics")]
    public async Task<IActionResult> GetHotTopics(CancellationToken ct = default)
    {
        var result = await _aiService.GetHotTopicsAsync(ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        Guid.TryParse(claim, out var id);
        return id;
    }
}
