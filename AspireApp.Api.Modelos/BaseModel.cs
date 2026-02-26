namespace AspireApp.Application.Models;

/// <summary>
/// Represents a base model with an identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public class BaseModel<T> where T : struct
{
    public T Id { get; set; }
}