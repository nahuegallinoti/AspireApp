using AspireApp.Domain.Entities.Base;

namespace AspireApp.Domain.Entities;

public class Role : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
