using AspireApp.DataAccess.Implementations;
using AspireApp.Entities;

namespace AspireApp.Api.Tests.DataAccess;

[TestClass]
public sealed class UsuarioDATest : BaseDATest<User, Guid, UsuarioDA>
{
    private UsuarioDA DA => _dataAccess;

    [TestMethod]
    public async Task UserExist_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        string email = "testuser@gmail.com";
        User user = new()
        {
            Id = Guid.NewGuid(),
            Email = email
        };

        await DA.AddAsync(user, CancellationToken.None);
        await DA.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await DA.UserExist(email, CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetUserByEmail_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        string email = "nagu@gmail.com";
        User user = new()
        {
            Id = Guid.NewGuid(),
            Email = email
        };

        await DA.AddAsync(user, CancellationToken.None);
        await DA.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await DA.GetUserByEmail(email, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Email, result.Email);
    }
}
