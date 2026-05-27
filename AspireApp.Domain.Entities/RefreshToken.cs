using AspireApp.Domain.Entities.Base;

namespace AspireApp.Domain.Entities;

public class RefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the opaque refresh token (the raw token is never stored).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresUtc { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedByIp { get; set; }

    public DateTimeOffset? RevokedUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? RevokedReason { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public User? User { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresUtc;
    public bool IsRevoked => RevokedUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;
}
