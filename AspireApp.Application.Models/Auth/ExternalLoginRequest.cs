using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth;

public sealed class ExternalLoginRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    /// <summary>OpenID Connect id_token (preferred).</summary>
    public string? IdToken { get; set; }

    /// <summary>OAuth access_token fallback when id_token is not issued.</summary>
    public string? AccessToken { get; set; }
}
