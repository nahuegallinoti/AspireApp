using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.RegisterUser;

public interface ILoginUserService
{
    Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken cancellationToken = default);
}