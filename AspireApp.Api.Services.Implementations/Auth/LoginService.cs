using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Implementations.Auth;

public class LoginService(ILoginServiceDependencies dependencies) : ILoginService
{
    private readonly ILoginServiceDependencies _loginDependencies = dependencies;

    public Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken cancellationToken = default)
    {
        return ValidateUser(user)
        .Bind(_loginDependencies.VerifyUserPassword)
        .Bind(_loginDependencies.CreateToken);
    }

    private static Result<UserLogin> ValidateUser(UserLogin userAccount)
    {
        var result = userAccount.Validate();

        return result;
    }
}
