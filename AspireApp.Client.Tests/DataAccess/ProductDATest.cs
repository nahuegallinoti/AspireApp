using AspireApp.DataAccess.Implementations;
using AspireApp.Entities;

namespace AspireApp.Tests.Client.DataAccess;

[TestClass]
public sealed class ProductDATest : BaseDATest<Product, long, ProductDA>
{
    private ProductDA DA => _dataAccess;

    //[TestMethod]
    //public async Task GetAllProducts_ShouldReturnProducts()
    //{
    //    // Arrange
    //    List<Product> expectedProducts = [
    //        new() { Id = 1, Name = "Product 1", Description = "Desc 1" },
    //        new() { Id = 2, Name = "Product 2", Description = "Desc 2" }
    //    ];

    //    expectedProducts.ForEach(async p => await DA.AddAsync(p));

    //    await DA.SaveChangesAsync();

    //    // Act
    //    var result = await DA.GetAllAsync();

    //    // Assert
    //    Assert.IsNotNull(result);
    //    Assert.AreEqual(expectedProducts.Count, result.Count());
    //    Assert.AreEqual("Product 1", result.First().Name);
    //}
}