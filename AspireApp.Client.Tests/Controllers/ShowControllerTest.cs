using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Show;
using Moq;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ShowControllerTest : BaseControllerTest<Dto.Show, long, ShowController, IShowService>
{
    private Mock<IMessageBus> _messageBus = null!;

    [TestInitialize]
    public void Init()
    {
        _serviceMock = new Mock<IShowService>();
        _messageBus = new Mock<IMessageBus>();
        _controller = CreateController();
    }

    protected override ShowController CreateController()
    {
        return new ShowController(_serviceMock.Object, _messageBus.Object);
    }

}