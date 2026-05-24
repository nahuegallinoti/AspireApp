using AspireApp.Domain.Entities.Base;

namespace AspireApp.Domain.Entities;

public class Show : BaseEntity<long>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
