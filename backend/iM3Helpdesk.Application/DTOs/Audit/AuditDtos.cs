namespace iM3Helpdesk.Application.DTOs.Audit;

public record ActivityLogItemDto(
    Guid Id,
    string Action,
    string Description,
    string EntityType,
    DateTime CreatedAt,
    string User);

public record AuditLogResultDto(
    IReadOnlyList<ActivityLogItemDto> Logs,
    int Total,
    int Page,
    int TotalPages);
