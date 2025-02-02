using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Product;
using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;
using Moq;

namespace AspireApp.Api.Tests.Application;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Product, long, IProductDA>
{
    private Mock<IProductDA> _productDAMock = null!;
    private IProductService _productService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _productDAMock = new(MockBehavior.Strict);
        _productService = new ProductService(_productDAMock.Object);
    }
}
