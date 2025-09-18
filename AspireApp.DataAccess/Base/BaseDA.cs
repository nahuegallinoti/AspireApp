using AspireApp.Core.ROP;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations.Base;

/// <summary>
/// Base data access implementation handling database operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TID">The identifier type.</typeparam>
public class BaseDA<T, TID> : IBaseDA<T, TID> where T : BaseEntity<TID>
                                              where TID : struct
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public BaseDA(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(TID id) => await _dbSet.FindAsync(id);

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct) => await _dbSet.ToListAsync(ct);

    /// <inheritdoc />
    public async Task<Result<T>> AddAsync(T entity, CancellationToken ct)
    {
        try
        {
            await _dbSet.AddAsync(entity, ct);
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(["Error al agregar entidad: " + ex.Message]);
        }
    }

    /// <inheritdoc />
    public void Update(T entity) => _dbSet.Update(entity);

    /// <inheritdoc />
    public void Delete(T entity) => _dbSet.Remove(entity);

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct) => await _context.SaveChangesAsync(ct);
}