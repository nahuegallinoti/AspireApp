using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth;

public sealed class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
