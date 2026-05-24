using AspireApp.Application.Models.Auth.User;
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
        Id = model.Id,
        Email = model.Email,
        Name = model.Name,
        Surname = model.Surname
    };
}
