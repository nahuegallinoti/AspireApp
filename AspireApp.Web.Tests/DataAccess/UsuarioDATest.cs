using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;
using Moq;

namespace AspireApp.Api.Tests.DataAccess;

[TestClass]
public sealed class UsuarioDATest : BaseDATest<User, Guid>
{
    private readonly Mock<IUsuarioDA> _usuarioDAMock = new(MockBehavior.Strict);

    [TestMethod]
    public async Task UserExist_ShouldReturnTrue()
    {
        // Arrange
        string email = "nagu@gmail.com";

        _usuarioDAMock.Setup(repo => repo.UserExist(email))
                      .ReturnsAsync(true);

        // Act
        var result = await _usuarioDAMock.Object.UserExist(email);

        // Assert
        Assert.IsTrue(result);
        _usuarioDAMock.Verify(repo => repo.UserExist(email), Times.Once);
    }

    [TestMethod]
    public async Task GetUserByEmail_ShouldReturnUser()
    {
        // Arrange
        string email = "nagu@gmail.com";

        User user = new()
        {
            Id = Guid.NewGuid(),
            Email = email
        };

        _usuarioDAMock.Setup(repo => repo.GetUserByEmail(email))
                      .ReturnsAsync(user);

        // Act
        var result = await _usuarioDAMock.Object.GetUserByEmail(email);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Email, result.Email);

        _usuarioDAMock.Verify(repo => repo.GetUserByEmail(email), Times.Once);
    }
}
