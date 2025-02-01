using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;
using Moq;

namespace AspireApp.Api.Tests.DataAccess;

[TestClass]
public sealed class ProductDATest : BaseDATest<Product, long>
{
    private Mock<IProductDA> _productDAMock = null!;

    [TestInitialize]
    public void Initialize()
    {
        _productDAMock = new(MockBehavior.Strict);
    }

    [TestMethod]
    public async Task GetAllProducts_ShouldReturnProducts()
    {
        // Arrange
        List<Product> expectedProducts = [
            new() { Id = 1, Name = "Product 1", Description = "Desc 1" },
            new() { Id = 2, Name = "Product 2", Description = "Desc 2" }
        ];

        _productDAMock.Setup(repo => repo.GetAllAsync())
                      .ReturnsAsync(expectedProducts);

        // Act
        var result = await _productDAMock.Object.GetAllAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedProducts.Count, result.Count());
        Assert.AreEqual("Product 1", result.First().Name);

        _productDAMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }
}