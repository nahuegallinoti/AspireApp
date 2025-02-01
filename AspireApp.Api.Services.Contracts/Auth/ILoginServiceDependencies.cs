using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Auth;

public interface ILoginServiceDependencies
{
    Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount);
    Result<AuthenticationResult> CreateToken(UserLogin userAccount);
}