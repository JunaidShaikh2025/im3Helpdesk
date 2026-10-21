using System.Collections.Concurrent;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Domain.Entities;
using iM3Helpdesk.Infrastructure.Repositories;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace iM3Helpdesk.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private IDbContextTransaction? _transaction;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(
        ApplicationDbContext context,
        ICurrentTenantService tenantService,
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        IAuditRepository auditRepository)
    {
        _context = context;
        _tenantService = tenantService;
        Tickets = ticketRepository;
        Users = userRepository;
        Audits = auditRepository;

        ActivityLogs = new GenericRepository<ActivityLog>(_context, _tenantService);
        Notifications = new GenericRepository<Notification>(_context, _tenantService);
        Organizations = new GenericRepository<Organization>(_context, _tenantService);
    }

    public ITicketRepository Tickets { get; }
    public IUserRepository Users { get; }
    public IAuditRepository Audits { get; }
    public IGenericRepository<ActivityLog> ActivityLogs { get; }
    public IGenericRepository<Notification> Notifications { get; }
    public IGenericRepository<Organization> Organizations { get; }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        return (IGenericRepository<TEntity>)_repositories.GetOrAdd(
            typeof(TEntity),
            _ => new GenericRepository<TEntity>(_context, _tenantService));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(ct);
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
