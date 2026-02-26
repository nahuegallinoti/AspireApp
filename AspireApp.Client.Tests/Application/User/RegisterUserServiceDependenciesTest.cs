using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.User;
using AspireApp.Core.Mappers;
using Moq;
using AspireApp.Tests.Client.Extensions;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Persistence;

namespace AspireApp.Tests.Client.Application.User;

[TestClass]
public class RegisterUserServiceDependenciesTest
{
    private Mock<IUsuarioDA> _usuarioDA = null!;
    private UsuarioMapper _usuarioMapper = null!;
    private IRegisterUserServiceDependencies _registerUserServiceDependencies = null!;

    [TestInitialize]
    public void Initialize()
    {
        _usuarioDA = new(MockBehavior.Default);
        _usuarioMapper = new();
        _registerUserServiceDependencies = new RegisterUserServiceDependencies(_usuarioDA.Object, _usuarioMapper);
    }

    [TestMethod]
    public async Task AddUser_Should_Add_And_Save_User()
    {
        // Arrange
        var user = new UserRegister
        {
            Name = "Naguel",
            Surname = "Gugu",
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        var id = user.SetId<UserRegister, Guid>();

        //_usuarioDA.Setup(x => x.AddAsync(It.IsAny<Ent.User>(), CancellationToken.None)).Returns(Task.CompletedTask);

        _usuarioDA.Setup(x => x.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        var result = await _registerUserServiceDependencies.AddUser(user, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success, "El usuario debería haberse registrado correctamente.");
        _usuarioDA.Verify(x => x.AddAsync(It.Is<Domain.Entities.User>(u => u.Email == user.Email && u.PasswordHash != null), CancellationToken.None), Times.Once);
        _usuarioDA.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task VerifyUserDoesNotExist_Should_Return_Failure_When_User_Exists()
    {
        // Arrange
        var user = new UserRegister
        {
            Name = "Naguel",
            Surname = "Gugu",
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        CancellationTokenSource cts = new();

        _usuarioDA.Setup(x => x.UserExist(user.Email, cts.Token)).ReturnsAsync(true);

        // Act
        var result = await _registerUserServiceDependencies.VerifyUserDoesNotExist(user, cts.Token);

        // Assert
        Assert.IsFalse(result.Success, "El usuario debería existir.");
        _usuarioDA.Verify(result => result.UserExist(user.Email, cts.Token), Times.Once);
    }


    [TestMethod]
    public async Task VerifyUserDoesNotExist_Should_Return_True_When_User_Does_Not_Exists()
    {
        // Arrange
        var user = new UserRegister
        {
            Name = "Naguel",
            Surname = "Gugu",
            Email = "nagu@gmail.com",
            Password = "Nagu"
        };

        CancellationTokenSource cts = new();

        _usuarioDA.Setup(x => x.UserExist(user.Email, cts.Token)).ReturnsAsync(false);

        // Act
        var result = await _registerUserServiceDependencies.VerifyUserDoesNotExist(user, cts.Token);

        // Assert
        Assert.IsTrue(result.Success, "El usuario ya existe.");
        _usuarioDA.Verify(result => result.UserExist(user.Email, cts.Token), Times.Once);
    }
}