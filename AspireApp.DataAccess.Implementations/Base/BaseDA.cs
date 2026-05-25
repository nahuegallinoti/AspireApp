using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities.Base;
using AspireApp.Domain.ROP;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations.Base;

public class BaseDA<T, TID>(AppDbContext context) : IBaseDA<T, TID>
    where T : BaseEntity<TID>
    where TID : struct
{
    internal readonly AppDbContext _context = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(TID id, CancellationToken ct) =>
        await _dbSet.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct) =>
        await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<Result<T>> AddAsync(T entity, CancellationToken ct)
    {
        await _dbSet.AddAsync(entity, ct);
        return Result.Success(entity);
    }

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
