using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserServiceDependencies
{
    Task<Result<UserRegister>> VerifyUserDoesNotExistAsync(UserRegister userAccount, CancellationToken ct);
    Task<Result<Guid>> AddUserAsync(UserRegister userAccount, CancellationToken ct);
}
