using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Roles;
using AspireApp.Domain.Entities;

namespace AspireApp.Tests.Application.Mappers;

public class RoleMapperTests
{
    private readonly RoleMapper _mapper = new();

    [Fact]
    public void ToModelCopiesAllProperties()
    {
        var id = Guid.NewGuid();
        var model = _mapper.ToModel(new Role { Id = id, Name = "Admin", Description = "Full access", IsSystem = true });

        model.Should().Be(new RoleDto(id, "Admin", "Full access", true));
    }

    [Fact]
    public void ToEntityGeneratesIdAndNormalizesName()
    {
        var entity = _mapper.ToEntity(new RoleDto(Guid.Empty, "Editor", "Edits", false));

        entity.Id.Should().NotBeEmpty();
        entity.NormalizedName.Should().Be("EDITOR");
    }

    [Fact]
    public void ToEntityPreservesProvidedId()
    {
        var id = Guid.NewGuid();
        _mapper.ToEntity(new RoleDto(id, "User", null, false)).Id.Should().Be(id);
    }

    [Fact]
    public void RoundTripPreservesRoleData()
    {
        var model = new RoleDto(Guid.NewGuid(), "Admin", "Full access", true);

        _mapper.ToModel(_mapper.ToEntity(model)).Should().Be(model);
    }
}
