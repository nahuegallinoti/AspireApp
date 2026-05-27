using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Models.Users;

public sealed class AssignRolesRequest
{
    [Required, MinLength(1)]
    public IReadOnlyList<string> Roles { get; set; } = [];
}
