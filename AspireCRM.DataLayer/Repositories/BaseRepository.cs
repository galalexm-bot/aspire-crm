using System.Linq.Expressions;
using AspireCRM.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.DataLayer.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AspireCRMDbContext Context;
    protected readonly ITenantService TenantService;
    protected IQueryable<T> Set => ApplyTenantFilter(Context.Set<T>());

    public BaseRepository(AspireCRMDbContext context, ITenantService tenantService)
    {
        Context = context;
        TenantService = tenantService;
    }

    private IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
    {
        if (TenantService.TenantId.HasValue)
            return query.Where(e => e.TenantId == TenantService.TenantId.Value && !e.IsDeleted);
        return query.Where(e => !e.IsDeleted);
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await Set.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await Set.ToListAsync(ct);
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Set.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null, CancellationToken ct = default)
    {
        var query = Set;
        if (filter is not null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        if (TenantService.TenantId.HasValue)
            entity.TenantId = TenantService.TenantId.Value;
        entity.CreatedAt = DateTime.UtcNow;
        var entry = await Context.Set<T>().AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entry.Entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await Context.SaveChangesAsync(ct);
    }
}