using AspireApp.Application.Contracts.Base;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Implementations.Base;

public abstract class BaseServiceLong<T>(IBaseDA<T, long> baseDA) : BaseService<T, long>(baseDA) where T : BaseEntity<long>
{
}

public abstract class BaseServiceGuid<T>(IBaseDA<T, Guid> baseDA) : BaseService<T, Guid>(baseDA) where T : BaseEntity<Guid>
{
}

public class BaseService<T, TID>(IBaseDA<T, TID> baseDA) : IBaseService<T, TID>
    where T : BaseEntity<TID>
    where TID : struct
{
    private readonly IBaseDA<T, TID> _baseDA = baseDA;

    public async Task AddAsync(T entity) => await _baseDA.AddAsync(entity);

    public void Delete(T entity) => _baseDA.Delete(entity);

    public async Task<IEnumerable<T>> GetAllAsync() => await _baseDA.GetAllAsync();

    public async Task<T?> GetByIdAsync(TID id) => await _baseDA.GetByIdAsync(id);

    public async Task SaveChangesAsync() => await _baseDA.SaveChangesAsync();

    public void Update(T entity) => _baseDA.Update(entity);
}
