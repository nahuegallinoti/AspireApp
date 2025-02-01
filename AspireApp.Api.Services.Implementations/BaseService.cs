using AspireApp.Application.Contracts;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Implementations;

public class BaseService<T, TDA>(TDA baseDA) : IBaseService<T, TDA>
    where T : BaseEntity
    where TDA : IBaseDA<T>
{
    protected readonly TDA _baseDA = baseDA;

    public async Task AddAsync(T entity)
    {
        await _baseDA.AddAsync(entity);
    }

    public void Delete(T entity)
    {
        _baseDA.Delete(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _baseDA.GetAllAsync();
    }

    //public async Task<T?> GetByIdAsync(TID id)
    //{
    //    return await _baseDA.GetByIdAsync(id);
    //}

    public async Task SaveChangesAsync()
    {
        await _baseDA.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        _baseDA.Update(entity);
    }
}