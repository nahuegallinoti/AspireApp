using AspireApp.Application.Models.Rabbit;
using AspireApp.Client.ApiClients;
using AspireApp.Domain.ROP;

namespace AspireApp.Client.Services;

public class RabbitMqSenderService(RabbitMqApiClient apiClient)
{
    private readonly RabbitMqApiClient _apiClient = apiClient;

    public async Task<Result<string>> SendMessageAsync(RabbitMessage message)
    {
        return await _apiClient.SendMessageAsync(message, CancellationToken.None);
    }
}