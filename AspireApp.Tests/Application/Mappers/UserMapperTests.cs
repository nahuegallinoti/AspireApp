using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.Entities;

namespace AspireApp.Tests.Application.Mappers;

public class UserMapperTests
{
    private readonly UserMapper _mapper = new();

    [Fact]
    public void ToModelCopiesRegistrationProperties()
    {
        var entity = new User { Id = Guid.NewGuid(), Email = "a@b.com", Name = "A", Surname = "B" };

        _mapper.ToModel(entity).Should().BeEquivalentTo(new UserRegister
        {
            Id = entity.Id, Email = entity.Email, Name = entity.Name, Surname = entity.Surname
        }, options => options.Excluding(x => x.Password));
    }

    [Fact]
    public void ToEntityGeneratesIdAndTrimsAndNormalizesStrings()
    {
        var entity = _mapper.ToEntity(new UserRegister
        {
            Email = "  User@Example.com  ", Name = "  Ada ", Surname = " Lovelace  ", Password = "password"
        });

        entity.Id.Should().NotBeEmpty();
        entity.Email.Should().Be("User@Example.com");
        entity.NormalizedEmail.Should().Be("USER@EXAMPLE.COM");
        entity.Name.Should().Be("Ada");
        entity.Surname.Should().Be("Lovelace");
    }

    [Fact]
    public void ToEntityPreservesProvidedId()
    {
        var id = Guid.NewGuid();
        var model = new UserRegister { Id = id, Email = "a@b.com", Name = "A", Surname = "B", Password = "password" };

        _mapper.ToEntity(model).Id.Should().Be(id);
    }

    [Fact]
    public void ToDtoProjectsValidRoleNamesAndCopiesUserProperties()
    {
        var created = DateTimeOffset.UnixEpoch;
        var login = created.AddDays(1);
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "a@b.com", Name = "A", Surname = "B",
            IsActive = false, EmailConfirmed = true, ExternalProvider = "oidc",
            CreatedUtc = created, LastLoginUtc = login,
            UserRoles =
            [
                new UserRole { Role = new Role { Name = "Admin" } },
                new UserRole { Role = new Role { Name = " " } },
                new UserRole { Role = null }
            ]
        };

        var dto = UserMapper.ToDto(user);

        dto.Roles.Should().Equal("Admin");
        dto.Should().BeEquivalentTo(new
        {
            user.Id, user.Email, user.Name, user.Surname, user.IsActive, user.EmailConfirmed,
            user.ExternalProvider, user.CreatedUtc, user.LastLoginUtc
        });
    }

    [Fact]
    public void ToDtoHandlesNullUserRoles()
    {
        var user = new User();
        user.UserRoles = null!;

        UserMapper.ToDto(user).Roles.Should().BeEmpty();
    }
}
