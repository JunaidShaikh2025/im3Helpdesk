using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context, ICurrentTenantService tenantService)
        : base(context, tenantService)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, bool includeOrganization = true, CancellationToken ct = default)
    {
        var query = _context.Users.IgnoreQueryFilters();
        if (includeOrganization)
        {
            query = query.Include(u => u.Organization);
        }

        return await query.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
    }

    public async Task<User?> GetByIdWithOrgAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshTokenHash &&
                u.RefreshTokenExpiresAt > DateTime.UtcNow, ct);
    }

    public async Task<User?> GetByVerificationTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct);
    }

    public async Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var query = _context.Users.IgnoreQueryFilters().Where(u => u.Email.ToLower() == email.ToLower());
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId == organizationId)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
    }

    public async Task<UserSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _context.UserSessions
            .Include(s => s.User)
            .ThenInclude(u => u!.Organization)
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);
    }

    public async Task<IReadOnlyList<UserSession>> GetFamilySessionsAsync(Guid familyId, CancellationToken ct = default)
    {
        return await _context.UserSessions
            .Where(s => s.FamilyId == familyId && !s.IsRevoked)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(ct);
    }

    public async Task AddSessionAsync(UserSession session, CancellationToken ct = default)
    {
        await _context.UserSessions.AddAsync(session, ct);
    }
}
