using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;
using UserEntity = AspireApp.Domain.Entities.User;

namespace AspireApp.Application.Contracts.Auth;

public interface ILoginServiceDependencies
{
    Task<Result<UserEntity>> VerifyUserPasswordAsync(UserLogin userAccount, CancellationToken ct);
    Result<AuthenticationResult> CreateToken(UserEntity user);
}
