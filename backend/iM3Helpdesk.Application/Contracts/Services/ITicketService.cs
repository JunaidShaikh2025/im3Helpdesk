using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.DTOs.Tickets;

namespace iM3Helpdesk.Application.Contracts.Services;

public interface ITicketService
{
    Task<ServiceResult<TicketStatusCountsDto>> GetMyStatusCountsAsync(
        Guid userId, string? roleClaim, CancellationToken ct = default);

    Task<IReadOnlyList<TicketListItemDto>> GetAllAsync(CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<TicketCalendarItemDto>>> GetCalendarTicketsAsync(
        DateOnly start, DateOnly end, Guid userId, string? roleClaim, CancellationToken ct = default);

    Task<TicketDetailDto?> GetByIdAsync(Guid id, string? roleClaim, CancellationToken ct = default);

    Task<ServiceResult<CreateTicketResultDto>> CreateAsync(
        CreateTicketDto dto, Guid userId, CancellationToken ct = default);

    Task<ServiceResult> UpdateAsync(
        Guid id, UpdateTicketDto dto, Guid userId, CancellationToken ct = default);

    Task<ServiceResult> DeleteAsync(
        Guid id, Guid userId, CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<DuplicateCandidateDto>>> GetDuplicatesAsync(
        Guid id, CancellationToken ct = default);

    Task<ServiceResult<MergeTicketsResultDto>> MergeAsync(
        Guid id, MergeIntoDto dto, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<TicketTimelineItemDto>> GetTimelineAsync(
        Guid id, CancellationToken ct = default);

    Task RecordViewAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<TicketViewerItemDto>> GetViewersAsync(
        Guid id, CancellationToken ct = default);

    Task<ServiceResult<IReadOnlyList<TicketWatcherItemDto>>> GetWatchersAsync(
        Guid id, CancellationToken ct = default);

    Task<ServiceResult> AddMeAsWatcherAsync(
        Guid id, Guid userId, CancellationToken ct = default);

    Task<ServiceResult> AddWatcherAsync(
        Guid id, AddWatcherDto dto, Guid actorId, CancellationToken ct = default);

    Task<ServiceResult> RemoveWatcherAsync(
        Guid id, Guid userId, Guid actorId, CancellationToken ct = default);

    Task<ServiceResult<BulkUpdateResultDto>> BulkUpdateAsync(
        BulkUpdateDto dto, Guid userId, CancellationToken ct = default);

    Task<byte[]> ExportCsvAsync(
        string? status, string? priority, CancellationToken ct = default);

    Task<IReadOnlyList<TicketSearchResultDto>> SearchAsync(
        string? query, string? status, string? priority, string? category, CancellationToken ct = default);

    Task<IReadOnlyList<TicketByTagItemDto>> GetByTagAsync(
        string tag, CancellationToken ct = default);
}
