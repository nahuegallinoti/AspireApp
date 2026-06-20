using AspireApp.Domain.Entities.Base;

namespace AspireApp.Domain.Entities;

public class User : AuditableEntity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public int PasswordIterations { get; set; }

    public string? ExternalProvider { get; set; }
    public string? ExternalProviderUserId { get; set; }

    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;

    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }

    public DateTimeOffset? LastLoginUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public bool IsExternal => !string.IsNullOrEmpty(ExternalProvider);
    public bool HasPassword => PasswordHash is { Length: > 0 } && PasswordSalt is { Length: > 0 };
    public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTimeOffset.UtcNow;
}
