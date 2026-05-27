using System.Security.Cryptography;
using System.Text;
using AspireApp.Application.Contracts.Auth;
using Microsoft.Extensions.Options;

namespace AspireApp.Application.Implementations.Auth;

internal sealed class Pbkdf2PasswordHasher(IOptions<IdentityOptions> options) : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private static readonly HashAlgorithmName HashAlgo = HashAlgorithmName.SHA256;

    public (byte[] Hash, byte[] Salt, int Iterations) Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var iterations = options.Value.PasswordIterations;
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgo, HashSize);
        return (hash, salt, iterations);
    }

    public bool Verify(string password, byte[] expectedHash, byte[] salt, int iterations)
    {
        if (string.IsNullOrEmpty(password) || expectedHash is null || salt is null || iterations <= 0)
            return false;

        var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgo, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expectedHash);
    }
}
