using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class LoginService(ILoginServiceDependencies dependencies) : ILoginService
{
    public Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken ct) =>
        user.Validate()
            .Bind(validated => dependencies.VerifyUserPasswordAsync(validated, ct))
            .Bind(dependencies.CreateToken);
}
