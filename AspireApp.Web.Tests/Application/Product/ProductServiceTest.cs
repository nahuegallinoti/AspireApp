using AspireApp.Application.Contracts.Product;
using AspireApp.DataAccess.Contracts;
using Moq;
using Ent = AspireApp.Entities;

namespace AspireApp.Api.Tests.Application.Product;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Ent.Product, long, IProductDA>
{
    private Mock<IProductDA> _productDAMock = null!;
    private IProductService _productService = null!;

    //[TestInitialize]
    //public void Initialize()
    //{
    //    _productDAMock = new(MockBehavior.Strict);
    //    _productService = new ProductService(_productDAMock.Object);
    //}
}
