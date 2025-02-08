using AspireApp.Api.Domain;
using AspireApp.Application.Contracts.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Implementations.Base;

/// <summary>
/// Implementation of the base service providing common CRUD functionalities.
/// </summary>
/// <typeparam name="TEntity">The database entity type.</typeparam>
/// <typeparam name="TModel">The domain model type.</typeparam>
/// <typeparam name="TID">The identifier type.</typeparam>
public class BaseService<TEntity, TModel, TID>(IBaseDA<TEntity, TID> baseDA, BaseMapper<TModel, TEntity> mapper)
    : IBaseService<TModel, TID>
                                where TEntity : BaseEntity<TID>
                                where TModel : BaseModel<TID>
                                where TID : struct
{
    private readonly IBaseDA<TEntity, TID> _baseDA = baseDA;
    private readonly BaseMapper<TModel, TEntity> _mapper = mapper;

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
