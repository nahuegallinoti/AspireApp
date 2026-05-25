using System.Net;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Auth;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;
using UserEntity = AspireApp.Domain.Entities.User;

namespace AspireApp.Tests.Application.Auth;

public class LoginServiceTests
{
    private readonly ILoginServiceDependencies _deps = Substitute.For<ILoginServiceDependencies>();

    private LoginService Sut() => new(_deps);

    [Fact]
    public async Task LoginReturnsTokenWhenCredentialsAreValid()
    {
        var login = new UserLogin { Email = "nahuel@example.com", Password = "Strong!Password1" };
        var user = new UserEntity { Id = Guid.NewGuid(), Email = login.Email };
        var token = new AuthenticationResult("jwt");

        _deps.VerifyUserPasswordAsync(login, Arg.Any<CancellationToken>()).Returns(user.Success());
        _deps.CreateToken(user).Returns(token.Success());

        var result = await Sut().Login(login, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(token);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedWhenCredentialsAreInvalid()
    {
        var login = new UserLogin { Email = "nahuel@example.com", Password = "wrong" };

        _deps.VerifyUserPasswordAsync(login, Arg.Any<CancellationToken>())
            .Returns(Result.Unauthorized<UserEntity>());

        var result = await Sut().Login(login, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginShortCircuitsWhenPayloadIsInvalid()
    {
        var login = new UserLogin { Email = string.Empty, Password = string.Empty };

        var result = await Sut().Login(login, CancellationToken.None);

        result.Success.Should().BeFalse();
        await _deps.DidNotReceive().VerifyUserPasswordAsync(Arg.Any<UserLogin>(), Arg.Any<CancellationToken>());
    }
}
