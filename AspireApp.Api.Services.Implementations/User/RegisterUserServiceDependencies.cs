using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.User;
using AspireApp.Core.Mappers;
using AspireApp.Core.ROP;
using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AspireApp.Application.Implementations.User;

public class RegisterUserServiceDependencies(IUsuarioDA usuarioDA, UsuarioMapper usuarioMapper) : IRegisterUserServiceDependencies
{
    private readonly IUsuarioDA _usuarioDA = usuarioDA;
    private readonly UsuarioMapper _usuarioMapper = usuarioMapper;

    public async Task<Result<Guid>> AddUser(UserRegister userAccount)
    {
        Usuario usuario = _usuarioMapper.ToEntity(userAccount);

        (byte[] passwordHash, byte[] passwordSalt) = CreatePasswordHash(userAccount.Password);

        usuario.PasswordHash = passwordHash;
        usuario.PasswordSalt = passwordSalt;

        await _usuarioDA.AddAsync(usuario);
        await _usuarioDA.SaveChangesAsync();

        return usuario.Id;
    }

    public static (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();

        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (passwordHash, passwordSalt);
    }

    public async Task<Result<UserRegister>> VerifyUserDoesNotExist(UserRegister userAccount)
    {
        var exists = await _usuarioDA.UserExist(userAccount.Email);

        return exists ? Result.Failure<UserRegister>("El usuario ya existe") : userAccount;
    }
}