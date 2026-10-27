using iM3Helpdesk.Application.DTOs.Tickets;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;

namespace iM3Helpdesk.Application.Contracts.Persistence;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket?> GetWithDetailsByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TicketListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TicketCalendarItemDto>> GetCalendarTicketsAsync(
        DateTime startUtc, DateTime endUtcExclusive, CancellationToken ct = default);
    Task<TicketStatusCountsDto> GetMyStatusCountsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TicketAttachmentItemDto>> GetAttachmentsByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    Task<int> GetNextTicketNumberAsync(Guid organizationId, CancellationToken ct = default);
    Task<bool> AgentGroupExistsAsync(Guid groupId, Guid organizationId, CancellationToken ct = default);
    Task<bool> UserExistsInOrgAsync(Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task<bool> IsMasterValueAllowedAsync(string field, string value, CancellationToken ct = default);
    Task AddCommentAsync(TicketComment comment, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetDuplicatesCandidatesAsync(
        Guid organizationId, Guid excludeTicketId, DateTime cutoff, CancellationToken ct = default);
    Task<IReadOnlyList<TicketTimelineItemDto>> GetTimelineAsync(Guid ticketId, CancellationToken ct = default);
    Task RecordViewerAsync(Guid ticketId, Guid userId, string userName, Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<TicketViewerItemDto>> GetViewersAsync(Guid ticketId, DateTime sinceUtc, CancellationToken ct = default);
    Task<IReadOnlyList<TicketWatcherItemDto>> GetWatchersAsync(Guid ticketId, CancellationToken ct = default);
    Task<bool> AddWatcherAsync(TicketWatcher watcher, CancellationToken ct = default);
    Task<bool> RemoveWatcherAsync(Guid ticketId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetWatcherUserIdsAsync(Guid ticketId, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetTicketsByIdsAsync(IEnumerable<Guid> ticketIds, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetExportTicketsAsync(TicketStatus? status, TicketPriority? priority, CancellationToken ct = default);
    Task<IReadOnlyList<TicketSearchResultDto>> SearchAsync(
        string? query, TicketStatus? status, TicketPriority? priority, string? category, CancellationToken ct = default);
    Task<IReadOnlyList<TicketByTagItemDto>> GetByTagAsync(string tag, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetHistoricalTicketsAsync(Guid organizationId, DateTime cutoff, bool includeAssignedTo = false, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetOpenTicketsAsync(Guid organizationId, DateTime cutoff, CancellationToken ct = default);
    Task<Ticket?> GetTicketSummaryDataAsync(Guid ticketId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCommentMessageIdsAsync(Guid ticketId, CancellationToken ct = default);
    Task<TicketComment?> GetCommentWithTicketAsync(Guid commentId, Guid ticketId, CancellationToken ct = default);
    Task<IReadOnlyList<TicketAttachment>> GetCommentAttachmentsAsync(Guid commentId, CancellationToken ct = default);
    Task RemoveCommentAsync(TicketComment comment, CancellationToken ct = default);
    Task RemoveAttachmentsAsync(IEnumerable<TicketAttachment> attachments, CancellationToken ct = default);
    Task<Ticket?> GetTicketWithCreatedByAsync(Guid id, CancellationToken ct = default);
    Task<int> MergeDuplicateTicketsAsync(Guid originalTicketId, IEnumerable<Guid> ticketIdsToClose, Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
