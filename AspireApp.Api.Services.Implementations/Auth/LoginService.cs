using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Auth;
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
        List<string> errores = [];

        if (string.IsNullOrWhiteSpace(userAccount.Email))
            errores.Add("El email no debe estar vacio");

        if (string.IsNullOrWhiteSpace(userAccount.Password))
            errores.Add("El password no debe estar vacio");

        return errores.Count is not 0
            ? Result.Failure<UserLogin>([.. errores])
            : userAccount;
    }
}
