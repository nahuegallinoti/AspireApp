using AspireApp.Api.Domain.Auth.User;
using AspireApp.Core.ROP;

namespace AspireApp.Application.Contracts.User;

public interface IRegisterUserService : IBaseService
{
    Task<Result<Guid>> AddUser(UserRegister usuario, CancellationToken cancellationToken = default);
}
