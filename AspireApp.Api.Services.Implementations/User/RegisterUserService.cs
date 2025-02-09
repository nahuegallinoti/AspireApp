using AspireApp.Api.Models.Auth.User;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Implementations.User;

public class RegisterUserService(IRegisterUserServiceDependencies dependencies) : IRegisterUserService
{
    private readonly IRegisterUserServiceDependencies _registerUserDependencies = dependencies;

    public Task<Result<Guid>> AddUser(UserRegister usuario, CancellationToken cancellationToken = default)
    {
        return ValidateUser(usuario)
        .Bind(user => _registerUserDependencies.VerifyUserDoesNotExist(user, cancellationToken))
        .Bind(user => _registerUserDependencies.AddUser(user, cancellationToken));
    }

    private static Result<UserRegister> ValidateUser(UserRegister userAccount)
    {
        var result = userAccount.Validate();

        return result;
    }
}
