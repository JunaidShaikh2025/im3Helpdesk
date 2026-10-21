using iM3Helpdesk.Application.DTOs.Audit;

namespace iM3Helpdesk.Application.Contracts.Persistence;

public interface IAuditRepository
{
    Task<(IReadOnlyList<ActivityLogItemDto> Logs, int Total)> GetLogsAsync(
        int page, int pageSize, string? entityType, CancellationToken ct = default);
}
