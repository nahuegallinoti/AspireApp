using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Users;

public sealed class UpdateUserRequest
{
    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Surname { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
