using AspireApp.Application.Models;
using AspireApp.Domain.Paging;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Base;

public interface IBaseService<T, TID>
    where T : BaseModel<TID>
    where TID : struct
{
    Task<Result<T>> AddAsync(T entity, CancellationToken ct);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<T>> GetPagedAsync(PagedQuery query, CancellationToken ct);
    Task<T?> GetByIdAsync(TID id, CancellationToken ct);
    void Update(T entity);
    Task<bool> UpdateAsync(T entity, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
