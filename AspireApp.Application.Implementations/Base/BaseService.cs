using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models;
using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities.Base;
using AspireApp.Domain.Paging;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Application.Implementations.Base;

public abstract class BaseService<TEntity, TModel, TID>(
    IBaseDA<TEntity, TID> baseDA,
    BaseMapper<TModel, TEntity> mapper,
    HybridCache hybridCache) : IBaseService<TModel, TID>
    where TEntity : BaseEntity<TID>
    where TModel : BaseModel<TID>
    where TID : struct
{
    private static readonly HybridCacheEntryOptions CacheOptions = new() { Expiration = TimeSpan.FromMinutes(5) };
    private static readonly string CacheTag = typeof(TModel).Name;

    private bool _pendingInvalidation;

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await baseDA.SaveChangesAsync(ct);
        if (_pendingInvalidation)
        {
            await hybridCache.RemoveByTagAsync(CacheTag, ct);
            _pendingInvalidation = false;
        }
    }

    public async Task<Result<TModel>> AddAsync(TModel model, CancellationToken ct)
    {
        var entity = mapper.ToEntity(model);
        await baseDA.AddAsync(entity, ct);
        await baseDA.SaveChangesAsync(ct);
        model.Id = entity.Id;
        await hybridCache.RemoveByTagAsync(CacheTag, ct);
        return Result.Success(model);
    }

    public void Delete(TModel model)
    {
        baseDA.Delete(mapper.ToEntity(model));
        _pendingInvalidation = true;
    }

    public async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken ct)
    {
        var entities = await baseDA.GetAllAsync(ct);
        return mapper.ToModelList(entities);
    }

    public async Task<PagedResult<TModel>> GetPagedAsync(PagedQuery query, CancellationToken ct)
    {
        // Pages are intentionally not cached: their contents change whenever the collection changes.
        var result = await baseDA.GetPagedAsync(query, ct);
        return new PagedResult<TModel>(
            mapper.ToModelList(result.Items).ToList(),
            result.Total,
            result.Page,
            result.PageSize);
    }

    public async Task<TModel?> GetByIdAsync(TID id, CancellationToken ct)
    {
        var entity = await hybridCache.GetOrCreateAsync(
            $"{CacheTag}:{id}",
            async token => await baseDA.GetByIdAsync(id, token),
            options: CacheOptions,
            tags: [CacheTag],
            cancellationToken: ct);

        return entity is null ? null : mapper.ToModel(entity);
    }

    public void Update(TModel model)
    {
        baseDA.Update(mapper.ToEntity(model));
        _pendingInvalidation = true;
    }

    public async Task<bool> UpdateAsync(TModel model, CancellationToken ct)
    {
        var entity = await baseDA.GetByIdAsync(model.Id, ct);
        if (entity is null)
            return false;

        mapper.UpdateEntity(entity, model);
        _pendingInvalidation = true;
        return true;
    }
}
