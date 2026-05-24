using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Auth.User;

public class UserRegister : UserBase
{
    [Required(ErrorMessage = "Field {0} is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Field {0} is required.")]
    public string Surname { get; set; } = string.Empty;
}