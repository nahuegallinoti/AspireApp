using System.ComponentModel.DataAnnotations;

namespace AspireApp.Api.Domain.Auth.User;

public abstract class UserBase : BaseModel<Guid>
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Field {0} is required.")]
    public string Password { get; set; } = string.Empty;
}
