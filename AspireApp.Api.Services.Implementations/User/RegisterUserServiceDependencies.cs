using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.User;
using AspireApp.Core.Mappers;
using AspireApp.Core.ROP;
using AspireApp.DataAccess.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace AspireApp.Application.Implementations.User;

public class RegisterUserServiceDependencies(IUsuarioDA usuarioDA, UsuarioMapper usuarioMapper) : IRegisterUserServiceDependencies
{
    private readonly IUsuarioDA _usuarioDA = usuarioDA;
    private readonly UsuarioMapper _usuarioMapper = usuarioMapper;

    public async Task<Result<Guid>> AddUser(UserRegister userAccount, CancellationToken cancellationToken)
    {
        Entities.User usuario = _usuarioMapper.ToEntity(userAccount);

        (byte[] passwordHash, byte[] passwordSalt) = CreatePasswordHash(userAccount.Password);

        usuario.PasswordHash = passwordHash;
        usuario.PasswordSalt = passwordSalt;

        await _usuarioDA.AddAsync(usuario, cancellationToken);
        await _usuarioDA.SaveChangesAsync(cancellationToken);

        return usuario.Id;
    }

    private static (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();

        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (passwordHash, passwordSalt);
    }

    public async Task<Result<UserRegister>> VerifyUserDoesNotExist(UserRegister userAccount, CancellationToken cancellationToken)
    {
        var exists = await _usuarioDA.UserExist(userAccount.Email, cancellationToken);

        return exists ? Result.Failure<UserRegister>("El usuario ya existe") : userAccount;
    }
}