namespace AspireApp.Application.Contracts.Auth;

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt, int Iterations) Hash(string password);

    bool Verify(string password, byte[] expectedHash, byte[] salt, int iterations);
}
