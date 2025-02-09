using AspireApp.Api.Models.Auth;
using AspireApp.Api.Models.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.Auth;
public interface ILoginService
{
    Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken cancellationToken);
}