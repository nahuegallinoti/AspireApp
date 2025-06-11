using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Product;
using Moq;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ProductControllerTest : BaseControllerTest<Dto.Product, long, ProductController, IProductService>
{
    private Mock<IMessageBus> _messageBus = null!;

    [TestInitialize]
    public void Init()
    {
        _serviceMock = new Mock<IProductService>();
        _messageBus = new Mock<IMessageBus>();
        _controller = CreateController();
    }

    protected override ProductController CreateController()
    {
        return new ProductController(_serviceMock.Object, _messageBus.Object);
    }

}