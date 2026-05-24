using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Contracts.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 24 * 60)]
    public int TokenLifetimeMinutes { get; set; } = 60;
}
