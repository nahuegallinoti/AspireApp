namespace AspireApp.Api.Tests.ApiClients;

using AspireApp.Web.ApiClients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

[TestClass]
public abstract class BaseApiClientTest<T> where T : BaseApiClient
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;
    private HttpClient _httpClient = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private T _apiClient = null!;
    protected abstract T CreateClient(IHttpClientFactory factory);

    [TestInitialize]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://fakeapi.com/")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient("ApiClient")).Returns(_httpClient);

        _apiClient = CreateClient(_httpClientFactoryMock.Object);
    }

    [TestMethod]
    public async Task PostAsync_ShouldCall_HttpClient_WithPostMethod()
    {
        var testData = new { Name = "Test" };
        SetupHttpResponse(HttpMethod.Post, "api/test");

        await _apiClient.PostAsync<object, object>("api/test", testData);

        VerifyHttpCall(HttpMethod.Post, "api/test");
    }

    [TestMethod]
    public async Task GetAsync_ShouldCall_HttpClient_WithGetMethod()
    {
        SetupHttpResponse(HttpMethod.Get, "api/test");

        await _apiClient.GetAsync<object>("api/test");

        VerifyHttpCall(HttpMethod.Get, "api/test");
    }

    [TestMethod]
    public async Task PutAsync_ShouldCall_HttpClient_WithPutMethod()
    {
        var testData = new { Name = "Updated" };
        SetupHttpResponse(HttpMethod.Put, "api/test");

        await _apiClient.PutAsync("api/test", testData);

        VerifyHttpCall(HttpMethod.Put, "api/test");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldCall_HttpClient_WithDeleteMethod()
    {
        SetupHttpResponse(HttpMethod.Delete, "api/test");

        await _apiClient.DeleteAsync<object>("api/test");

        VerifyHttpCall(HttpMethod.Delete, "api/test");
    }

    protected void SetupHttpResponse(HttpMethod method, string url)
    {
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { success = true })
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri!.ToString() == $"https://fakeapi.com/{url}"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage)
            .Verifiable();
    }

    protected void VerifyHttpCall(HttpMethod method, string url)
    {
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == method && req.RequestUri!.ToString() == $"https://fakeapi.com/{url}"),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}