using AspireApp.Entities.Base;

namespace AspireApp.DataAccess.Contracts.Base;

public interface IBaseDA<T, TID> where T : BaseEntity<TID>
                                 where TID: struct
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TID id);
    void Update(T entity);
    Task SaveChangesAsync();
}