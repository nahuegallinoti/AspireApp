using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Login;

public interface ILoginServiceDependencies
{
    Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount);
    Task<Result<string?>> CreateToken(UserLogin userAccount);
}