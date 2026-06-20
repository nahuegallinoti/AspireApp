using System.Net;
using AspireApp.Application.Implementations.Roles;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Roles;
using AspireApp.Application.Persistence;
using AspireApp.Domain.Entities;

namespace AspireApp.Tests.Application.Roles;

public class RoleServiceTests
{
    private readonly IRoleDA _roleDA = Substitute.For<IRoleDA>();
    private readonly RoleService _sut;
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    public RoleServiceTests() => _sut = new RoleService(_roleDA, new RoleMapper());

    [Fact]
    public async Task GetAllMapsEveryRole()
    {
        _roleDA.GetAllAsync(Ct).Returns([NewRole(name: "A"), NewRole(name: "B")]);

        var result = await _sut.GetAllAsync(Ct);

        result.Select(x => x.Name).Should().Equal("A", "B");
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoRolesExist()
    {
        _roleDA.GetAllAsync(Ct).Returns([]);

        (await _sut.GetAllAsync(Ct)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundWhenRoleDoesNotExist()
    {
        var id = Guid.NewGuid();
        _roleDA.GetByIdAsync(id, Ct).Returns((Role?)null);

        var result = await _sut.GetByIdAsync(id, Ct);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdReturnsMappedRoleWhenItExists()
    {
        var role = NewRole();
        _roleDA.GetByIdAsync(role.Id, Ct).Returns(role);

        var result = await _sut.GetByIdAsync(role.Id, Ct);

        result.Success.Should().BeTrue();
        result.Value.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task CreateRejectsInvalidRequestWithoutAccessingDataStore()
    {
        var result = await _sut.CreateAsync(new CreateRoleRequest { Name = "" }, Ct);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _roleDA.DidNotReceive().ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _roleDA.DidNotReceive().AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReturnsConflictWhenNameExists()
    {
        _roleDA.ExistsAsync("Admin", Ct).Returns(true);

        var result = await _sut.CreateAsync(new CreateRoleRequest { Name = "Admin" }, Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        await _roleDA.DidNotReceive().AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTrimsAndNormalizesNameAndPersistsRole()
    {
        var request = new CreateRoleRequest { Name = "  Admin  ", Description = "Access" };

        var result = await _sut.CreateAsync(request, Ct);

        result.Success.Should().BeTrue();
        await _roleDA.Received(1).AddAsync(Arg.Is<Role>(role =>
            role.Name == "Admin" && role.NormalizedName == "ADMIN" && !role.IsSystem), Ct);
        await _roleDA.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundWhenRoleDoesNotExist()
    {
        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateRoleRequest(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRejectsSystemRoleWithoutPersisting()
    {
        var role = NewRole(isSystem: true);
        _roleDA.GetByIdAsync(role.Id, Ct).Returns(role);

        var result = await _sut.UpdateAsync(role.Id, new UpdateRoleRequest { Description = "New" }, Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Errors.Should().Contain("System roles cannot be modified.");
        _roleDA.DidNotReceive().Update(Arg.Any<Role>());
        await _roleDA.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateChangesDescriptionAndPersistsRole()
    {
        var role = NewRole();
        _roleDA.GetByIdAsync(role.Id, Ct).Returns(role);

        var result = await _sut.UpdateAsync(role.Id, new UpdateRoleRequest { Description = "New" }, Ct);

        result.Success.Should().BeTrue();
        role.Description.Should().Be("New");
        _roleDA.Received(1).Update(role);
        await _roleDA.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task DeleteReturnsNotFoundWhenRoleDoesNotExist()
    {
        var result = await _sut.DeleteAsync(Guid.NewGuid(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRejectsSystemRole()
    {
        var role = NewRole(isSystem: true);
        _roleDA.GetByIdAsync(role.Id, Ct).Returns(role);

        var result = await _sut.DeleteAsync(role.Id, Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        _roleDA.DidNotReceive().Delete(Arg.Any<Role>());
    }

    [Fact]
    public async Task DeleteRemovesAndPersistsRegularRole()
    {
        var role = NewRole();
        _roleDA.GetByIdAsync(role.Id, Ct).Returns(role);

        var result = await _sut.DeleteAsync(role.Id, Ct);

        result.Success.Should().BeTrue();
        _roleDA.Received(1).Delete(role);
        await _roleDA.Received(1).SaveChangesAsync(Ct);
    }

    private static Role NewRole(bool isSystem = false, string name = "Editor") => new()
    {
        Id = Guid.NewGuid(), Name = name, NormalizedName = name.ToUpperInvariant(), IsSystem = isSystem
    };
}
