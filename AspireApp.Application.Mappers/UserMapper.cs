using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Mappers;

public sealed class UserMapper : BaseMapper<UserRegister, User>
{
    public override UserRegister ToModel(User entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        Name = entity.Name,
        Surname = entity.Surname
    };

    public override User ToEntity(UserRegister model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Email = model.Email.Trim(),
        NormalizedEmail = model.Email.Trim().ToUpperInvariant(),
        Name = model.Name.Trim(),
        Surname = model.Surname.Trim()
    };

    public static UserDto ToDto(User entity)
    {
        var roles = entity.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty)
                                     .Where(r => !string.IsNullOrWhiteSpace(r))
                                     .ToList() ?? [];

        return new UserDto(
            entity.Id,
            entity.Email,
            entity.Name,
            entity.Surname,
            entity.IsActive,
            entity.EmailConfirmed,
            entity.ExternalProvider,
            entity.CreatedUtc,
            entity.LastLoginUtc,
            roles);
    }
}
