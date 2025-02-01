using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Contracts.Base;

public interface IBaseService<T, TID, TDA> where T : BaseEntity<TID>
                                           where TID : struct
                                           where TDA : IBaseDA<T, TID>
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TID id);
    void Update(T entity);
    Task SaveChangesAsync();
}