namespace AspireApp.Domain.Entities.Base;

/// <summary>Entity with an identifier and persistence-managed audit timestamps.</summary>
public abstract class AuditableEntity<T> : BaseEntity<T>, IAuditableEntity
    where T : struct
{
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset? UpdatedUtc { get; set; }
}
