using System.Linq.Expressions;
using iM3Helpdesk.Application.Contracts.Persistence;
using iM3Helpdesk.Domain.Interfaces;
using iM3Helpdesk.Infrastructure.Persistence;
using iM3Helpdesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace iM3Helpdesk.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;
    protected readonly ICurrentTenantService? TenantService;

    public GenericRepository(ApplicationDbContext context, ICurrentTenantService? tenantService = null)
    {
        Context = context;
        DbSet = context.Set<T>();
        TenantService = tenantService;
    }

    protected virtual IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
    {
        if (TenantService == null || TenantService.IsSuperAdmin || !typeof(IMustHaveTenant).IsAssignableFrom(typeof(T)))
            return query;

        var orgId = TenantService.OrganizationId;
        if (!orgId.HasValue)
            return query;

        return query.Where(e => EF.Property<Guid>(e, "OrganizationId") == orgId.Value);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, bool asNoTracking = true, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        query = ApplyTenantFilter(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        query = ApplyTenantFilter(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken ct = default)
    {
        var query = DbSet.Where(predicate);
        query = ApplyTenantFilter(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken ct = default)
    {
        var query = DbSet.Where(predicate);
        query = ApplyTenantFilter(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken ct = default)
    {
        var query = DbSet.Where(predicate);
        query = ApplyTenantFilter(query);
        return await query.AnyAsync(ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, 
        CancellationToken ct = default)
    {
        var query = predicate != null ? DbSet.Where(predicate) : DbSet.AsQueryable();
        query = ApplyTenantFilter(query);
        return await query.CountAsync(ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        if (entity is IMustHaveTenant tenantEntity && tenantEntity.OrganizationId == Guid.Empty && TenantService?.OrganizationId.HasValue == true)
        {
            tenantEntity.OrganizationId = TenantService.OrganizationId.Value;
        }

        await DbSet.AddAsync(entity, ct);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (var entity in entities)
        {
            if (entity is IMustHaveTenant tenantEntity && tenantEntity.OrganizationId == Guid.Empty && TenantService?.OrganizationId.HasValue == true)
            {
                tenantEntity.OrganizationId = TenantService.OrganizationId.Value;
            }
        }

        await DbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
}
