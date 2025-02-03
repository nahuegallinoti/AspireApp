using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.Auth;
using AspireApp.Application.Implementations.User;
using AspireApp.Core.ROP;
using Moq;

namespace AspireApp.Api.Tests.Application.Auth;

[TestClass]
public sealed class LoginServiceTest
{
    private Mock<ILoginServiceDependencies> _loginServiceDependencies = null!;
    private ILoginService _loginService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _loginServiceDependencies = new(MockBehavior.Strict);
        _loginService = new LoginService(_loginServiceDependencies.Object);
    }

    [TestMethod]
    public async Task Login()
    {
        // Arrange
        var user = new UserLogin
        {
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Simulamos que el usuario existe y la contraseña es válida.
        _loginServiceDependencies.Setup(x => x.VerifyUserPassword(user)).ReturnsAsync(user.Success());

        // Simulamos que la creación del token es exitosa.
        AuthenticationResult authResult = new("fake-jwt-token");

        _loginServiceDependencies.Setup(x => x.CreateToken(user)).Returns(authResult);

        // Act
        var result = await _loginService.Login(user, cancellationToken);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(authResult.Token, result.Value.Token);
    }

    [TestMethod]
    public async Task Login_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "nonexistentuser@gmail.com",
            Password = "WrongPassword"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Simulamos que el usuario no existe.
        _loginServiceDependencies.Setup(x => x.VerifyUserPassword(userLogin)).ReturnsAsync(Result.Failure<UserLogin>("Usuario no encontrado"));

        // Act
        var result = await _loginService.Login(userLogin, cancellationToken);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task Login_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "",  // Email vacío para fallar la validación
            Password = "Nagu"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        var result = await _loginService.Login(userLogin, cancellationToken);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("El email no debe estar vacio", result.Errors.FirstOrDefault());
    }
}