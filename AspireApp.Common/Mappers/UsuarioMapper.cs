using AspireApp.Api.Domain.Auth.User;
using AspireApp.Entidad;

namespace AspireApp.Core.Mappers;

public sealed class UsuarioMapper : BaseMapper<UserRegister, Usuario>
{
    public UsuarioMapper() { }

    public override UserRegister ToModel(Usuario entity)
    {
        if (entity is null) return new();

        return new UserRegister
        {
            Email = entity.Email
        };
    }

    public override Usuario ToEntity(UserRegister model)
    {
        if (model is null) return new();

        return new Usuario
        {
            Email = model.Email,
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Password = model.Password
        };
    }
}