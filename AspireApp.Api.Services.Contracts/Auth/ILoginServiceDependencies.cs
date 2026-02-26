using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Auth;

public interface ILoginServiceDependencies
{
    Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount, CancellationToken cancellationToken);
    Result<AuthenticationResult> CreateToken(UserLogin userAccount);
}