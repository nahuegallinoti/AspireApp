using AspireApp.Api.Domain.Rabbit;
using AspireApp.Core.ROP;
using AspireApp.Web.ApiClients;

namespace AspireApp.Web.Services;

public class RabbitMqSenderService(RabbitMqApiClient apiClient)
{
    private readonly RabbitMqApiClient _apiClient = apiClient;

    public async Task<Result<string>> SendMessageAsync(RabbitMessage message)
    {
        return await _apiClient.SendMessageAsync(message, CancellationToken.None);
    }
}