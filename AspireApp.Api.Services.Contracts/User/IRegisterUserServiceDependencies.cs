using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserServiceDependencies
{
    Task<Result<UserRegister>> VerifyUserDoesNotExist(UserRegister userAccount, CancellationToken cancellationToken);
    Task<Result<Guid>> AddUser(UserRegister userAccount, CancellationToken cancellationToken);
}