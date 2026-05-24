namespace AspireApp.Domain.Entities.Base;

/// <summary>
/// Represents a base entity with an identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public class BaseEntity<T> where T : struct
{
    public T Id { get; set; }
}