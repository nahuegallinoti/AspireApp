using System.ComponentModel.DataAnnotations;

namespace AspireApp.Api.Domain.Auth.User;

public abstract class UserBase : BaseModel<Guid>
{
    [Required(ErrorMessage = "El campo {0} es requerido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo {0} es requerido.")]
    public string Password { get; set; } = string.Empty;
}
