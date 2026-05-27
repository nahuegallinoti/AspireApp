using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth;

public sealed class ExternalLoginRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The id_token issued by the SSO provider (e.g. Google) - must be validated server-side.
    /// </summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
