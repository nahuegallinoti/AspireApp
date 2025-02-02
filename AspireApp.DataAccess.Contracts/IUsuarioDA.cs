using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Contracts;

public interface IUsuarioDA : IBaseDA<User, Guid>
{
    Task<bool> UserExist(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
}
