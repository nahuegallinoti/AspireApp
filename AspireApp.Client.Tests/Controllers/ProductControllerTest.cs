using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.Product;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ProductControllerTest : BaseControllerTest<Dto.Product, long, ProductController, IProductService>
{
    protected override ProductController CreateController()
    {
        return new ProductController(_serviceMock.Object);
    }

}