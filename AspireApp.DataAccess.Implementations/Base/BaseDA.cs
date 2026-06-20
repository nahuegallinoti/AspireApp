using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities.Base;
using AspireApp.Domain.Paging;
using AspireApp.Domain.ROP;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations.Base;

public abstract class BaseDA<T, TID>(AppDbContext context) : IBaseDA<T, TID>
    where T : BaseEntity<TID>
    where TID : struct
{
    protected AppDbContext Context { get; } = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(TID id, CancellationToken ct) =>
        await _dbSet.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct) =>
        await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<PagedResult<T>> GetPagedAsync(PagedQuery query, CancellationToken ct)
    {
        var (page, pageSize) = query.Normalize();
        var source = ApplySort(_dbSet.AsNoTracking(), query);
        var total = await source.CountAsync(ct);
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, total, page, pageSize);
    }

    protected virtual IQueryable<T> ApplySort(IQueryable<T> source, PagedQuery query) =>
        query.SortDir == SortDirection.Desc
            ? source.OrderByDescending(entity => entity.Id)
            : source.OrderBy(entity => entity.Id);

    public async Task<Result<T>> AddAsync(T entity, CancellationToken ct)
    {
        await _dbSet.AddAsync(entity, ct);
        return Result.Success(entity);
    }

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct) => Context.SaveChangesAsync(ct);
}
