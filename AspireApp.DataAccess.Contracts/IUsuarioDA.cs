using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Contracts;

public interface IUsuarioDA : IBaseDA<User, Guid>
{
    Task<User?> GetUserByEmail(string email);
    Task<bool> UserExist(string email);
}
