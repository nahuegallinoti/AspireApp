using AspireApp.Api.Domain.Auth.User;
using AspireApp.Api.Tests.Extensions;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Implementations.User;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Ent = AspireApp.Entities;
using Moq;

namespace AspireApp.Api.Tests.Application.User;

[TestClass]
public class RegisterUserServiceDependenciesTest
{
    private Mock<IUsuarioDA> _usuarioDA = null!;
    private UsuarioMapper _usuarioMapper = null!;
    private IRegisterUserServiceDependencies _registerUserServiceDependencies = null!;

    [TestInitialize]
    public void Initialize()
    {
        _usuarioDA = new(MockBehavior.Strict);
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

        _usuarioDA.Setup(x => x.AddAsync(It.IsAny<Ent.User>())).Returns(Task.CompletedTask);
        _usuarioDA.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _registerUserServiceDependencies.AddUser(user);

        // Assert
        Assert.IsTrue(result.Success, "El usuario debería haberse registrado correctamente.");
        _usuarioDA.Verify(x => x.AddAsync(It.Is<Ent.User>(u => u.Email == user.Email && u.PasswordHash != null)), Times.Once);
        _usuarioDA.Verify(x => x.SaveChangesAsync(), Times.Once);
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

        _usuarioDA.Setup(x => x.UserExist(user.Email, default)).ReturnsAsync(true);

        // Act
        var result = await _registerUserServiceDependencies.VerifyUserDoesNotExist(user);

        // Assert
        Assert.IsFalse(result.Success, "El usuario debería existir.");
        _usuarioDA.Verify(result => result.UserExist(user.Email, default), Times.Once);
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

        _usuarioDA.Setup(x => x.UserExist(user.Email, default)).ReturnsAsync(false);

        // Act
        var result = await _registerUserServiceDependencies.VerifyUserDoesNotExist(user);

        // Assert
        Assert.IsTrue(result.Success, "El usuario ya existe.");
        _usuarioDA.Verify(result => result.UserExist(user.Email, default), Times.Once);
    }
}