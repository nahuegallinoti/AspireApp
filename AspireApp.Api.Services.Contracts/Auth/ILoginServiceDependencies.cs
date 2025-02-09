using AspireApp.Api.Models.Auth;
using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Auth;

public interface ILoginServiceDependencies
{
    Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount, CancellationToken cancellationToken);
    Result<AuthenticationResult> CreateToken(UserLogin userAccount);
}