using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Domain.ROP;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public sealed class AuthControllerTest
{
    private Mock<ILoginService> _mockLoginService = null!;
    private Mock<IRegisterUserService> _mockRegisterUserService = null!;
    private AuthController _controller = null!;

    [TestInitialize]
    public void Initialize()
    {
        _mockLoginService = new Mock<ILoginService>(MockBehavior.Strict);
        _mockRegisterUserService = new Mock<IRegisterUserService>(MockBehavior.Strict);
        _controller = new AuthController(_mockRegisterUserService.Object, _mockLoginService.Object);
    }

    [TestMethod]
    public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
    {
        // Arrange
        var userLogin = new UserLogin { Email = "test@example.com", Password = "password" };
        var authResult = new AuthenticationResult { Token = "fake-jwt-token" };
        _mockLoginService
            .Setup(s => s.Login(userLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResult.Success());

        // Act
        var result = await _controller.Login(userLogin);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(authResult, okResult.Value);
    }

    [TestMethod]
    public async Task Login_ShouldReturnProblem_WhenLoginFails()
    {
        // Arrange
        var userLogin = new UserLogin { Email = "test@example.com", Password = "wrongpassword" };

        _mockLoginService
            .Setup(s => s.Login(userLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthenticationResult>(["Invalid credentials"], HttpStatusCode.Unauthorized));

        // Act
        var result = await _controller.Login(userLogin);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(401, objectResult.StatusCode);
    }

    [TestMethod]
    public async Task Register_ShouldReturnCreated_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var userRegister = new UserRegister { Email = "newuser@example.com", Password = "password" };
        var userId = Guid.NewGuid();

        _mockRegisterUserService
            .Setup(s => s.AddUser(userRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId.Success());

        // Act
        var result = await _controller.Register(userRegister);

        // Assert
        var createdResult = result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(userId, createdResult.Value);
    }

    [TestMethod]
    public async Task Register_ShouldReturnProblem_WhenRegistrationFails()
    {
        // Arrange
        var userRegister = new UserRegister { Email = "existinguser@example.com", Password = "password" };

        _mockRegisterUserService
            .Setup(s => s.AddUser(userRegister, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(["Email already in use"], HttpStatusCode.Conflict));

        // Act
        var result = await _controller.Register(userRegister);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(409, objectResult.StatusCode);
    }
}