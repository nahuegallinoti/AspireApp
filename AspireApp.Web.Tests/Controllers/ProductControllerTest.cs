using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.Product;
using AspireApp.Entities;

namespace AspireApp.Api.Tests.Controllers;

[TestClass]
public class ProductControllerTest : BaseControllerTest<Product, long, ProductController, IProductService>
{
    protected override ProductController CreateController()
    {
        return new ProductController(_serviceMock.Object);
    }

}