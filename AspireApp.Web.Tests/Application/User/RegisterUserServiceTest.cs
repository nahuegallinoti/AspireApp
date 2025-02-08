using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.User;
using AspireApp.Core.ROP;
using Moq;

namespace AspireApp.Api.Tests.Application.User;

[TestClass]
public sealed class RegisterUserServiceTest
{
    private Mock<IRegisterUserServiceDependencies> _registerUserServiceDependencies = null!;
    private IRegisterUserService _registerUserService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _registerUserServiceDependencies = new(MockBehavior.Strict);
        _registerUserService = new RegisterUserService(_registerUserServiceDependencies.Object);
    }

    [TestMethod]
    public async Task AddUser()
    {
        // Arrange
        var user = new UserRegister
        {
            Name = "Naguel",
            Surname = "Gugu",
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        _registerUserServiceDependencies.Setup(x => x.VerifyUserDoesNotExist(user, cancellationToken)).ReturnsAsync(user.Success());
        _registerUserServiceDependencies.Setup(x => x.AddUser(user, cancellationToken)).ReturnsAsync(Guid.NewGuid().Success());

        // Act
        var result = await _registerUserService.AddUser(user, cancellationToken);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }
}