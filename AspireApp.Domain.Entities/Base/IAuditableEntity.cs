namespace AspireApp.Domain.Entities.Base;

/// <summary>Marks an entity whose audit timestamps are managed by persistence.</summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedUtc { get; set; }
    DateTimeOffset? UpdatedUtc { get; set; }
}
