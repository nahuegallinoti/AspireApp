using System.Net;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.User;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Tests.Application.User;

public class RegisterUserServiceTests
{
    private readonly IRegisterUserServiceDependencies _deps = Substitute.For<IRegisterUserServiceDependencies>();

    private RegisterUserService Sut() => new(_deps);

    [Fact]
    public async Task Register_returns_id_when_payload_is_valid_and_user_does_not_exist()
    {
        var input = new UserRegister
        {
            Email = "new@example.com",
            Password = "Strong!Password1",
            Name = "Nahuel",
            Surname = "Gallinoti"
        };
        var newId = Guid.NewGuid();

        _deps.VerifyUserDoesNotExistAsync(input, Arg.Any<CancellationToken>()).Returns(input.Success());
        _deps.AddUserAsync(input, Arg.Any<CancellationToken>()).Returns(newId.Success());

        var result = await Sut().RegisterAsync(input, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Value.Should().Be(newId);
    }

    [Fact]
    public async Task Register_returns_conflict_when_user_already_exists()
    {
        var input = new UserRegister
        {
            Email = "existing@example.com",
            Password = "Strong!Password1",
            Name = "Nahuel",
            Surname = "Gallinoti"
        };

        _deps.VerifyUserDoesNotExistAsync(input, Arg.Any<CancellationToken>())
            .Returns(Result.Conflict<UserRegister>("Email already in use."));

        var result = await Sut().RegisterAsync(input, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        await _deps.DidNotReceive().AddUserAsync(Arg.Any<UserRegister>(), Arg.Any<CancellationToken>());
    }
}
