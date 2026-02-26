using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface IUsuarioDA : IBaseDA<User, Guid>
{
    Task<bool> UserExist(string email, CancellationToken cancellationToken);
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
}
