using System.Net;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Users;
using AspireApp.Application.Models.Users;
using AspireApp.Application.Persistence;
using AspireApp.Domain.Entities;

namespace AspireApp.Tests.Application.Users;

public class UserServiceTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddDays(10);
    private readonly IUserDA _userDA = Substitute.For<IUserDA>();
    private readonly IRoleDA _roleDA = Substitute.For<IRoleDA>();
    private readonly IRefreshTokenDA _refreshTokenDA = Substitute.For<IRefreshTokenDA>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly UserService _sut;
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    public UserServiceTests()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(Now);
        _sut = new UserService(_userDA, _roleDA, _refreshTokenDA, _passwordHasher, timeProvider);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundWhenUserDoesNotExist()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdReturnsMappedUserWhenItExists()
    {
        var user = NewUser();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = "Admin" } });
        _userDA.GetByIdWithRolesAsync(user.Id, Ct).Returns(user);

        var result = await _sut.GetByIdAsync(user.Id, Ct);

        result.Success.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Roles.Should().Equal("Admin");
    }

    [Theory]
    [InlineData(0, 10, 1, 10, 0)]
    [InlineData(-5, 0, 1, 1, 0)]
    [InlineData(3, 9999, 3, 200, 400)]
    [InlineData(2, -1, 2, 1, 1)]
    public async Task ListClampsPaginationAndUsesCanonicalSkip(int page, int pageSize, int expectedPage, int expectedSize, int expectedSkip)
    {
        _userDA.ListWithRolesAsync(expectedSkip, expectedSize, "find", Ct).Returns([NewUser(), NewUser()]);
        _userDA.CountAsync("find", Ct).Returns(12);

        var result = await _sut.ListAsync(page, pageSize, "find", Ct);

        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedSize);
        result.Total.Should().Be(12);
        result.Items.Should().HaveCount(2);
        await _userDA.Received(1).ListWithRolesAsync(expectedSkip, expectedSize, "find", Ct);
    }

    [Fact]
    public async Task UpdateRejectsInvalidRequestWithoutLoadingUser()
    {
        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateUserRequest(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _userDA.DidNotReceive().GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReturnsNotFoundWhenUserDoesNotExist()
    {
        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTrimsFieldsSetsStateAndPersists()
    {
        var user = NewUser();
        _userDA.GetByIdWithRolesAsync(user.Id, Ct).Returns(user);

        var result = await _sut.UpdateAsync(user.Id, new UpdateUserRequest
        {
            Name = "  Grace ", Surname = " Hopper  ", IsActive = false
        }, Ct);

        result.Success.Should().BeTrue();
        user.Name.Should().Be("Grace");
        user.Surname.Should().Be("Hopper");
        user.IsActive.Should().BeFalse();
        user.UpdatedUtc.Should().Be(Now);
        _userDA.Received(1).Update(user);
        await _userDA.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task UpdateProfileChangesNamesWithoutChangingActiveState()
    {
        var user = NewUser();
        user.IsActive = false;
        _userDA.GetByIdWithRolesAsync(user.Id, Ct).Returns(user);

        var result = await _sut.UpdateProfileAsync(user.Id, new UpdateUserProfileRequest
        {
            Name = "  Grace ", Surname = " Hopper  "
        }, Ct);

        result.Success.Should().BeTrue();
        user.Name.Should().Be("Grace");
        user.Surname.Should().Be("Hopper");
        user.IsActive.Should().BeFalse();
        user.UpdatedUtc.Should().Be(Now);
    }

    [Fact]
    public async Task DeleteReturnsNotFoundWithoutRevokingTokens()
    {
        var result = await _sut.DeleteAsync(Guid.NewGuid(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
        await _refreshTokenDA.DidNotReceive().RevokeAllForUserAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRevokesTokensBeforeDeletingAndPersists()
    {
        var user = NewUser();
        _userDA.GetByIdAsync(user.Id, Ct).Returns(user);

        var result = await _sut.DeleteAsync(user.Id, Ct);

        result.Success.Should().BeTrue();
        Received.InOrder(() =>
        {
            _ = _refreshTokenDA.RevokeAllForUserAsync(user.Id, "UserDeleted", null, Ct);
            _userDA.Delete(user);
        });
        await _userDA.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task AssignRolesRejectsInvalidRequest()
    {
        var result = await _sut.AssignRolesAsync(Guid.NewGuid(), new AssignRolesRequest(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignRolesReturnsNotFoundWhenUserDoesNotExist()
    {
        var result = await _sut.AssignRolesAsync(Guid.NewGuid(), new AssignRolesRequest { Roles = ["Admin"] }, Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRolesReportsUnknownRolesWithoutModifyingUser()
    {
        var user = NewUser();
        var existing = new UserRole { RoleId = Guid.NewGuid() };
        user.UserRoles.Add(existing);
        _userDA.GetByIdWithRolesAsync(user.Id, Ct).Returns(user);
        _roleDA.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Ct)
            .Returns([new Role { Id = Guid.NewGuid(), Name = "Admin" }]);

        var result = await _sut.AssignRolesAsync(user.Id, new AssignRolesRequest { Roles = ["Admin", "Missing"] }, Ct);

        result.Errors.Single().Should().Contain("Unknown role(s):").And.Contain("Missing");
        user.UserRoles.Should().ContainSingle().Which.Should().BeSameAs(existing);
        _userDA.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task AssignRolesMatchesNamesCaseInsensitivelyAndReplacesRoles()
    {
        var user = NewUser();
        user.UserRoles.Add(new UserRole { RoleId = Guid.NewGuid() });
        var admin = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        _userDA.GetByIdWithRolesAsync(user.Id, Ct).Returns(user);
        _roleDA.GetByNamesAsync(Arg.Any<IEnumerable<string>>(), Ct).Returns([admin]);

        var result = await _sut.AssignRolesAsync(user.Id, new AssignRolesRequest { Roles = ["admin"] }, Ct);

        result.Success.Should().BeTrue();
        user.UserRoles.Should().ContainSingle(x => x.UserId == user.Id && x.RoleId == admin.Id);
        _userDA.Received(1).Update(user);
        await _userDA.Received(1).SaveChangesAsync(Ct);
        await _refreshTokenDA.Received(1).RevokeAllForUserAsync(user.Id, "RolesChanged", null, Ct);
    }

    [Fact]
    public async Task ChangePasswordRejectsInvalidRequest()
    {
        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), new ChangePasswordRequest
        {
            CurrentPassword = "old", NewPassword = "short"
        }, Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePasswordReturnsNotFoundWhenUserDoesNotExist()
    {
        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), ValidPasswordChange(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePasswordRejectsSingleSignOnAccountWithoutHashing()
    {
        var user = NewUser(hasPassword: false);
        _userDA.GetByIdAsync(user.Id, Ct).Returns(user);

        var result = await _sut.ChangePasswordAsync(user.Id, ValidPasswordChange(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Errors.Single().Should().Contain("single sign-on");
        _passwordHasher.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Fact]
    public async Task ChangePasswordReturnsUnauthorizedWhenCurrentPasswordIsWrong()
    {
        var user = NewUser();
        _userDA.GetByIdAsync(user.Id, Ct).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<int>()).Returns(false);

        var result = await _sut.ChangePasswordAsync(user.Id, ValidPasswordChange(), Ct);

        result.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await _userDA.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordUpdatesHashPersistsAndRevokesTokens()
    {
        var user = NewUser();
        var newHash = new byte[] { 3 };
        var newSalt = new byte[] { 4 };
        _userDA.GetByIdAsync(user.Id, Ct).Returns(user);
        _passwordHasher.Verify("current-password", user.PasswordHash!, user.PasswordSalt!, user.PasswordIterations).Returns(true);
        _passwordHasher.Hash("new-password").Returns((newHash, newSalt, 42));

        var result = await _sut.ChangePasswordAsync(user.Id, ValidPasswordChange(), Ct);

        result.Success.Should().BeTrue();
        user.PasswordHash.Should().BeSameAs(newHash);
        user.PasswordSalt.Should().BeSameAs(newSalt);
        user.PasswordIterations.Should().Be(42);
        user.UpdatedUtc.Should().Be(Now);
        _userDA.Received(1).Update(user);
        await _userDA.Received(1).SaveChangesAsync(Ct);
        await _refreshTokenDA.Received(1).RevokeAllForUserAsync(user.Id, "PasswordChanged", null, Ct);
    }

    private static User NewUser(bool hasPassword = true) => new()
    {
        Id = Guid.NewGuid(), Email = "user@example.com", Name = "User", Surname = "Test",
        IsActive = true, EmailConfirmed = true, UserRoles = [],
        PasswordHash = hasPassword ? [1] : null,
        PasswordSalt = hasPassword ? [2] : null,
        PasswordIterations = hasPassword ? 10 : 0
    };

    private static UpdateUserRequest ValidUpdate() => new() { Name = "Name", Surname = "Surname", IsActive = true };

    private static ChangePasswordRequest ValidPasswordChange() => new()
    {
        CurrentPassword = "current-password", NewPassword = "new-password"
    };
}
