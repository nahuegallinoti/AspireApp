using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Auth;
using Microsoft.Extensions.Options;

namespace AspireApp.Tests.Application.Auth;

public class Pbkdf2PasswordHasherTests
{
    private static Pbkdf2PasswordHasher CreateSut(int iterations = 10_000) =>
        new(Options.Create(new IdentityOptions { PasswordIterations = iterations }));

    [Fact]
    public void HashProducesUniqueSaltsForSamePassword()
    {
        var sut = CreateSut();

        var a = sut.Hash("Sup3rSecret!");
        var b = sut.Hash("Sup3rSecret!");

        a.Salt.Should().NotEqual(b.Salt);
        a.Hash.Should().NotEqual(b.Hash);
        a.Iterations.Should().Be(10_000);
    }

    [Fact]
    public void VerifyReturnsTrueForCorrectPassword()
    {
        var sut = CreateSut();

        var (hash, salt, iterations) = sut.Hash("Sup3rSecret!");

        sut.Verify("Sup3rSecret!", hash, salt, iterations).Should().BeTrue();
    }

    [Fact]
    public void VerifyReturnsFalseForWrongPassword()
    {
        var sut = CreateSut();

        var (hash, salt, iterations) = sut.Hash("Sup3rSecret!");

        sut.Verify("Wrong!", hash, salt, iterations).Should().BeFalse();
    }

    [Fact]
    public void VerifyReturnsFalseForEmptyInputs()
    {
        var sut = CreateSut();
        sut.Verify("", [1, 2, 3], [4, 5, 6], 1000).Should().BeFalse();
    }
}
