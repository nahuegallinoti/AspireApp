using AspireApp.Api.Domain;
using AspireApp.Application.Contracts.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;

namespace AspireApp.Application.Implementations.Base;

public abstract class BaseServiceLong<TEntity, TModel>(IBaseDA<TEntity, long> baseDA, BaseMapper<TModel, TEntity> mapper)
    :
    BaseService<TEntity, TModel, long>(baseDA, mapper) where TEntity : BaseEntity<long>
                                                       where TModel : BaseModel<long>
{
}

public abstract class BaseServiceGuid<TEntity, TModel>(IBaseDA<TEntity, Guid> baseDA, BaseMapper<TModel, TEntity> mapper)
    :
    BaseService<TEntity, TModel, Guid>(baseDA, mapper) where TEntity : BaseEntity<Guid>
                                                       where TModel : BaseModel<Guid>
{
}


public class BaseService<TEntity, TModel, TID>(IBaseDA<TEntity, TID> baseDA, BaseMapper<TModel, TEntity> mapper)
    : IBaseService<TModel, TID>
                                where TEntity : BaseEntity<TID>
                                where TModel : BaseModel<TID>
                                where TID : struct
{
    private readonly IBaseDA<TEntity, TID> _baseDA = baseDA;
    private readonly BaseMapper<TModel, TEntity> _mapper = mapper;

    public async Task SaveChangesAsync() => await _baseDA.SaveChangesAsync();


    public async Task AddAsync(TModel model)
    {
        TEntity entity = _mapper.ToEntity(model);

        await _baseDA.AddAsync(entity);
    }

    public void Delete(TModel model)
    {
        TEntity? entity = _mapper.ToEntity(model);

        _baseDA.Delete(entity);
    }

    public async Task<IEnumerable<TModel>> GetAllAsync()
    {
        IEnumerable<TEntity> entities = await _baseDA.GetAllAsync();

        return _mapper.ToModelList(entities);
    }

    public async Task<TModel?> GetByIdAsync(TID id)
    {
        TEntity? entity = await _baseDA.GetByIdAsync(id);

        if (entity is null)
            return null;

        return _mapper.ToModel(entity);
    }

    public void Update(TModel model)
    {
        TEntity entity = _mapper.ToEntity(model);

        _baseDA.Update(entity);
    }
}
