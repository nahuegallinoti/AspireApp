using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.Entities;

namespace AspireApp.Core.Mappers;

public sealed class UsuarioMapper : BaseMapper<UserRegister, User>
{
    public UsuarioMapper() { }

    public override UserRegister ToModel(User entity)
    {
        if (entity is null) return new();

        return new UserRegister
        {
            Email = entity.Email
        };
    }

    public override User ToEntity(UserRegister model)
    {
        if (model is null) return new();

        return new User
        {
            Id = model.Id,
            Email = model.Email,
            Name = model.Name,
            Surname = model.Surname,
            Password = model.Password
        };
    }
}