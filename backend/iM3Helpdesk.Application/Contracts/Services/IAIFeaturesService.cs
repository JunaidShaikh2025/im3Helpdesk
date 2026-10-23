using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.DTOs.AI;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface IAIFeaturesService
{
    Task<ServiceResult<object>> GetTideForecastAsync(int days = 7, CancellationToken ct = default);
    Task<ServiceResult<object>> GetInsightsAsync(CancellationToken ct = default);
    Task<ServiceResult<object>> DetectDuplicatesAsync(CancellationToken ct = default);
    Task<ServiceResult<MergeTicketsResultDto>> MergeTicketsAsync(MergeTicketsDto dto, Guid userId, CancellationToken ct = default);
    Task<ServiceResult<object>> GetTicketSummaryAsync(Guid ticketId, CancellationToken ct = default);
    Task<ServiceResult<object>> GetHotTopicsAsync(CancellationToken ct = default);
}
