using AspireApp.Api.Domain;

namespace AspireApp.Application.Contracts.Base
{
    /// <summary>
    /// Defines the base service contract for CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TID">The identifier type.</typeparam>
    public interface IBaseService<T, TID> where T : BaseModel<TID>
                                          where TID : struct
    {
        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<T> AddAsync(T entity, CancellationToken ct);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity"></param>
        void Delete(T entity);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);

        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsync(TID id);

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);

        /// <summary>
        /// Saves the changes made to the entities.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync(CancellationToken ct);
    }
}
