using AspireApp.Entities.Base;

namespace AspireApp.Entities;

public class Show : BaseEntity<long>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
