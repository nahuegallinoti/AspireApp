using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Login;
using AspireApp.Application.Contracts.RegisterUser;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Implementations.Login;

public class LoginService(ILoginServiceDependencies dependencies) : ILoginUserService
{
    private readonly ILoginServiceDependencies _loginDependencies = dependencies;

    public Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken cancellationToken = default)
    {
        return ValidateUser(user)
        .Bind(VerifyUserPassword)
        .Bind(CreateToken);
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

    private Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount) =>
        _loginDependencies.VerifyUserPassword(userAccount);

    private Result<AuthenticationResult> CreateToken(UserLogin userAccount) =>
        _loginDependencies.CreateToken(userAccount);

}
