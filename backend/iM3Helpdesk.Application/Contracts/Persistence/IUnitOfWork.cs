using iM3Helpdesk.Domain.Entities;

namespace iM3Helpdesk.Application.Contracts.Persistence;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    ITicketRepository Tickets { get; }
    IUserRepository Users { get; }
    IAuditRepository Audits { get; }
    IGenericRepository<ActivityLog> ActivityLogs { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<Organization> Organizations { get; }

    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
