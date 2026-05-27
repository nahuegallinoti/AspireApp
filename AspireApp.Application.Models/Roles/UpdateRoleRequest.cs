using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Roles;

public sealed class UpdateRoleRequest
{
    [MaxLength(256)]
    public string? Description { get; set; }
}
