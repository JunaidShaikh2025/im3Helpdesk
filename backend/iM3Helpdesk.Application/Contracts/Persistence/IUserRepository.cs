using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Contracts.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, bool includeOrganization = true, CancellationToken ct = default);
    Task<User?> GetByIdWithOrgAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);
    Task<User?> GetByVerificationTokenAsync(string token, CancellationToken ct = default);
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken ct = default);
    Task<UserSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyList<UserSession>> GetFamilySessionsAsync(Guid familyId, CancellationToken ct = default);
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddSessionAsync(UserSession session, CancellationToken ct = default);
}
