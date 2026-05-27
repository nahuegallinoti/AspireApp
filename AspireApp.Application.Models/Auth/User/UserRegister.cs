using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth.User;

public class UserRegister : UserBase
{
    [Required(ErrorMessage = "Field {0} is required.")]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Field {0} is required.")]
    [MaxLength(128)]
    public string Surname { get; set; } = string.Empty;
}
