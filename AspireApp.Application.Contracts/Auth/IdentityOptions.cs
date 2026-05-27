using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Contracts.Auth;

public sealed class IdentityOptions
{
    public const string SectionName = "Identity";

    [Range(0, 50)]
    public int MaxFailedAccessAttempts { get; set; } = 5;

    [Range(1, 24 * 60)]
    public int LockoutMinutes { get; set; } = 15;

    [Range(6, 256)]
    public int MinPasswordLength { get; set; } = 8;

    /// <summary>PBKDF2 iterations. OWASP 2023 recommends >= 600,000 for SHA-256.</summary>
    [Range(10_000, 5_000_000)]
    public int PasswordIterations { get; set; } = 600_000;

    public string DefaultRole { get; set; } = "User";

    public SeedAdminOptions SeedAdmin { get; set; } = new();
}

public sealed class SeedAdminOptions
{
    public bool Enabled { get; set; } = true;

    [EmailAddress]
    public string Email { get; set; } = "admin@aspireapp.local";

    public string Name { get; set; } = "Admin";

    public string Surname { get; set; } = "User";

    /// <summary>If empty, no admin password seeded (e.g. SSO-only environments).</summary>
    public string? Password { get; set; }
}
