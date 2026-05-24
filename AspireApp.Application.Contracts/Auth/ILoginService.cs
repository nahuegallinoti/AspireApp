using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Auth;
public interface ILoginService
{
    Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken cancellationToken);
}