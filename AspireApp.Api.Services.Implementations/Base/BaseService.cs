using AspireApp.Api.Domain;
using AspireApp.Application.Contracts.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.Application.Implementations.Base;

public class BaseService<TEntity, TModel, TID>(IBaseDA<TEntity, TID> baseDA, BaseMapper<TModel, TEntity> mapper, IMemoryCache cache)
    : IBaseService<TModel, TID>
                                where TEntity : BaseEntity<TID>
                                where TModel : BaseModel<TID>
                                where TID : struct
{
    private readonly IBaseDA<TEntity, TID> _baseDA = baseDA;
    private readonly BaseMapper<TModel, TEntity> _mapper = mapper;
    private readonly IMemoryCache _cache = cache;

    public async Task SaveChangesAsync(CancellationToken ct) => await _baseDA.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task AddAsync(TModel model, CancellationToken ct)
    {
        TEntity entity = _mapper.ToEntity(model);
        await _baseDA.AddAsync(entity, ct);
    }

    /// <inheritdoc />
    public void Delete(TModel model)
    {
        TEntity? entity = _mapper.ToEntity(model);
        _baseDA.Delete(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken ct)
    {
        IEnumerable<TEntity> entities = await _baseDA.GetAllAsync(ct);
        return _mapper.ToModelList(entities);
    }

    /// <inheritdoc />
    public async Task<TModel?> GetByIdAsync(TID id)
    {
        var cacheKey = $"{typeof(TModel).Name}:{id}";

        TEntity? entity = await _cache.GetOrCreateAsync(cacheKey, async async => await _baseDA.GetByIdAsync(id));

        return entity is not null ? _mapper.ToModel(entity) : null;
    }

    public async Task<TModel?> GetByIdAsyncFromDb(TID id)
    {
        TEntity? entity = await _baseDA.GetByIdAsync(id);
        return entity is not null ? _mapper.ToModel(entity) : null;
    }



    /// <inheritdoc />
    public void Update(TModel model)
    {
        TEntity entity = _mapper.ToEntity(model);
        _baseDA.Update(entity);
    }
}
