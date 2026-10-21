using iM3Helpdesk.Application.DTOs.Audit;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface IAuditService
{
    Task<AuditLogResultDto> GetLogsAsync(
        int page, int pageSize, string? entityType, CancellationToken ct = default);
}
