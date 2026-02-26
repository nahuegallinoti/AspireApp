using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Show;
using AspireApp.Application.Models.App;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ShowControllerTest : BaseControllerTest<Show, long, ShowController, IShowService>
{
    private Mock<IMessageBus> _messageBus = null!;
    private Mock<ILogger<ShowController>> _logger = null!;

    [TestInitialize]
    public void Init()
    {
        _serviceMock = new Mock<IShowService>();
        _messageBus = new Mock<IMessageBus>();
        _logger = new Mock<ILogger<ShowController>>();
        _controller = CreateController();
    }

    protected override ShowController CreateController()
    {
        return new ShowController(_serviceMock.Object, _messageBus.Object, _logger.Object);
    }

}