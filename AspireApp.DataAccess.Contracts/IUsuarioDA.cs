using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Contracts;

public interface IUsuarioDA : IBaseDA<Usuario>
{
    Task<Usuario?> GetUserByEmail(string email);
    Task<bool> UserExist(string email);
}
