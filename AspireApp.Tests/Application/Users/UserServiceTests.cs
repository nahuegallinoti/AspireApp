using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Users;
using AspireApp.Application.Models.Users;
using AspireApp.Application.Persistence;
using AspireApp.Domain.Entities;

namespace AspireApp.Tests.Application.Users;

public class UserServiceTests
{
    private static (UserService sut,
                    IUserDA userDA,
                    IRoleDA roleDA,
                    IRefreshTokenDA refreshTokenDA,
                    IPasswordHasher hasher,
                    TimeProvider time) Build()
    {
        var userDA = Substitute.For<IUserDA>();
        var roleDA = Substitute.For<IRoleDA>();
        var refreshTokenDA = Substitute.For<IRefreshTokenDA>();
        var hasher = Substitute.For<IPasswordHasher>();
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var sut = new UserService(userDA, roleDA, refreshTokenDA, hasher, time);
        return (sut, userDA, roleDA, refreshTokenDA, hasher, time);
    }

    private static User MakeUser(params string[] roleNames)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "u@example.com",
            Name = "Ana",
            Surname = "López",
            IsActive = true,
            PasswordHash = [1, 2, 3],
            PasswordSalt = [4, 5, 6],
            PasswordIterations = 10_000,
            CreatedUtc = DateTimeOffset.UtcNow,
            UserRoles = []
        };

        foreach (var name in roleNames)
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                Role = new Role { Id = Guid.NewGuid(), Name = name }
            });

        return user;
    }

    [Fact]
    public async Task GetById_WhenUserExists_ReturnsDto()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var user = MakeUser("Admin");
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await sut.GetByIdAsync(user.Id, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Value.Email.Should().Be("u@example.com");
        result.Value.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task GetById_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        userDA.GetByIdWithRolesAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.GetByIdAsync(id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_NormalizesPageAndPageSize()
    {
        var (sut, userDA, _, _, _, _) = Build();
        IReadOnlyList<User> users = [MakeUser(), MakeUser()];
        userDA.ListWithRolesAsync(0, 200, null, CancellationToken.None).Returns(users);
        userDA.CountAsync(null, CancellationToken.None).Returns(2);

        await sut.ListAsync(0, 500, null, CancellationToken.None);

        await userDA.Received(1).ListWithRolesAsync(0, 200, null, CancellationToken.None);
    }

    [Fact]
    public async Task List_ReturnsCorrectPageMetadata()
    {
        var (sut, userDA, _, _, _, _) = Build();
        IReadOnlyList<User> users = [MakeUser()];
        userDA.ListWithRolesAsync(10, 10, null, CancellationToken.None).Returns(users);
        userDA.CountAsync(null, CancellationToken.None).Returns(25);

        var result = await sut.ListAsync(2, 10, null, CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(25);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Update_WhenValid_PersistsAndReturnsDto()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var user = MakeUser();
        var request = new UpdateUserRequest { Name = "Pedro", Surname = "Ruiz", IsActive = false };
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await sut.UpdateAsync(user.Id, request, CancellationToken.None);

        user.Name.Should().Be("Pedro");
        user.Surname.Should().Be("Ruiz");
        user.IsActive.Should().BeFalse();
        userDA.Received(1).Update(user);
        await userDA.Received(1).SaveChangesAsync(CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        var request = new UpdateUserRequest { Name = "Pedro", Surname = "Ruiz" };
        userDA.GetByIdWithRolesAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.UpdateAsync(id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_TrimsNameAndSurname()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var user = MakeUser();
        var request = new UpdateUserRequest { Name = "  Bob  ", Surname = " Mar " };
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);

        await sut.UpdateAsync(user.Id, request, CancellationToken.None);

        user.Name.Should().Be("Bob");
        user.Surname.Should().Be("Mar");
    }

    [Fact]
    public async Task Update_WhenNameEmpty_ReturnsValidationFailure()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var request = new UpdateUserRequest { Name = "", Surname = "Ruiz" };

        var result = await sut.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        userDA.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Update_WhenNameTooLong_ReturnsValidationFailure()
    {
        var (sut, _, _, _, _, _) = Build();
        var request = new UpdateUserRequest { Name = new string('x', 129), Surname = "Ruiz" };

        var result = await sut.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_WhenValid_DoesNotChangeIsActive()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var user = MakeUser();
        user.IsActive = false;
        var request = new UpdateUserProfileRequest { Name = "X", Surname = "Y" };
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        user.IsActive.Should().BeFalse();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfile_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        var request = new UpdateUserProfileRequest { Name = "X", Surname = "Y" };
        userDA.GetByIdWithRolesAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.UpdateProfileAsync(id, request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProfile_WhenNameEmpty_ReturnsValidationFailure()
    {
        var (sut, _, _, _, _, _) = Build();
        var request = new UpdateUserProfileRequest { Name = "", Surname = "Y" };

        var result = await sut.UpdateProfileAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenUserExists_RevokesTokensAndDeletes()
    {
        var (sut, userDA, _, refreshTokenDA, _, _) = Build();
        var user = MakeUser();
        userDA.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await sut.DeleteAsync(user.Id, CancellationToken.None);

        await refreshTokenDA.Received(1).RevokeAllForUserAsync(user.Id, "UserDeleted", null, CancellationToken.None);
        userDA.Received(1).Delete(user);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        userDA.GetByIdAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.DeleteAsync(id, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        userDA.DidNotReceive().Delete(Arg.Any<User>());
    }

    [Fact]
    public async Task AssignRoles_WhenValid_ReplacesRolesAndRevokesTokens()
    {
        var (sut, userDA, roleDA, refreshTokenDA, _, _) = Build();
        var user = MakeUser("Admin");
        var request = new AssignRolesRequest { Roles = ["User", "Manager"] };
        IReadOnlyList<Role> roles =
        [
            new() { Id = Guid.NewGuid(), Name = "User" },
            new() { Id = Guid.NewGuid(), Name = "Manager" }
        ];
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);
        roleDA.GetByNamesAsync(request.Roles, CancellationToken.None).Returns(roles);

        var result = await sut.AssignRolesAsync(user.Id, request, CancellationToken.None);

        user.UserRoles.Should().HaveCount(2);
        await refreshTokenDA.Received(1).RevokeAllForUserAsync(user.Id, "RolesChanged", null, CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoles_WhenRoleMissing_ReturnsFailure()
    {
        var (sut, userDA, roleDA, _, _, _) = Build();
        var user = MakeUser();
        var request = new AssignRolesRequest { Roles = ["User", "Manager"] };
        IReadOnlyList<Role> roles = [new() { Id = Guid.NewGuid(), Name = "User" }];
        userDA.GetByIdWithRolesAsync(user.Id, CancellationToken.None).Returns(user);
        roleDA.GetByNamesAsync(request.Roles, CancellationToken.None).Returns(roles);

        var result = await sut.AssignRolesAsync(user.Id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(error => error.Contains("Unknown role(s)"));
    }

    [Fact]
    public async Task AssignRoles_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        var request = new AssignRolesRequest { Roles = ["User"] };
        userDA.GetByIdWithRolesAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.AssignRolesAsync(id, request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRoles_WhenRolesListEmpty_ReturnsValidationFailure()
    {
        var (sut, _, _, _, _, _) = Build();
        var request = new AssignRolesRequest { Roles = [] };

        var result = await sut.AssignRolesAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WhenValid_UpdatesHashAndRevokesTokens()
    {
        var (sut, userDA, _, refreshTokenDA, hasher, _) = Build();
        var user = MakeUser();
        var request = new ChangePasswordRequest { CurrentPassword = "old-password", NewPassword = "new-password" };
        byte[] newHash = [7, 8, 9];
        byte[] newSalt = [10, 11, 12];
        userDA.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);
        hasher.Verify(request.CurrentPassword, user.PasswordHash!, user.PasswordSalt!, user.PasswordIterations).Returns(true);
        hasher.Hash(request.NewPassword).Returns((newHash, newSalt, 12_000));

        var result = await sut.ChangePasswordAsync(user.Id, request, CancellationToken.None);

        user.PasswordHash.Should().BeSameAs(newHash);
        await refreshTokenDA.Received(1).RevokeAllForUserAsync(user.Id, "PasswordChanged", null, CancellationToken.None);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_WhenUserNotFound_ReturnsNotFound()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var id = Guid.NewGuid();
        var request = new ChangePasswordRequest { CurrentPassword = "old-password", NewPassword = "new-password" };
        userDA.GetByIdAsync(id, CancellationToken.None).Returns((User?)null);

        var result = await sut.ChangePasswordAsync(id, request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_WhenUserIsExternal_ReturnsConflict()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var user = MakeUser();
        user.PasswordHash = null;
        var request = new ChangePasswordRequest { CurrentPassword = "old-password", NewPassword = "new-password" };
        userDA.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);

        var result = await sut.ChangePasswordAsync(user.Id, request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ChangePassword_WhenCurrentPasswordWrong_ReturnsUnauthorized()
    {
        var (sut, userDA, _, _, hasher, _) = Build();
        var user = MakeUser();
        var request = new ChangePasswordRequest { CurrentPassword = "wrong-password", NewPassword = "new-password" };
        userDA.GetByIdAsync(user.Id, CancellationToken.None).Returns(user);
        hasher.Verify(request.CurrentPassword, user.PasswordHash!, user.PasswordSalt!, user.PasswordIterations).Returns(false);

        var result = await sut.ChangePasswordAsync(user.Id, request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WhenNewPasswordTooShort_ReturnsValidationFailure()
    {
        var (sut, userDA, _, _, _, _) = Build();
        var request = new ChangePasswordRequest { CurrentPassword = "old-password", NewPassword = "1234567" };

        var result = await sut.ChangePasswordAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        userDA.DidNotReceive().Update(Arg.Any<User>());
    }
}
