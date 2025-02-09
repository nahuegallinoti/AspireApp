using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;

namespace AspireApp.Api.Services;

public class RabbitMqService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqService> _logger;

    // TODO: Ver donde se pone
    public RabbitMqService(ILogger<RabbitMqService> logger)
    {
        _factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            HostName = "localhost",
            Port = 5672
        };

        _logger = logger;
    }

    public Task<Result<string>> SendMessage(RabbitMessage message)
    {
        return ValidateMessage(message)
               .Bind(PublishMessage);
    }

    private static Result<RabbitMessage> ValidateMessage(RabbitMessage message) => message.Validate();

    public async Task<Result<string>> PublishMessage(RabbitMessage message)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "test_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message.Message);

            await channel.BasicPublishAsync(exchange: "", routingKey: "test_queue", body);

            return message.Message.Success();
        }

        catch (Exception ex)
        {
            var errors = GetErrorMessages(ex);

            _logger.LogError(ex, $"Errors formatted: {errors}");

            return Result.Failure<string>("An internal server error has occurred. Try later :)");
        }
    }

    private static ImmutableArray<string> GetErrorMessages(Exception ex)
    {
        List<string> messages = [$"Error: {ex.Message}"];

        Exception? inner = ex.InnerException;

        while (inner is not null)
        {
            messages.Add($"Inner Exception: {inner.Message}");
            inner = inner.InnerException;
        }

        return ImmutableArray.Create(messages.ToArray());
    }
}
