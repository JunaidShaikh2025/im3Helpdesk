using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Audit;

namespace iM3Helpdesk.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<AuditLogResultDto> GetLogsAsync(
        int page, int pageSize, string? entityType, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var (logs, total) = await _auditRepository.GetLogsAsync(page, pageSize, entityType, ct);
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return new AuditLogResultDto(logs, total, page, totalPages);
    }
}
