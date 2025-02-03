using AspireApp.Api.Domain;

namespace AspireApp.Application.Contracts.Base;

public interface IBaseServiceLong<T> : IBaseService<T, long> where T : BaseModel<long>
{
}

public interface IBaseServiceGuid<T> : IBaseService<T, Guid> where T : BaseModel<Guid>
{
}

public interface IBaseService<T, TID> where T : BaseModel<TID>
                                      where TID : struct
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TID id);
    void Update(T entity);
    Task SaveChangesAsync();
}