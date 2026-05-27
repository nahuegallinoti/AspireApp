using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Users;

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(256)]
    public string NewPassword { get; set; } = string.Empty;
}
