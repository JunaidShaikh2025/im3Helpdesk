using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Application.DTOs.Audit;
using iM3Helpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<ActivityLogItemDto> Logs, int Total)> GetLogsAsync(
        int page, int pageSize, string? entityType, CancellationToken ct = default)
    {
        var query = _context.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        var total = await query.CountAsync(ct);

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityLogItemDto(
                a.Id,
                a.Action,
                a.Description,
                a.EntityType,
                a.CreatedAt,
                a.User != null ? a.User.FullName : "System"))
            .ToListAsync(ct);

        return (logs, total);
    }
}
