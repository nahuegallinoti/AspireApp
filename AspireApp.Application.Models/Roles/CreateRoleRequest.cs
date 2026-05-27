using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Roles;

public sealed class CreateRoleRequest
{
    [Required, MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Description { get; set; }
}
