namespace AspireApp.Application.Models.Roles;

public sealed record RoleDto(Guid Id, string Name, string? Description, bool IsSystem);
