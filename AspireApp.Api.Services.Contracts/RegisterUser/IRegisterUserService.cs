using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.RegisterUser;

public interface ILoginUserService
{
    Task<Result<string?>> Login(UserLogin user, CancellationToken cancellationToken = default);
}
