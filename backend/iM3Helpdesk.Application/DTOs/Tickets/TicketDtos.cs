namespace iM3Helpdesk.Application.DTOs.Tickets;

public record TicketStatusCountsDto(
    int Open,
    int InProgress,
    int Pending,
    int Resolved,
    int Closed,
    int Total);

public record TicketListItemDto(
    Guid Id,
    string Title,
    string? Category,
    string Status,
    string Priority,
    string TicketType,
    string? Tags,
    int TicketNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? LastActivityAt,
    DateTime? SlaDeadline,
    string? SlaStatus,
    bool IsSlaBreached,
    string CreatedBy,
    string? AssignedTo,
    Guid? AssignedToId,
    Guid? AgentGroupId);

public record TicketCalendarItemDto(
    Guid Id,
    string Title,
    string? Category,
    string Status,
    string Priority,
    string TicketType,
    string? Tags,
    int TicketNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? LastActivityAt,
    string? FromName,
    string? FromEmail,
    string? InboundMessageId,
    string? SlaStatus,
    string CreatedBy,
    string? AssignedTo,
    Guid? AssignedToId);

public record UserBriefDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhotoUrl = null,
    bool IsAgent = false);

public record AgentGroupBriefDto(
    Guid Id,
    string Name);

public record TicketCommentUserBriefDto(
    Guid Id,
    string FullName,
    string Email);

public record TicketCommentItemDto(
    Guid Id,
    string Comment,
    DateTime CreatedAt,
    bool IsInternal,
    string Source,
    string? EmailMessageId,
    string? Cc,
    string? Bcc,
    string? NotifiedTo,
    string? FromName,
    string? FromEmail,
    DateTime? EditedAt,
    TicketCommentUserBriefDto? EditedBy,
    UserBriefDto? User);

public record TicketAttachmentItemDto(
    Guid Id,
    string FileName,
    string FileUrl,
    string ContentType,
    long FileSize,
    DateTime UploadedAt,
    Guid? CommentId,
    bool IsImage,
    string SizeFormatted);

public record TicketDetailDto(
    Guid Id,
    string Title,
    string Description,
    string? Category,
    string? FromEmail,
    string? FromName,
    string? InboundMessageId,
    string? CcEmails,
    string? BccEmails,
    string Status,
    string Priority,
    string? TicketType,
    string? Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? SlaDeadline,
    string? SlaStatus,
    bool IsSlaBreached,
    int TicketNumber,
    int TimeSpentMinutes,
    string TicketDisplayId,
    Guid? AssignedToUserId,
    UserBriefDto? AssignedTo,
    Guid? AgentGroupId,
    AgentGroupBriefDto? AgentGroup,
    UserBriefDto? CreatedBy,
    IReadOnlyList<TicketCommentItemDto> Comments,
    IReadOnlyList<TicketAttachmentItemDto> Attachments);

public record CreateTicketResultDto(
    string Message,
    Guid Id,
    int TicketNumber);

public record DuplicateCandidateDto(
    Guid Id,
    int TicketNumber,
    string Title,
    string Status,
    DateTime CreatedAt,
    int Similarity,
    bool SameCustomer);

public record MergeTicketsResultDto(
    string Message,
    Guid OriginalId,
    Guid DuplicateId,
    int DuplicateTicketNumber);

public record TicketTimelineItemDto(
    Guid Id,
    string Action,
    string Description,
    DateTime CreatedAt,
    string User);

public record TicketViewerItemDto(
    Guid UserId,
    string UserName,
    DateTime ViewedAt);

public record TicketWatcherItemDto(
    Guid UserId,
    string FullName,
    string Email,
    string? PhotoUrl,
    DateTime CreatedAt);

public record BulkUpdateResultDto(
    string Message,
    int UpdatedCount);

public record TicketSearchResultDto(
    Guid Id,
    string Title,
    string? Category,
    string Status,
    string Priority,
    string TicketType,
    string? Tags,
    int TicketNumber,
    DateTime CreatedAt,
    DateTime? SlaDeadline,
    string? SlaStatus,
    bool IsSlaBreached,
    string CreatedBy,
    string? AssignedTo);

public record TicketByTagItemDto(
    Guid Id,
    string Title,
    string? Category,
    string Status,
    string Priority,
    int TicketNumber,
    DateTime CreatedAt,
    string CreatedBy);
