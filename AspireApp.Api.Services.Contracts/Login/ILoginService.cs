using AspireApp.Core.ROP;
using AspireApp.Entidad;

namespace AspireApp.Application.Contracts.Login;

public interface IRegisterUserService
{
    Task<Result<Usuario>> AddUser(Usuario usuario, CancellationToken cancellationToken = default);
}
