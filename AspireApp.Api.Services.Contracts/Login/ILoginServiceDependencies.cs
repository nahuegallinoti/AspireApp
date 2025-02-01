using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Login;

public interface ILoginServiceDependencies : IBaseService
{
    Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount);
    Task<Result<AuthenticationResult>> CreateToken(UserLogin userAccount);
}