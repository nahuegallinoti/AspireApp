using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Login;
using AspireApp.Core.ROP;
using AspireApp.DataAccess.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AspireApp.Application.Implementations.Login;

public class LoginServiceDependencies(IConfiguration configuration, IUsuarioDA usuarioDA) : ILoginServiceDependencies
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IUsuarioDA _usuarioDA = usuarioDA;

    public async Task<Result<UserLogin>> VerifyUserPassword(UserLogin userAccount)
    {
        var usuario = await _usuarioDA.GetUserByEmail(userAccount.Email);

        if (usuario is null || !VerifyPasswordHash(userAccount.Password, usuario.PasswordHash, usuario.PasswordSalt))
            return Result.Failure<UserLogin>("El usuario es inválido");

        return userAccount;
    }

    public Task<Result<string?>> CreateToken(UserLogin userAccount)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, userAccount.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Task.FromResult(tokenString.Success<string?>());
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(storedHash);
    }

}