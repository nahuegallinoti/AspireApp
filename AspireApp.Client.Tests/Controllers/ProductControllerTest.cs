using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Models.App;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ProductControllerTest : BaseControllerTest<Product, long, ProductController, IProductService>
{
    private Mock<IMessageBus> _messageBus = null!;
    private Mock<ILogger<ProductController>> _logger = null!;

    [TestInitialize]
    public void Init()
    {
        _serviceMock = new Mock<IProductService>();
        _messageBus = new Mock<IMessageBus>();
        _logger = new Mock<ILogger<ProductController>>();
        _controller = CreateController();
    }

    protected override ProductController CreateController()
    {
        return new ProductController(_serviceMock.Object, _messageBus.Object, _logger.Object);
    }

}