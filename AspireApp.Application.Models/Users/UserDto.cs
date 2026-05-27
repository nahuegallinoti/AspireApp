namespace AspireApp.Application.Models.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Name,
    string Surname,
    bool IsActive,
    bool EmailConfirmed,
    string? ExternalProvider,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? LastLoginUtc,
    IReadOnlyList<string> Roles);
