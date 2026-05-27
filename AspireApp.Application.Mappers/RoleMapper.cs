using AspireApp.Application.Models.Roles;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Mappers;

public sealed class RoleMapper : BaseMapper<RoleDto, Role>
{
    public override RoleDto ToModel(Role entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.IsSystem);

    public override Role ToEntity(RoleDto model) => new()
    {
        Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
        Name = model.Name,
        NormalizedName = model.Name.ToUpperInvariant(),
        Description = model.Description,
        IsSystem = model.IsSystem
    };
}
