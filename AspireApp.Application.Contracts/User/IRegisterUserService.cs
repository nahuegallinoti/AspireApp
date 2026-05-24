using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserService
{
    Task<Result<Guid>> RegisterAsync(UserRegister user, CancellationToken ct);
}
