using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entidad;

namespace AspireApp.DataAccess.Contracts;

public interface IUsuarioDA : IBaseDA<Usuario>
{
    Task<Usuario?> GetUserByEmail(string email);
    Task<bool> UserExist(string email);
}
