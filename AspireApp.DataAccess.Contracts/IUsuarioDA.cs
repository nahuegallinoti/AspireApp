using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Contracts;

public interface IUsuarioDA : IBaseDA<Usuario, Guid>
{
    Task<Usuario?> GetUserByEmail(string email);
    Task<bool> UserExist(string email);
}
