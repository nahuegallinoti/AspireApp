using System.Security.Cryptography;
using System.Text;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Implementations.User;

internal sealed class RegisterUserServiceDependencies(IUserDA userDA, UserMapper mapper) : IRegisterUserServiceDependencies
{
    public async Task<Result<Guid>> AddUserAsync(UserRegister userAccount, CancellationToken ct)
    {
        var entity = mapper.ToEntity(userAccount);

        (entity.PasswordHash, entity.PasswordSalt) = CreatePasswordHash(userAccount.Password);

        await userDA.AddAsync(entity, ct);
        await userDA.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<Result<UserRegister>> VerifyUserDoesNotExistAsync(UserRegister userAccount, CancellationToken ct) =>
        await userDA.ExistsAsync(userAccount.Email, ct)
            ? Result.Conflict<UserRegister>("Email already in use.")
            : userAccount;

    private static (byte[] hash, byte[] salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
    }
}
