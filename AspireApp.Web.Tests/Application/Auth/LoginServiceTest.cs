using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Api.Tests.Helpers;
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
        _loginServiceDependencies.Setup(x => x.VerifyUserPassword(user, cancellationToken)).ReturnsAsync(user.Success());

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
        _loginServiceDependencies.Setup(x => x.VerifyUserPassword(userLogin, cancellationToken)).ReturnsAsync(Result.Failure<UserLogin>("Usuario no encontrado"));

        // Act
        var result = await _loginService.Login(userLogin, cancellationToken);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task Login_ShouldReturnFailure_WhenEmailIsEmpty()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "",
            Password = "Nagu"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Obtener el mensaje de error para la propiedad "Email" usando reflexión
        var expectedErrorMessage = ValidationHelper.GetErrorMessage(userLogin, nameof(userLogin.Email));

        // Act: Ejecutamos el Login (sin cambios, solo para testear el comportamiento)
        var result = await _loginService.Login(userLogin, cancellationToken);

        // Assert: Verificamos el resultado de validación
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expectedErrorMessage, result.Errors.FirstOrDefault());
    }

    [TestMethod]
    public async Task Login_ShouldReturnFailure_WhenPasswordIsEmpty()
    {
        // Arrange
        var userLogin = new UserLogin
        {
            Email = "nagu@gmail.com",
            Password = ""
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Obtener el mensaje de error para la propiedad "Password" usando reflexión
        var expectedErrorMessage = ValidationHelper.GetErrorMessage(userLogin, nameof(userLogin.Password));

        // Act: Ejecutamos el Login (sin cambios, solo para testear el comportamiento)
        var result = await _loginService.Login(userLogin, cancellationToken);

        // Assert: Verificamos el resultado de validación
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expectedErrorMessage, result.Errors.FirstOrDefault());
    }
}