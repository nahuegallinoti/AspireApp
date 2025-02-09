using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Implementations.Auth;
using AspireApp.DataAccess.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Ent = AspireApp.Entities;

namespace AspireApp.Api.Tests.Application.Auth;

[TestClass]
public class LoginServiceDependenciesTest
{
    private Mock<IUsuarioDA> _usuarioDAMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private ILoginServiceDependencies _loginServiceDependencies = null!;
    private Mock<ILogger<ILoginServiceDependencies>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _usuarioDAMock = new Mock<IUsuarioDA>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ILoginServiceDependencies>>();

        _loginServiceDependencies = new LoginServiceDependencies(_configurationMock.Object, _usuarioDAMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task VerifyUserPassword_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        UserLogin userLogin = new()
        {
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        byte[] passwordSalt;
        byte[] passwordHash;

        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userLogin.Password));
        }

        Ent.User user = new()
        {
            Email = "nagu@gmail.com",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _usuarioDAMock.Setup(x => x.GetUserByEmail(userLogin.Email, CancellationToken.None)).ReturnsAsync(user);

        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("supersecretkey");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("audience");

        // Act
        var result = await _loginServiceDependencies.VerifyUserPassword(userLogin, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(userLogin, result.Value);
    }

    [TestMethod]
    public async Task VerifyUserPassword_ShouldReturnFailure_WhenUserIsInvalid()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "invaliduser@gmail.com",
            Password = "WrongPassword"
        };

        _usuarioDAMock.Setup(x => x.GetUserByEmail(userLogin.Email, CancellationToken.None)).ReturnsAsync((Ent.User?)null);

        // Act
        var result = await _loginServiceDependencies.VerifyUserPassword(userLogin, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void CreateToken_ShouldReturnToken_WhenUserIsValid()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("supersecretkey");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("audience");

        // Act
        var result = _loginServiceDependencies.CreateToken(userLogin);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void CreateToken_ShouldReturnFailure_WhenConfigurationIsMissing()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        _configurationMock.Setup(x => x["Jwt:Key"]).Returns((string?)null);

        // Act
        var result = _loginServiceDependencies.CreateToken(userLogin);

        // Assert
        Assert.IsFalse(result.Success);
    }
}
