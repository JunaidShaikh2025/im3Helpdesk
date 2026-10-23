using iM3Helpdesk.Application.Common;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.DTOs.Tickets;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Domain.Enums;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context, ICurrentTenantService? tenantService = null)
        : base(context, tenantService)
    {
        _context = context;
    }

    public override async Task<Ticket?> GetByIdAsync(Guid id, bool asNoTracking = true, CancellationToken ct = default)
    {
        var query = _context.Tickets.AsQueryable();
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Ticket?> GetWithDetailsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.AgentGroup)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.Comments)
                .ThenInclude(c => c.EditedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<TicketListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsSplitQuery()
            .OrderByDescending(t => t.CreatedAt)
            .Take(500)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Title,
                t.Category,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.TicketType ?? "Support",
                t.Tags,
                t.TicketNumber,
                t.CreatedAt,
                t.UpdatedAt,
                t.ResolvedAt,
                t.LastActivityAt,
                t.SlaDeadline,
                t.SlaStatus,
                t.IsSlaBreached,
                t.CreatedBy != null ? t.CreatedBy.FullName : string.Empty,
                t.AssignedTo != null ? t.AssignedTo.FullName : null,
                t.AssignedToUserId,
                t.AgentGroupId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketCalendarItemDto>> GetCalendarTicketsAsync(
        DateTime startUtc, DateTime endUtcExclusive, Guid? customerUserId, CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsSplitQuery()
            .AsQueryable();

        if (customerUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == customerUserId.Value);

        query = query.Where(t =>
            (t.CreatedAt >= startUtc && t.CreatedAt < endUtcExclusive) ||
            (t.UpdatedAt != null && t.UpdatedAt >= startUtc && t.UpdatedAt < endUtcExclusive) ||
            (t.LastActivityAt != null && t.LastActivityAt >= startUtc && t.LastActivityAt < endUtcExclusive));

        return await query
            .OrderByDescending(t => t.UpdatedAt ?? t.LastActivityAt ?? t.CreatedAt)
            .Take(2000)
            .Select(t => new TicketCalendarItemDto(
                t.Id,
                t.Title,
                t.Category,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.TicketType ?? "Support",
                t.Tags,
                t.TicketNumber,
                t.CreatedAt,
                t.UpdatedAt,
                t.ResolvedAt,
                t.LastActivityAt,
                t.FromName,
                t.FromEmail,
                t.InboundMessageId,
                t.SlaStatus,
                t.CreatedBy != null ? t.CreatedBy.FullName : string.Empty,
                t.AssignedTo != null ? t.AssignedTo.FullName : null,
                t.AssignedToUserId))
            .ToListAsync(ct);
    }

    public async Task<TicketStatusCountsDto> GetMyStatusCountsAsync(
        Guid userId, bool isCustomer, CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        if (isCustomer)
            query = query.Where(t => t.CreatedByUserId == userId);
        else
            query = query.Where(t => t.AssignedToUserId == userId);

        var grouped = await query
            .GroupBy(t => t.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(ct);

        int Get(TicketStatus s) => grouped.FirstOrDefault(x => x.status == s)?.count ?? 0;

        var open = Get(TicketStatus.Open);
        var inProgress = Get(TicketStatus.InProgress);
        var resolved = Get(TicketStatus.Resolved) + Get(TicketStatus.ResolvedOnBeta);
        var closed = Get(TicketStatus.Closed);
        var pending = Get(TicketStatus.Pending);
        var total = open + inProgress + pending + resolved + closed;

        return new TicketStatusCountsDto(open, inProgress, pending, resolved, closed, total);
    }

    public async Task<IReadOnlyList<TicketAttachmentItemDto>> GetAttachmentsByTicketIdAsync(
        Guid ticketId, CancellationToken ct = default)
    {
        var attachments = await _context.TicketAttachments
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(ct);

        return attachments.Select(a => new TicketAttachmentItemDto(
            a.Id,
            a.FileName,
            a.FileUrl,
            a.ContentType,
            a.FileSize,
            a.UploadedAt,
            a.CommentId,
            a.ContentType.StartsWith("image/"),
            TicketHelpers.FormatSize(a.FileSize))).ToList();
    }

    public async Task<int> GetNextTicketNumberAsync(Guid organizationId, CancellationToken ct = default)
    {
        var lastNum = await _context.Tickets
            .IgnoreQueryFilters()
            .Where(t => t.OrganizationId == organizationId)
            .MaxAsync(t => (int?)t.TicketNumber, ct)
            ?? 1000;

        return lastNum + 1;
    }

    public async Task<bool> AgentGroupExistsAsync(Guid groupId, Guid organizationId, CancellationToken ct = default)
    {
        return await _context.AgentGroups
            .AnyAsync(g => g.Id == groupId && g.OrganizationId == organizationId, ct);
    }

    public async Task<bool> UserExistsInOrgAsync(Guid userId, Guid organizationId, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == userId && u.OrganizationId == organizationId, ct);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<IReadOnlyList<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var idList = userIds.ToList();
        return await _context.Users
            .IgnoreQueryFilters()
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);
    }

    public async Task<bool> IsMasterValueAllowedAsync(string field, string value, CancellationToken ct = default)
    {
        var hasRows = await _context.TicketFieldMasters
            .AnyAsync(x => x.Field == field, ct);

        if (!hasRows)
            return true;

        return await _context.TicketFieldMasters
            .AnyAsync(x => x.Field == field && x.IsActive && x.Value == value, ct);
    }

    public override async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _context.Tickets.AddAsync(ticket, ct);
    }

    public async Task AddCommentAsync(TicketComment comment, CancellationToken ct = default)
    {
        await _context.TicketComments.AddAsync(comment, ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetDuplicatesCandidatesAsync(
        Guid organizationId, Guid excludeTicketId, DateTime cutoff, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t =>
                t.OrganizationId == organizationId &&
                t.Id != excludeTicketId &&
                t.Status != TicketStatus.Closed &&
                t.CreatedAt >= cutoff)
            .Take(200)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketTimelineItemDto>> GetTimelineAsync(Guid ticketId, CancellationToken ct = default)
    {
        return await _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.EntityId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new TicketTimelineItemDto(
                a.Id,
                a.Action,
                a.Description,
                a.CreatedAt,
                a.User != null ? a.User.FullName : "System"))
            .ToListAsync(ct);
    }

    public async Task RecordViewerAsync(
        Guid ticketId, Guid userId, string userName, Guid organizationId, CancellationToken ct = default)
    {
        var existing = await _context.TicketViewers
            .FirstOrDefaultAsync(v => v.TicketId == ticketId && v.UserId == userId, ct);

        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            await _context.TicketViewers.AddAsync(new TicketViewer
            {
                TicketId = ticketId,
                UserId = userId,
                UserName = userName,
                OrganizationId = organizationId,
                ViewedAt = DateTime.UtcNow
            }, ct);
        }
    }

    public async Task<IReadOnlyList<TicketViewerItemDto>> GetViewersAsync(
        Guid ticketId, DateTime sinceUtc, CancellationToken ct = default)
    {
        return await _context.TicketViewers
            .Where(v => v.TicketId == ticketId && v.ViewedAt >= sinceUtc)
            .OrderByDescending(v => v.ViewedAt)
            .Select(v => new TicketViewerItemDto(v.UserId, v.UserName, v.ViewedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketWatcherItemDto>> GetWatchersAsync(
        Guid ticketId, CancellationToken ct = default)
    {
        var watcherRows = await _context.TicketWatchers
            .AsNoTracking()
            .Where(w => w.TicketId == ticketId)
            .ToListAsync(ct);

        var userIds = watcherRows.Select(w => w.UserId).Distinct().ToList();

        var users = await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email, u.PhotoUrl })
            .ToListAsync(ct);

        var userMap = users.ToDictionary(u => u.Id, u => u);

        return watcherRows
            .Select(w =>
            {
                userMap.TryGetValue(w.UserId, out var u);
                return new TicketWatcherItemDto(
                    w.UserId,
                    u?.FullName ?? "Unknown",
                    u?.Email ?? string.Empty,
                    u?.PhotoUrl,
                    w.CreatedAt);
            })
            .OrderBy(x => x.FullName)
            .ToList();
    }

    public async Task<bool> AddWatcherAsync(TicketWatcher watcher, CancellationToken ct = default)
    {
        var already = await _context.TicketWatchers
            .AnyAsync(w => w.TicketId == watcher.TicketId && w.UserId == watcher.UserId, ct);
        if (already) return false;

        try
        {
            await _context.TicketWatchers.AddAsync(watcher, ct);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RemoveWatcherAsync(Guid ticketId, Guid userId, CancellationToken ct = default)
    {
        var watcher = await _context.TicketWatchers
            .FirstOrDefaultAsync(w => w.TicketId == ticketId && w.UserId == userId, ct);
        if (watcher == null) return false;

        _context.TicketWatchers.Remove(watcher);
        return true;
    }

    public async Task<IReadOnlyList<Guid>> GetWatcherUserIdsAsync(Guid ticketId, CancellationToken ct = default)
    {
        return await _context.TicketWatchers
            .Where(w => w.TicketId == ticketId)
            .Select(w => w.UserId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsByIdsAsync(IEnumerable<Guid> ticketIds, CancellationToken ct = default)
    {
        var ids = ticketIds.ToList();
        return await _context.Tickets
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetExportTicketsAsync(
        TicketStatus? status, TicketPriority? priority, CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketSearchResultDto>> SearchAsync(
        string? query, TicketStatus? status, TicketPriority? priority, string? category, CancellationToken ct = default)
    {
        var tickets = _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
            tickets = tickets.Where(t => t.Title.Contains(query) || t.Description.Contains(query));

        if (status.HasValue)
            tickets = tickets.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            tickets = tickets.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrEmpty(category))
            tickets = tickets.Where(t => t.Category == category);

        return await tickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .Select(t => new TicketSearchResultDto(
                t.Id,
                t.Title,
                t.Category,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.TicketType ?? "Support",
                t.Tags,
                t.TicketNumber,
                t.CreatedAt,
                t.SlaDeadline,
                t.SlaStatus,
                t.IsSlaBreached,
                t.CreatedBy != null ? t.CreatedBy.FullName : string.Empty,
                t.AssignedTo != null ? t.AssignedTo.FullName : null))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TicketByTagItemDto>> GetByTagAsync(string tag, CancellationToken ct = default)
    {
        var normalizedTag = tag.ToLower();
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Where(t => t.Tags != null && t.Tags.Contains(normalizedTag))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketByTagItemDto(
                t.Id,
                t.Title,
                t.Category,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.TicketNumber,
                t.CreatedAt,
                t.CreatedBy != null ? t.CreatedBy.FullName : string.Empty))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetHistoricalTicketsAsync(Guid organizationId, DateTime cutoff, bool includeAssignedTo = false, CancellationToken ct = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId && t.CreatedAt >= cutoff);

        if (includeAssignedTo)
        {
            query = query.Include(t => t.AssignedTo);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetOpenTicketsAsync(Guid organizationId, DateTime cutoff, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId && t.Status != TicketStatus.Closed && t.CreatedAt >= cutoff)
            .ToListAsync(ct);
    }

    public async Task<Ticket?> GetTicketSummaryDataAsync(Guid ticketId, CancellationToken ct = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);
    }

    public async Task<int> MergeDuplicateTicketsAsync(Guid originalTicketId, IEnumerable<Guid> ticketIdsToClose, Guid userId, Guid organizationId, CancellationToken ct = default)
    {
        var original = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == originalTicketId && t.OrganizationId == organizationId, ct);

        if (original == null)
            return -1;

        var merged = 0;
        foreach (var tId in ticketIdsToClose)
        {
            var dup = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == tId && t.OrganizationId == organizationId && t.Id != originalTicketId, ct);

            if (dup == null) continue;

            dup.Status = TicketStatus.Closed;
            dup.ResolvedAt = DateTime.UtcNow;
            dup.UpdatedAt = DateTime.UtcNow;

            _context.TicketComments.Add(new TicketComment
            {
                TicketId = dup.Id,
                UserId = userId,
                Comment = $"🔀 This ticket was merged into #TN{original.TicketNumber}. Please follow up on the original ticket.",
                IsInternal = false,
                Source = "system",
                OrganizationId = organizationId
            });

            _context.TicketComments.Add(new TicketComment
            {
                TicketId = original.Id,
                UserId = userId,
                Comment = $"🔀 Ticket #TN{dup.TicketNumber} was merged into this ticket.",
                IsInternal = true,
                Source = "system",
                OrganizationId = organizationId
            });

            merged++;
        }

        await _context.SaveChangesAsync(ct);
        return merged;
    }

    public async Task<IReadOnlyList<string>> GetCommentMessageIdsAsync(Guid ticketId, CancellationToken ct = default)
    {
        return await _context.TicketComments
            .Where(c => c.TicketId == ticketId && !string.IsNullOrEmpty(c.EmailMessageId))
            .OrderBy(c => c.CreatedAt)
            .Select(c => c.EmailMessageId!)
            .ToListAsync(ct);
    }

    public async Task<TicketComment?> GetCommentWithTicketAsync(Guid commentId, Guid ticketId, CancellationToken ct = default)
    {
        return await _context.TicketComments
            .Include(c => c.Ticket)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.TicketId == ticketId, ct);
    }

    public async Task<IReadOnlyList<TicketAttachment>> GetCommentAttachmentsAsync(Guid commentId, CancellationToken ct = default)
    {
        return await _context.TicketAttachments
            .Where(a => a.CommentId == commentId)
            .ToListAsync(ct);
    }

    public Task RemoveCommentAsync(TicketComment comment, CancellationToken ct = default)
    {
        _context.TicketComments.Remove(comment);
        return Task.CompletedTask;
    }

    public Task RemoveAttachmentsAsync(IEnumerable<TicketAttachment> attachments, CancellationToken ct = default)
    {
        _context.TicketAttachments.RemoveRange(attachments);
        return Task.CompletedTask;
    }

    public async Task<Ticket?> GetTicketWithCreatedByAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tickets
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
