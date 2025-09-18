using AspireApp.Core.ROP;
using AspireApp.Entities.Base;

namespace AspireApp.DataAccess.Contracts.Base;

/// <summary>
/// Defines the contract for data access operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TID">The identifier type.</typeparam>
public interface IBaseDA<T, TID> where T : BaseEntity<TID>
                                 where TID : struct
{
    /// <summary>
    /// Add an entity to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<Result<T>> AddAsync(T entity, CancellationToken ct);

    /// <summary>
    /// Delete an entity from the database.
    /// </summary>
    /// <param name="entity"></param>
    void Delete(T entity);

    /// <summary>
    /// Get all entities from the database.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Get an entity by its identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T?> GetByIdAsync(TID id);

    /// <summary>
    /// Update an entity in the database.
    /// </summary>
    /// <param name="entity"></param>
    void Update(T entity);

    /// <summary>
    /// Save changes to the database.
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken ct);
}
