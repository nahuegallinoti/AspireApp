using AspireApp.Web.ApiClients;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AspireApp.Api.Tests.ApiClients;

[TestClass]
public abstract class BaseApiClientTest<TClient, TResponse, TRequest>
    where TClient : BaseApiClient
    where TResponse : class
    where TRequest : class
{
    protected Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    protected HttpClient _httpClient = null!;
    protected TClient _apiClient = null!;

    [TestInitialize]
    public void Initialize()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        //_apiClient = CreateApiClient(httpClientFactoryMock.Object);

        _apiClient = Activator.CreateInstance<TClient>()!;
    }

    protected abstract TClient CreateApiClient(IHttpClientFactory httpClientFactory);

    [TestMethod]
    public async Task GetAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        var expectedResponse = CreateResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _apiClient.GetAsync<TResponse>("https://example.com/api/resource");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedResponse, result.Value);
    }

    [TestMethod]
    public async Task PostAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        var request = CreateRequest();
        var expectedResponse = CreateResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _apiClient.PostAsync<TResponse, TRequest>("https://example.com/api/resource", request);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedResponse, result.Value);
    }

    [TestMethod]
    public async Task PutAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        var request = CreateRequest();
        var expectedResponse = CreateResponse();
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _apiClient.PutAsync<TResponse>("https://example.com/api/resource", expectedResponse);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedResponse, result.Value);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenResponseIsOk()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var result = await _apiClient.DeleteAsync<TResponse>("https://example.com/api/resource");

        // Assert
        Assert.IsTrue(result.Success);
    }

    protected abstract TResponse CreateResponse();
    protected abstract TRequest CreateRequest();
}