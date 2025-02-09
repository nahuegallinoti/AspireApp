using AspireApp.Api.Domain;
using AspireApp.Application.Contracts.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Application.Implementations.Base;

public class BaseService<TEntity, TModel, TID>(IBaseDA<TEntity, TID> baseDA, BaseMapper<TModel, TEntity> mapper, HybridCache hybridCache)
    : IBaseService<TModel, TID>
                                where TEntity : BaseEntity<TID>
                                where TModel : BaseModel<TID>
                                where TID : struct
{
    private readonly IBaseDA<TEntity, TID> _baseDA = baseDA;
    private readonly BaseMapper<TModel, TEntity> _mapper = mapper;
    private readonly HybridCache _hybridCache = hybridCache;

    public async Task SaveChangesAsync(CancellationToken ct) => await _baseDA.SaveChangesAsync(ct);

    /// <inheritdoc />
    public async Task<TModel> AddAsync(TModel model, CancellationToken ct)
    {
        TEntity entity = _mapper.ToEntity(model);

        await _baseDA.AddAsync(entity, ct);
        await _baseDA.SaveChangesAsync(ct);

        model.Id = entity.Id;

        return model;
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
        string modelName = typeof(TModel).Name;

        string cacheKey = $"{modelName}:{id}";

        TEntity? entity = await _hybridCache.GetOrCreateAsync(cacheKey,
                 async _ => await _baseDA.GetByIdAsync(id),
                 options: new() { Expiration = TimeSpan.FromMinutes(5) }, // TODO: Aca se setea la duracion en la cache
                 tags: [modelName]);

        if (entity is null)
            return null;

        return _mapper.ToModel(entity);
    }

    /// <inheritdoc />
    public void Update(TModel model)
    {
        TEntity entity = _mapper.ToEntity(model);
        _baseDA.Update(entity);
    }
}
