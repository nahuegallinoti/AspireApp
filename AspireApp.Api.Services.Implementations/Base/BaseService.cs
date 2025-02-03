using AspireApp.Application.Contracts.Base;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Implementations.Base;

public class BaseService<T, TID, TDA> : IBaseService<T, TID, TDA>
    where T : BaseEntity<TID>
    where TID : struct
    where TDA : IBaseDA<T, TID>
{
    private readonly TDA _baseDA;

    public BaseService(TDA baseDA)
    {
        _baseDA = baseDA;
    }

    public async Task AddAsync(T entity) => await _baseDA.AddAsync(entity);

    public void Delete(T entity) => _baseDA.Delete(entity);

    public async Task<IEnumerable<T>> GetAllAsync() => await _baseDA.GetAllAsync();

    public async Task<T?> GetByIdAsync(TID id) => await _baseDA.GetByIdAsync(id);

    public async Task SaveChangesAsync() => await _baseDA.SaveChangesAsync();

    public void Update(T entity) => _baseDA.Update(entity);
}