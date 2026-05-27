using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Users;

public sealed class UpdateUserProfileRequest
{
    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Surname { get; set; } = string.Empty;
}
