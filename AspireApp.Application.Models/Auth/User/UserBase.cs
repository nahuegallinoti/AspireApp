using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth.User;

public abstract class UserBase : BaseModel<Guid>
{
    [Required(ErrorMessage = $"Field {nameof(Email)} is required.")]
    [EmailAddress(ErrorMessage = $"Field {nameof(Email)} must be a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = $"Field {nameof(Password)} is required.")]
    [MinLength(8, ErrorMessage = $"Field {nameof(Password)} must be at least 8 characters long.")]
    [MaxLength(256)]
    public string Password { get; set; } = string.Empty;
}
