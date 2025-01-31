using System.ComponentModel.DataAnnotations;

namespace AspireApp.Api.Domain.Auth.User;

public class UserRegister : UserBase
{
    [Required(ErrorMessage = "El campo {0} es requerido.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo {0} es requerido.")]
    public string Apellido { get; set; } = string.Empty;
}