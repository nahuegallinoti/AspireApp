namespace AspireApp.Core.Mappers;

/// <summary>
/// Abstract base class for mapping between domain models and entities.
/// </summary>
/// <typeparam name="TModel">The domain model type.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class BaseMapper<TModel, TEntity>
{
    /// <summary>
    /// Converts an entity to a domain model.
    /// </summary>
    /// <param name="entity">The entity to convert.</param>
    /// <returns>The corresponding domain model.</returns>
    public abstract TModel ToModel(TEntity entity);

    /// <summary>
    /// Converts a domain model to an entity.
    /// </summary>
    /// <param name="model">The model to convert.</param>
    /// <returns>The corresponding entity.</returns>
    public abstract TEntity ToEntity(TModel model);

    /// <summary>
    /// Converts a collection of entities to a collection of domain models.
    /// </summary>
    /// <param name="entities">The collection of entities.</param>
    /// <returns>A collection of corresponding domain models.</returns>
    public virtual IEnumerable<TModel> ToModelList(IEnumerable<TEntity> entities) => entities?.Select(ToModel) ?? [];

    /// <summary>
    /// Converts a collection of domain models to a collection of entities.
    /// </summary>
    /// <param name="models">The collection of models.</param>
    /// <returns>A collection of corresponding entities.</returns>
    public virtual IEnumerable<TEntity> ToEntityList(IEnumerable<TModel> models) => models?.Select(ToEntity) ?? [];
}
