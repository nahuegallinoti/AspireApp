using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AspireApp.Application.Implementations.EventBus;

/// <summary>
/// RabbitMQ implementation of <see cref="IMessageBus"/>.
/// Connection is provided by the Aspire RabbitMQ client integration (singleton <see cref="IConnection"/>).
/// </summary>
internal sealed class RabbitMqMessageBus(
    IConnection connection,
    IOptions<MessageBusOptions> options,
    ILogger<RabbitMqMessageBus> logger) : IMessageBus, IAsyncDisposable
{
    private readonly MessageBusOptions _options = options.Value;
    private readonly ConcurrentDictionary<string, bool> _declaredBindings = new();
    private IChannel? _publishChannel;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public async Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic, CancellationToken ct = default)
        where TMessage : class
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await EnsureTopologyAsync(topic, ct);

        var channel = await GetChannelAsync(ct);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await channel.BasicPublishAsync(
            exchange: _options.DefaultExchange,
            routingKey: topic,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);

        logger.LogInformation(
            "Published message to RabbitMQ exchange '{Exchange}' with routing key '{RoutingKey}'",
            _options.DefaultExchange, topic);

        return Result.Success(json);
    }

    private async Task EnsureTopologyAsync(string topic, CancellationToken ct)
    {
        if (_declaredBindings.ContainsKey(topic))
            return;

        var channel = await GetChannelAsync(ct);
        await channel.ExchangeDeclareAsync(
            exchange: _options.DefaultExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: topic,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: topic,
            exchange: _options.DefaultExchange,
            routingKey: topic,
            cancellationToken: ct);

        _declaredBindings.TryAdd(topic, true);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_publishChannel is { IsOpen: true })
            return _publishChannel;

        await _channelLock.WaitAsync(ct);
        try
        {
            if (_publishChannel is { IsOpen: true })
                return _publishChannel;

            _publishChannel = await connection.CreateChannelAsync(cancellationToken: ct);
            return _publishChannel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel is not null)
            await _publishChannel.DisposeAsync();
        _channelLock.Dispose();
    }
}
