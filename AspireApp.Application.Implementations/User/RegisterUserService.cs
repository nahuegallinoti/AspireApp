using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Implementations.User;

internal sealed class RegisterUserService(IRegisterUserServiceDependencies dependencies) : IRegisterUserService
{
    public Task<Result<Guid>> RegisterAsync(UserRegister user, CancellationToken ct) =>
        user.Validate()
            .Bind(validated => dependencies.VerifyUserDoesNotExistAsync(validated, ct))
            .Bind(validated => dependencies.AddUserAsync(validated, ct));
}
