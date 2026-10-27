using System.Net;
using System.Text;
using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.Contracts.Services;
using iM3Helpdesk.Application.DTOs.Tickets;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace iM3Helpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _uow;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITenantService _tenantService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ISlaService _slaService;
    private readonly ISmartAssignService? _smartAssignService;
    private readonly ILogger<TicketService> _logger;

    [ActivatorUtilitiesConstructor]
    public TicketService(
        IUnitOfWork uow,
        ITenantService tenantService,
        INotificationService notificationService,
        IEmailService emailService,
        ISlaService slaService,
        ILogger<TicketService> logger,
        ISmartAssignService? smartAssignService = null)
    {
        _uow = uow;
        _ticketRepository = uow.Tickets;
        _tenantService = tenantService;
        _notificationService = notificationService;
        _emailService = emailService;
        _slaService = slaService;
        _logger = logger;
        _smartAssignService = smartAssignService;
    }

    public async Task<ServiceResult<TicketStatusCountsDto>> GetMyStatusCountsAsync(
        Guid userId, string? roleClaim, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return ServiceResult<TicketStatusCountsDto>.Unauthorized();

        var counts = await _ticketRepository.GetMyStatusCountsAsync(userId, ct);
        return ServiceResult<TicketStatusCountsDto>.Success(counts);
    }

    public async Task<IReadOnlyList<TicketListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _ticketRepository.GetAllAsync(ct);
    }

    public async Task<ServiceResult<IReadOnlyList<TicketCalendarItemDto>>> GetCalendarTicketsAsync(
        DateOnly start, DateOnly end, Guid userId, string? roleClaim, CancellationToken ct = default)
    {
        if (end < start)
            return ServiceResult<IReadOnlyList<TicketCalendarItemDto>>.BadRequest("Invalid range");

        if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays > 400)
            return ServiceResult<IReadOnlyList<TicketCalendarItemDto>>.BadRequest("Range too large");

        var (startUtc, endUtcExclusive) = ToUtcRangeForIstDates(start, end);
        var tickets = await _ticketRepository.GetCalendarTicketsAsync(startUtc, endUtcExclusive, ct);
        return ServiceResult<IReadOnlyList<TicketCalendarItemDto>>.Success(tickets);
    }

    public async Task<TicketDetailDto?> GetByIdAsync(Guid id, string? roleClaim, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetWithDetailsByIdAsync(id, ct);
        if (ticket == null) return null;

        bool isAgent = roleClaim is "Agent" or "CompanyAdmin" or "SuperAdmin";
        var attachments = await _ticketRepository.GetAttachmentsByTicketIdAsync(id, ct);

        var comments = ticket.Comments
            .Where(c => isAgent || !c.IsInternal)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TicketCommentItemDto(
                c.Id,
                c.Comment,
                c.CreatedAt,
                c.IsInternal,
                c.Source ?? "web",
                c.EmailMessageId,
                c.Cc,
                c.Bcc,
                c.NotifiedTo,
                c.FromName,
                c.FromEmail,
                c.EditedAt,
                c.EditedBy == null ? null : new TicketCommentUserBriefDto(c.EditedBy.Id, c.EditedBy.FullName, c.EditedBy.Email),
                c.User == null ? null : new UserBriefDto(
                    c.User.Id,
                    c.User.FullName,
                    c.User.Email,
                    c.User.PhotoUrl,
                    c.User.Role == UserRole.Agent || c.User.Role == UserRole.CompanyAdmin)))
            .ToList();

        var assignedToDto = ticket.AssignedTo == null ? null : new UserBriefDto(
            ticket.AssignedTo.Id,
            ticket.AssignedTo.FullName,
            ticket.AssignedTo.Email,
            ticket.AssignedTo.PhotoUrl);

        var createdByDto = ticket.CreatedBy == null ? null : new UserBriefDto(
            ticket.CreatedBy.Id,
            ticket.CreatedBy.FullName,
            ticket.CreatedBy.Email,
            ticket.CreatedBy.PhotoUrl);

        var agentGroupDto = ticket.AgentGroup == null ? null : new AgentGroupBriefDto(
            ticket.AgentGroup.Id,
            ticket.AgentGroup.Name);

        return new TicketDetailDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Category,
            ticket.FromEmail,
            ticket.FromName,
            ticket.InboundMessageId,
            ticket.CcEmails,
            ticket.BccEmails,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.TicketType,
            ticket.Tags,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            ticket.SlaDeadline,
            ticket.SlaStatus,
            ticket.IsSlaBreached,
            ticket.TicketNumber,
            ticket.TimeSpentMinutes,
            $"#TN{ticket.TicketNumber}",
            ticket.AssignedToUserId,
            assignedToDto,
            ticket.AgentGroupId,
            agentGroupDto,
            createdByDto,
            comments,
            attachments);
    }

    public async Task<ServiceResult<CreateTicketResultDto>> CreateAsync(
        CreateTicketDto dto, Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return ServiceResult<CreateTicketResultDto>.Unauthorized();

        var orgId = _tenantService.OrganizationId!.Value;
        var lastNum = await _ticketRepository.GetNextTicketNumberAsync(orgId, ct);

        Guid? groupId = null;
        if (dto.AgentGroupId.HasValue && dto.AgentGroupId.Value != Guid.Empty)
        {
            if (await _ticketRepository.AgentGroupExistsAsync(dto.AgentGroupId.Value, orgId, ct))
                groupId = dto.AgentGroupId.Value;
        }

        Guid? assignedTo = null;
        if (dto.AssignedToUserId.HasValue && dto.AssignedToUserId.Value != Guid.Empty)
            assignedTo = dto.AssignedToUserId.Value;

        var priority = TicketHelpers.TryParseTicketPriority(dto.Priority, out var p) ? p : TicketPriority.Medium;

        var ticket = new Ticket
        {
            Title = dto.Title?.Trim() ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Category = dto.Category ?? "General",
            Priority = priority,
            Status = TicketStatus.Open,
            TicketType = dto.TicketType ?? "Support",
            OrganizationId = orgId,
            CreatedByUserId = userId,
            Tags = dto.Tags ?? string.Empty,
            AssignedToUserId = assignedTo,
            AgentGroupId = groupId,
            TicketNumber = lastNum
        };

        if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketPriorityField, ticket.Priority.ToString(), ct))
        {
            return ServiceResult<CreateTicketResultDto>.BadRequest($"Priority {ticket.Priority} is not active in ticket master");
        }

        var normalizedTicketType = (ticket.TicketType ?? "Support").Trim();
        if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketTypeField, normalizedTicketType, ct))
        {
            return ServiceResult<CreateTicketResultDto>.BadRequest($"Ticket Type {normalizedTicketType} is not active in ticket master");
        }
        ticket.TicketType = normalizedTicketType;

        ticket.SlaDeadline = await _slaService.CalculateSlaDeadlineAsync(orgId, ticket.Priority, ticket.CreatedAt, groupId);
        ticket.SlaStatus = "OnTrack";

        await _ticketRepository.AddAsync(ticket, ct);
        await _ticketRepository.SaveChangesAsync(ct);

        await _notificationService.CreateActivityAsync(
            userId, orgId, "Created", $"New ticket: {ticket.Title}", "Ticket", ticket.Id);

        if (ticket.AssignedToUserId == null && _smartAssignService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _smartAssignService.EvaluateTicketAsync(ticket.Id, orgId, allowAutoAssign: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Smart Assign trigger failed for ticket {TicketId}", ticket.Id);
                }
            });
        }

        return ServiceResult<CreateTicketResultDto>.Success(
            new CreateTicketResultDto("Ticket created", ticket.Id, ticket.TicketNumber));
    }

    public async Task<ServiceResult> UpdateAsync(
        Guid id, UpdateTicketDto dto, Guid userId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: false, ct);
        if (ticket == null)
            return ServiceResult.NotFound("Ticket not found");

        if (userId == Guid.Empty)
            return ServiceResult.Unauthorized();

        var orgId = _tenantService.OrganizationId!.Value;

        if (!string.IsNullOrEmpty(dto.Title))
            ticket.Title = dto.Title.Trim();
        if (dto.Description != null)
            ticket.Description = dto.Description;
        if (dto.Category != null)
            ticket.Category = dto.Category;

        if (dto.TicketType != null)
        {
            var normalizedTicketType = dto.TicketType.Trim();
            if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketTypeField, normalizedTicketType, ct))
            {
                return ServiceResult.BadRequest($"Ticket Type {normalizedTicketType} is not active in ticket master");
            }
            ticket.TicketType = normalizedTicketType;
        }

        if (dto.Tags != null)
            ticket.Tags = dto.Tags;

        if (!string.IsNullOrEmpty(dto.Priority) && TicketHelpers.TryParseTicketPriority(dto.Priority, out var newP))
        {
            if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketPriorityField, newP.ToString(), ct))
            {
                return ServiceResult.BadRequest($"Priority {newP} is not active in ticket master");
            }

            ticket.Priority = newP;
            ticket.SlaDeadline = await _slaService.CalculateSlaDeadlineAsync(
                ticket.OrganizationId, ticket.Priority, ticket.CreatedAt, ticket.AgentGroupId);
            ticket.SlaStatus = _slaService.GetSlaStatus(ticket.SlaDeadline, ticket.Status);
        }

        if (!string.IsNullOrEmpty(dto.Status) && TicketHelpers.TryParseTicketStatus(dto.Status, out var newS))
        {
            if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketStatusField, newS.ToString(), ct))
            {
                return ServiceResult.BadRequest($"Status {newS} is not active in ticket master");
            }

            if ((newS == TicketStatus.Resolved || newS == TicketStatus.ResolvedOnBeta) &&
                ticket.Status != TicketStatus.Resolved &&
                ticket.Status != TicketStatus.ResolvedOnBeta)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }
            ticket.Status = newS;
        }

        if (dto.AssignedToUserId.HasValue)
        {
            if (dto.AssignedToUserId.Value == Guid.Empty)
                ticket.AssignedToUserId = null;
            else if (await _ticketRepository.UserExistsInOrgAsync(dto.AssignedToUserId.Value, orgId, ct))
                ticket.AssignedToUserId = dto.AssignedToUserId.Value;
        }

        if (dto.AgentGroupId.HasValue)
        {
            if (dto.AgentGroupId.Value == Guid.Empty)
                ticket.AgentGroupId = null;
            else if (await _ticketRepository.AgentGroupExistsAsync(dto.AgentGroupId.Value, orgId, ct))
                ticket.AgentGroupId = dto.AgentGroupId.Value;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.LastActivityAt = DateTime.UtcNow;
        await _ticketRepository.SaveChangesAsync(ct);

        await _notificationService.CreateActivityAsync(
            userId, orgId, "Updated", $"Ticket updated: {ticket.Title}", "Ticket", ticket.Id);

        await NotifyWatchersAndAssigneeAsync(
            ticket, userId, "Ticket updated", $"Details were updated on #TN{ticket.TicketNumber}: {ticket.Title}", ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: false, ct);
        if (ticket == null)
            return ServiceResult.NotFound();

        if (ticket.IsDeleted)
            return ServiceResult.Success(200);

        ticket.IsDeleted = true;
        ticket.DeletedAt = DateTime.UtcNow;
        ticket.DeletedByUserId = userId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepository.SaveChangesAsync(ct);

        await _notificationService.CreateActivityAsync(
            userId, _tenantService.OrganizationId!.Value, "Deleted", $"Ticket moved to recycle bin: {ticket.Title}", "Ticket", ticket.Id);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<DuplicateCandidateDto>>> GetDuplicatesAsync(
        Guid id, CancellationToken ct = default)
    {
        var orgId = _tenantService.OrganizationId!.Value;
        var current = await _ticketRepository.GetByIdAsync(id, asNoTracking: true, ct);
        if (current == null)
            return ServiceResult<IReadOnlyList<DuplicateCandidateDto>>.NotFound();

        var cutoff = DateTime.UtcNow.AddDays(-60);
        var others = await _ticketRepository.GetDuplicatesCandidatesAsync(orgId, id, cutoff, ct);

        var currentWords = GetWords(current.Title);
        var duplicates = new List<DuplicateCandidateDto>();

        foreach (var t in others)
        {
            var words = GetWords(t.Title);
            var sim = GetSimilarity(currentWords, words);
            var sameCustomer = current.CreatedByUserId == t.CreatedByUserId;
            var score = sim + (sameCustomer ? 0.2 : 0);

            if (score >= 0.5)
            {
                duplicates.Add(new DuplicateCandidateDto(
                    t.Id,
                    t.TicketNumber,
                    t.Title,
                    t.Status.ToString(),
                    t.CreatedAt,
                    (int)(score * 100),
                    sameCustomer));
            }
        }

        var sorted = duplicates.OrderByDescending(d => d.Similarity).Take(5).ToList();
        return ServiceResult<IReadOnlyList<DuplicateCandidateDto>>.Success(sorted);
    }

    public async Task<ServiceResult<MergeTicketsResultDto>> MergeAsync(
        Guid id, MergeIntoDto dto, Guid userId, CancellationToken ct = default)
    {
        var orgId = _tenantService.OrganizationId!.Value;

        var original = await _ticketRepository.GetByIdAsync(id, asNoTracking: false, ct);
        if (original == null || original.OrganizationId != orgId)
            return ServiceResult<MergeTicketsResultDto>.NotFound("Original ticket not found");

        var duplicate = await _ticketRepository.GetByIdAsync(dto.DuplicateTicketId, asNoTracking: false, ct);
        if (duplicate == null || duplicate.OrganizationId != orgId || duplicate.Id == id)
            return ServiceResult<MergeTicketsResultDto>.NotFound("Duplicate ticket not found");

        duplicate.Status = TicketStatus.Closed;
        duplicate.ResolvedAt = DateTime.UtcNow;
        duplicate.UpdatedAt = DateTime.UtcNow;

        await _ticketRepository.AddCommentAsync(new TicketComment
        {
            TicketId = duplicate.Id,
            UserId = userId,
            Comment = $"🔄 This ticket has been merged into <strong>#TN{original.TicketNumber}</strong> — <em>{original.Title}</em>. Please follow up on the original ticket.",
            IsInternal = false,
            Source = "system",
            OrganizationId = orgId
        }, ct);

        await _ticketRepository.AddCommentAsync(new TicketComment
        {
            TicketId = original.Id,
            UserId = userId,
            Comment = $"🔄 Ticket <strong>#TN{duplicate.TicketNumber}</strong> has been merged into this ticket.",
            IsInternal = true,
            Source = "system",
            OrganizationId = orgId
        }, ct);

        await _notificationService.CreateActivityAsync(
            userId, orgId, "Merged", $"#TN{duplicate.TicketNumber} merged into #TN{original.TicketNumber}", "Ticket", original.Id);

        await _ticketRepository.SaveChangesAsync(ct);

        if (duplicate.CreatedByUserId.HasValue && duplicate.CreatedByUserId.Value != Guid.Empty)
        {
            var customer = await _ticketRepository.GetUserByIdAsync(duplicate.CreatedByUserId.Value, ct);
            if (!string.IsNullOrWhiteSpace(customer?.Email))
            {
                try
                {
                    await _emailService.SendAsync(
                        customer.Email,
                        $"Ticket #TN{duplicate.TicketNumber} merged",
                        $"<p>Hi {customer.FullName},</p><p>Your ticket <strong>#TN{duplicate.TicketNumber}</strong> has been merged into <strong>#TN{original.TicketNumber}</strong>. Please use that ticket number for future reference.</p>",
                        organizationId: duplicate.OrganizationId);
                }
                catch { /* ignore email error */ }
            }
        }

        return ServiceResult<MergeTicketsResultDto>.Success(new MergeTicketsResultDto(
            $"#TN{duplicate.TicketNumber} merged into #TN{original.TicketNumber}",
            original.Id,
            duplicate.Id,
            duplicate.TicketNumber));
    }

    public async Task<IReadOnlyList<TicketTimelineItemDto>> GetTimelineAsync(Guid id, CancellationToken ct = default)
    {
        return await _ticketRepository.GetTimelineAsync(id, ct);
    }

    public async Task RecordViewAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty) return;

        var user = await _ticketRepository.GetUserByIdAsync(userId, ct);
        var userName = user?.FullName ?? "Unknown";
        var orgId = _tenantService.OrganizationId!.Value;

        await _ticketRepository.RecordViewerAsync(id, userId, userName, orgId, ct);
        await _ticketRepository.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TicketViewerItemDto>> GetViewersAsync(Guid id, CancellationToken ct = default)
    {
        var sinceUtc = DateTime.UtcNow.AddHours(-24);
        return await _ticketRepository.GetViewersAsync(id, sinceUtc, ct);
    }

    public async Task<ServiceResult<IReadOnlyList<TicketWatcherItemDto>>> GetWatchersAsync(Guid id, CancellationToken ct = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: true, ct);
        if (ticket == null)
            return ServiceResult<IReadOnlyList<TicketWatcherItemDto>>.NotFound();

        var watchers = await _ticketRepository.GetWatchersAsync(id, ct);
        return ServiceResult<IReadOnlyList<TicketWatcherItemDto>>.Success(watchers);
    }

    public async Task<ServiceResult> AddMeAsWatcherAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return ServiceResult.Unauthorized();

        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: true, ct);
        if (ticket == null)
            return ServiceResult.NotFound();

        var added = await _ticketRepository.AddWatcherAsync(new TicketWatcher
        {
            TicketId = id,
            UserId = userId,
            OrganizationId = ticket.OrganizationId,
            CreatedAt = DateTime.UtcNow
        }, ct);

        if (!added)
            return ServiceResult.Success(); // already watching

        await _ticketRepository.SaveChangesAsync(ct);

        await _notificationService.CreateActivityAsync(
            userId, ticket.OrganizationId, "WatcherAdded", "Started watching this ticket", "Ticket", id);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AddWatcherAsync(
        Guid id, AddWatcherDto dto, Guid actorId, CancellationToken ct = default)
    {
        if (dto.UserId == Guid.Empty)
            return ServiceResult.BadRequest("UserId is required");

        if (actorId == Guid.Empty)
            return ServiceResult.Unauthorized();

        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: true, ct);
        if (ticket == null)
            return ServiceResult.NotFound();

        var targetUser = await _ticketRepository.GetUserByIdAsync(dto.UserId, ct);
        if (targetUser == null || targetUser.OrganizationId != ticket.OrganizationId)
            return ServiceResult.BadRequest("User not found in organization");

        var added = await _ticketRepository.AddWatcherAsync(new TicketWatcher
        {
            TicketId = id,
            UserId = dto.UserId,
            OrganizationId = ticket.OrganizationId,
            CreatedAt = DateTime.UtcNow
        }, ct);

        if (!added)
            return ServiceResult.Success(); // already watching

        await _ticketRepository.SaveChangesAsync(ct);

        await _notificationService.CreateActivityAsync(
            actorId, ticket.OrganizationId, "WatcherAdded", $"Added watcher: {targetUser.FullName}", "Ticket", id);

        if (dto.UserId != actorId)
        {
            await _notificationService.CreateAsync(
                dto.UserId,
                ticket.OrganizationId,
                "Added as watcher",
                $"You were added as watcher on #TN{ticket.TicketNumber}: {ticket.Title}",
                "info",
                id);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveWatcherAsync(Guid id, Guid userId, Guid actorId, CancellationToken ct = default)
    {
        if (actorId == Guid.Empty)
            return ServiceResult.Unauthorized();

        var ticket = await _ticketRepository.GetByIdAsync(id, asNoTracking: true, ct);
        if (ticket == null)
            return ServiceResult.NotFound();

        var removed = await _ticketRepository.RemoveWatcherAsync(id, userId, ct);
        if (!removed)
            return ServiceResult.Success();

        await _ticketRepository.SaveChangesAsync(ct);

        var action = userId == actorId ? "Stopped watching this ticket" : "Removed a watcher from this ticket";
        await _notificationService.CreateActivityAsync(
            actorId, ticket.OrganizationId, "WatcherRemoved", action, "Ticket", id);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<BulkUpdateResultDto>> BulkUpdateAsync(
        BulkUpdateDto dto, Guid userId, CancellationToken ct = default)
    {
        var tickets = await _ticketRepository.GetTicketsByIdsAsync(dto.TicketIds, ct);
        if (tickets.Count == 0)
            return ServiceResult<BulkUpdateResultDto>.BadRequest("No tickets found");

        foreach (var t in tickets)
        {
            if (!string.IsNullOrEmpty(dto.Status) && TicketHelpers.TryParseTicketStatus(dto.Status, out var s))
            {
                if (!await _ticketRepository.IsMasterValueAllowedAsync(TicketHelpers.TicketStatusField, s.ToString(), ct))
                {
                    return ServiceResult<BulkUpdateResultDto>.BadRequest($"Status {s} is not active in ticket master");
                }
                t.Status = s;
            }

            if (dto.AssignedToUserId.HasValue)
                t.AssignedToUserId = dto.AssignedToUserId;

            t.UpdatedAt = DateTime.UtcNow;
        }

        await _ticketRepository.SaveChangesAsync(ct);

        if (userId != Guid.Empty)
        {
            await _notificationService.CreateActivityAsync(
                userId, _tenantService.OrganizationId!.Value, "BulkUpdate", $"Bulk updated {tickets.Count} tickets", "Ticket");
        }

        return ServiceResult<BulkUpdateResultDto>.Success(
            new BulkUpdateResultDto($"{tickets.Count} tickets updated", tickets.Count));
    }

    public async Task<byte[]> ExportCsvAsync(string? status, string? priority, CancellationToken ct = default)
    {
        TicketStatus? sFilter = null;
        if (!string.IsNullOrEmpty(status) && status != "All" && TicketHelpers.TryParseTicketStatus(status, out var parsedS))
            sFilter = parsedS;

        TicketPriority? pFilter = null;
        if (!string.IsNullOrEmpty(priority) && priority != "All" && TicketHelpers.TryParseTicketPriority(priority, out var parsedP))
            pFilter = parsedP;

        var tickets = await _ticketRepository.GetExportTicketsAsync(sFilter, pFilter, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,TicketNumber,Title,Category,Status,Priority,CreatedBy,AssignedTo,SlaStatus,CreatedAt,ResolvedAt");

        foreach (var t in tickets)
        {
            sb.AppendLine(
                $"{t.Id}," +
                $"TN{t.TicketNumber}," +
                $"\"{t.Title.Replace("\"", "\"\"")}\"," +
                $"{t.Category}," +
                $"{t.Status}," +
                $"{t.Priority}," +
                $"\"{t.CreatedBy?.FullName ?? ""}\"," +
                $"\"{t.AssignedTo?.FullName ?? "Unassigned"}\"," +
                $"{t.SlaStatus ?? "N/A"}," +
                $"{t.CreatedAt:yyyy-MM-dd HH:mm}," +
                $"{(t.ResolvedAt.HasValue ? t.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm") : "")}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<IReadOnlyList<TicketSearchResultDto>> SearchAsync(
        string? query, string? status, string? priority, string? category, CancellationToken ct = default)
    {
        TicketStatus? sf = null;
        if (!string.IsNullOrEmpty(status) && status != "All" && TicketHelpers.TryParseTicketStatus(status, out var parsedS))
            sf = parsedS;

        TicketPriority? pf = null;
        if (!string.IsNullOrEmpty(priority) && priority != "All" && TicketHelpers.TryParseTicketPriority(priority, out var parsedP))
            pf = parsedP;

        var catFilter = (!string.IsNullOrEmpty(category) && category != "All") ? category : null;
        return await _ticketRepository.SearchAsync(query, sf, pf, catFilter, ct);
    }

    public async Task<IReadOnlyList<TicketByTagItemDto>> GetByTagAsync(string tag, CancellationToken ct = default)
    {
        return await _ticketRepository.GetByTagAsync(tag, ct);
    }

    private async Task NotifyWatchersAndAssigneeAsync(
        Ticket ticket, Guid actorUserId, string title, string message, CancellationToken ct = default)
    {
        var recipients = new HashSet<Guid>();
        if (ticket.AssignedToUserId.HasValue)
            recipients.Add(ticket.AssignedToUserId.Value);

        var watcherIds = await _ticketRepository.GetWatcherUserIdsAsync(ticket.Id, ct);
        foreach (var wid in watcherIds)
            recipients.Add(wid);

        recipients.Remove(actorUserId);
        if (recipients.Count == 0) return;

        foreach (var recipientId in recipients)
        {
            try
            {
                await _notificationService.CreateAsync(
                    recipientId, ticket.OrganizationId, title, message, "info", ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Watcher notification failed for {UserId}", recipientId);
            }
        }

        var users = await _ticketRepository.GetUsersByIdsAsync(recipients, ct);
        foreach (var user in users.Where(u => !string.IsNullOrWhiteSpace(u.Email)))
        {
            try
            {
                await _emailService.SendAsync(
                    user.Email!,
                    title,
                    $"<p>{WebUtility.HtmlEncode(message)}</p>",
                    organizationId: ticket.OrganizationId,
                    ticketNumberTag: $"#TN{ticket.TicketNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Watcher email failed for {Email}", user.Email);
            }
        }
    }

    private static TimeZoneInfo? TryGetIst()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); }
        catch { return null; }
    }

    private static (DateTime startUtc, DateTime endUtcExclusive) ToUtcRangeForIstDates(DateOnly start, DateOnly end)
    {
        var tz = TryGetIst();
        var startIst = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var endNextIstDay = end.AddDays(1);
        var endIstExclusive = new DateTime(endNextIstDay.Year, endNextIstDay.Month, endNextIstDay.Day, 0, 0, 0, DateTimeKind.Unspecified);

        if (tz == null)
            return (DateTime.SpecifyKind(startIst, DateTimeKind.Utc), DateTime.SpecifyKind(endIstExclusive, DateTimeKind.Utc));

        return (
            TimeZoneInfo.ConvertTimeToUtc(startIst, tz),
            TimeZoneInfo.ConvertTimeToUtc(endIstExclusive, tz)
        );
    }

    private static List<string> GetWords(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new();
        return text.ToLower()
            .Split(new[] { ' ', ',', '.', '!', '?', '-', '_', '/', '(', ')', '[' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();
    }

    private static double GetSimilarity(List<string> w1, List<string> w2)
    {
        if (!w1.Any() || !w2.Any())
            return 0;
        var inter = w1.Intersect(w2).Count();
        var union = w1.Union(w2).Count();
        return union > 0 ? (double)inter / union : 0;
    }
}
