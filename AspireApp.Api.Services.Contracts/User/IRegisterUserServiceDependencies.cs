using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserServiceDependencies
{
    Task<Result<UserRegister>> VerifyUserDoesNotExist(UserRegister userAccount);
    Task<Result<Guid>> AddUser(UserRegister userAccount);
}