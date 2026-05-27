namespace AspireApp.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset AssignedUtc { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
    public Role? Role { get; set; }
}
