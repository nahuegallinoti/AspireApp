using AspireApp.Domain.Entities.Base;
using AspireApp.Domain.Paging;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Persistence.Base;

public interface IBaseDA<T, TID>
    where T : BaseEntity<TID>
    where TID : struct
{
    Task<Result<T>> AddAsync(T entity, CancellationToken ct);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<T>> GetPagedAsync(PagedQuery query, CancellationToken ct);
    Task<T?> GetByIdAsync(TID id, CancellationToken ct);
    void Update(T entity);
    Task SaveChangesAsync(CancellationToken ct);
}
