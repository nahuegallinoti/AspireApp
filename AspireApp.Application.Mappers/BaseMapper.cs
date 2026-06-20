namespace AspireApp.Application.Mappers;

public abstract class BaseMapper<TModel, TEntity>
{
    public abstract TModel ToModel(TEntity entity);
    public abstract TEntity ToEntity(TModel model);
    public virtual void UpdateEntity(TEntity entity, TModel model) =>
        throw new NotSupportedException($"{GetType().Name} must override UpdateEntity to support tracked updates.");
    public virtual IEnumerable<TModel> ToModelList(IEnumerable<TEntity> entities) => entities?.Select(ToModel) ?? [];
    public virtual IEnumerable<TEntity> ToEntityList(IEnumerable<TModel> models) => models?.Select(ToEntity) ?? [];
}
