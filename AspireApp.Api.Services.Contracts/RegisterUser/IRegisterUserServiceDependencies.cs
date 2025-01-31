using AspireApp.Core.ROP;
using AspireApp.Entidad;

namespace AspireApp.Application.Contracts.RegisterUser;

public interface IRegisterUserServiceDependencies
{
    Task<Result<Usuario>> VerifyUserDoesNotExist(Usuario userAccount);
    Task<Result<bool>> AddUser(Usuario userAccount);
    (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password);
}