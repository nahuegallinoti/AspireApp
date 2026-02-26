using AspireApp.Application.Models.Auth.User;
using AspireApp.Client.ApiClients;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AspireApp.Tests.Client.ApiClients;

[TestClass]
public class RegisterApiClientTest : BaseApiClientTest<RegisterApiClient>
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private RegisterApiClient _apiClient = null!;

    protected override RegisterApiClient CreateClient(IHttpClientFactory factory) => new(factory);


    [TestInitialize]
    public void Initialize()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://fakeapi.com/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _apiClient = new RegisterApiClient(httpClientFactoryMock.Object);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        var userRegister = new UserRegister { Email = "newuser@example.com", Password = "password" };
        var userId = Guid.NewGuid();
        var jsonResponse = JsonSerializer.Serialize(userId);

        CancellationTokenSource tokenSource = new();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _apiClient.RegisterAsync(userRegister, tokenSource.Token);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(userId, result.Value);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldReturnFailure_WhenResponseIsConflict()
    {
        // Arrange
        var userRegister = new UserRegister { Email = "existinguser@example.com", Password = "password" };

        CancellationTokenSource tokenSource = new();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict
            });

        // Act
        var result = await _apiClient.RegisterAsync(userRegister, tokenSource.Token);

        // Assert
        Assert.IsFalse(result.Success);
    }
}