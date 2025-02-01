using AspireApp.Application.Contracts.RegisterUser;
using AspireApp.Core.ROP;
using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AspireApp.Application.Implementations.RegisterUser;

public class RegisterUserServiceDependencies(IUsuarioDA usuarioDA) : IRegisterUserServiceDependencies
{
    private readonly IUsuarioDA _usuarioDA = usuarioDA;

    public async Task<Result<bool>> AddUser(Usuario userAccount)
    {
        await _usuarioDA.AddAsync(userAccount);
        await _usuarioDA.SaveChangesAsync();
        return true;
    }

    public (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();

        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (passwordHash, passwordSalt);
    }

    public async Task<Result<Usuario>> VerifyUserDoesNotExist(Usuario userAccount)
    {
        var exists = await _usuarioDA.UserExist(userAccount.Email);

        return exists ? Result.Failure<Usuario>("El usuario ya existe") : userAccount;
    }
}