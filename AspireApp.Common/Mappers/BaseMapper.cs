namespace AspireApp.Core.Mappers;

public abstract class BaseMapper<TModel, TEntity>
{
    public abstract TModel ToModel(TEntity entity);
    public abstract TEntity ToEntity(TModel model);

    public virtual IEnumerable<TModel> ToModelList(IEnumerable<TEntity> entities) => entities?.Select(ToModel) ?? [];
    public virtual IEnumerable<TEntity> ToEntityList(IEnumerable<TModel> models) => models?.Select(ToEntity) ?? [];
}