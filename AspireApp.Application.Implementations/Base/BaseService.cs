using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models;
using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities.Base;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Application.Implementations.Base;

public class BaseService<TEntity, TModel, TID>(
    IBaseDA<TEntity, TID> baseDA,
    BaseMapper<TModel, TEntity> mapper,
    HybridCache hybridCache) : IBaseService<TModel, TID>
    where TEntity : BaseEntity<TID>
    where TModel : BaseModel<TID>
    where TID : struct
{
    private static readonly HybridCacheEntryOptions CacheOptions = new() { Expiration = TimeSpan.FromMinutes(5) };
    private static readonly string ModelName = typeof(TModel).Name;

    public Task SaveChangesAsync(CancellationToken ct) => baseDA.SaveChangesAsync(ct);

    public Task<Result<TModel>> AddAsync(TModel model, CancellationToken ct)
    {
        TEntity entity = mapper.ToEntity(model);

        return baseDA.AddAsync(entity, ct)
            .Bind(async _ =>
            {
                await baseDA.SaveChangesAsync(ct);
                model.Id = entity.Id;
                await hybridCache.RemoveByTagAsync(ModelName, ct);
                return Result.Success(model);
            });
    }

    public void Delete(TModel model) => baseDA.Delete(mapper.ToEntity(model));

    public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken ct)
    {
        IEnumerable<TEntity> entities = await baseDA.GetAllAsync(ct);
        return mapper.ToModelList(entities);
    }

    public async Task<TModel?> GetByIdAsync(TID id, CancellationToken ct)
    {
        string cacheKey = $"{ModelName}:{id}";

        TEntity? entity = await hybridCache.GetOrCreateAsync(
            cacheKey,
            async token => await baseDA.GetByIdAsync(id, token),
            options: CacheOptions,
            tags: [ModelName],
            cancellationToken: ct);

        return entity is null ? null : mapper.ToModel(entity);
    }

    public void Update(TModel model) => baseDA.Update(mapper.ToEntity(model));
}
