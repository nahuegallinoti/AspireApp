using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Contracts;

public interface IBaseService<T, TDA> where T : BaseEntity
                                      where TDA : IBaseDA<T>
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    //Task<T?> GetByIdAsync(TID id);
    void Update(T entity);
    Task SaveChangesAsync();
}