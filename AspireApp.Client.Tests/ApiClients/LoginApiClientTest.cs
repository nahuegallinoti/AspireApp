using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Client.ApiClients;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AspireApp.Tests.Client.ApiClients;

[TestClass]
public class LoginApiClientTest : BaseApiClientTest<LoginApiClient>
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private LoginApiClient _apiClient = null!;

    protected override LoginApiClient CreateClient(IHttpClientFactory factory) => new(factory);


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

        _apiClient = new LoginApiClient(httpClientFactoryMock.Object);
    }

    [TestMethod]
    public async Task LoginAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        var loginRequest = new UserLogin { Email = "user@example.com", Password = "password" };
        var authResult = new AuthenticationResult { Token = "fake-token" };
        var jsonResponse = JsonSerializer.Serialize(authResult);

        CancellationTokenSource tokenSource = new();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _apiClient.LoginAsync(loginRequest, tokenSource.Token);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(authResult.Token, result.Value.Token);
    }

    [TestMethod]
    public async Task LoginAsync_ShouldReturnFailure_WhenResponseIsUnauthorized()
    {
        // Arrange
        var loginRequest = new UserLogin { Email = "user@example.com", Password = "wrongpassword" };

        CancellationTokenSource tokenSource = new();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act
        var result = await _apiClient.LoginAsync(loginRequest, tokenSource.Token);

        // Assert
        Assert.IsFalse(result.Success);
    }

}