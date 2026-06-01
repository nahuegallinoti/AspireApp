using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AspireApp.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of <see cref="IMessageBus"/>.
/// <para>
/// Topology is declared lazily on first publish to each topic:
/// </para>
/// <list type="bullet">
///   <item>Durable exchange (type/durability configurable).</item>
///   <item>Durable, persistent queue bound to the routing key.</item>
///   <item>Optional dead-letter exchange + per-queue DLQ so rejected, expired or overflowed
///         messages are preserved instead of lost.</item>
///   <item>Optional message TTL and queue length limits.</item>
/// </list>
/// <para>
/// The publish channel runs with publisher confirms tracking enabled, mandatory publishing on,
/// and a <c>basic.return</c> handler that logs un-routable messages.
/// </para>
/// </summary>
internal sealed class RabbitMqMessageBus(
    IConnection connection,
    IOptions<MessageBusOptions> options,
    ILogger<RabbitMqMessageBus> logger) : IMessageBus, IAsyncDisposable
{
    private readonly MessageBusOptions _options = options.Value;
    private readonly RabbitMqOptions _rabbit = options.Value.RabbitMq;
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
            ContentEncoding = "utf-8",
            DeliveryMode = _rabbit.PersistMessages ? DeliveryModes.Persistent : DeliveryModes.Transient,
            MessageId = Guid.NewGuid().ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = typeof(TMessage).FullName ?? typeof(TMessage).Name,
            AppId = "AspireApp"
        };

        try
        {
            await channel.BasicPublishAsync(
                exchange: _options.DefaultExchange,
                routingKey: topic,
                mandatory: _rabbit.Mandatory,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish message to RabbitMQ exchange '{Exchange}' (routingKey='{RoutingKey}', confirms={Confirms}, mandatory={Mandatory}).",
                _options.DefaultExchange, topic, _rabbit.PublisherConfirms, _rabbit.Mandatory);
            return Result.Failure<string>($"RabbitMQ publish failed: {ex.Message}", System.Net.HttpStatusCode.BadGateway);
        }

        logger.LogInformation(
            "Published message to RabbitMQ exchange '{Exchange}' (routingKey='{RoutingKey}', messageId={MessageId}, persistent={Persistent}).",
            _options.DefaultExchange, topic, properties.MessageId, _rabbit.PersistMessages);

        return Result.Success(json);
    }

    private async Task EnsureTopologyAsync(string topic, CancellationToken ct)
    {
        if (_declaredBindings.ContainsKey(topic))
            return;

        var channel = await GetChannelAsync(ct);
        var exchangeType = MapExchangeType(_rabbit.ExchangeType);

        await channel.ExchangeDeclareAsync(
            exchange: _options.DefaultExchange,
            type: exchangeType,
            durable: _rabbit.ExchangeDurable,
            autoDelete: _rabbit.ExchangeAutoDelete,
            cancellationToken: ct);

        var queueArgs = new Dictionary<string, object?>();
        string? dlxName = null;

        if (_rabbit.DeadLetter)
        {
            dlxName = _options.DefaultExchange + _rabbit.DeadLetterExchangeSuffix;
            var dlqName = topic + _rabbit.DeadLetterQueueSuffix;

            await channel.ExchangeDeclareAsync(
                exchange: dlxName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: ct);

            await channel.QueueDeclareAsync(
                queue: dlqName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct);

            await channel.QueueBindAsync(
                queue: dlqName,
                exchange: dlxName,
                routingKey: topic,
                cancellationToken: ct);

            queueArgs["x-dead-letter-exchange"] = dlxName;
            queueArgs["x-dead-letter-routing-key"] = topic;
        }

        if (_rabbit.MessageTtlMs is int ttl && ttl > 0)
            queueArgs["x-message-ttl"] = ttl;

        if (_rabbit.MaxQueueLength is int maxLen && maxLen > 0)
        {
            queueArgs["x-max-length"] = maxLen;
            queueArgs["x-overflow"] = "reject-publish-dlx";
        }

        await channel.QueueDeclareAsync(
            queue: topic,
            durable: _rabbit.QueueDurable,
            exclusive: _rabbit.QueueExclusive,
            autoDelete: _rabbit.QueueAutoDelete,
            arguments: queueArgs.Count == 0 ? null : queueArgs,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: topic,
            exchange: _options.DefaultExchange,
            routingKey: topic,
            cancellationToken: ct);

        logger.LogInformation(
            "RabbitMQ topology ready: exchange='{Exchange}' ({Type}, durable={ExchangeDurable}), queue='{Queue}' (durable={QueueDurable}), dlx='{Dlx}'.",
            _options.DefaultExchange, exchangeType, _rabbit.ExchangeDurable, topic, _rabbit.QueueDurable, dlxName ?? "<none>");

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

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: _rabbit.PublisherConfirms,
                publisherConfirmationTrackingEnabled: _rabbit.PublisherConfirms);

            _publishChannel = await connection.CreateChannelAsync(channelOptions, ct);
            _publishChannel.BasicReturnAsync += OnBasicReturnAsync;
            return _publishChannel;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    private Task OnBasicReturnAsync(object sender, BasicReturnEventArgs args)
    {
        logger.LogWarning(
            "RabbitMQ returned an un-routable message: exchange='{Exchange}', routingKey='{RoutingKey}', replyCode={ReplyCode}, replyText='{ReplyText}'.",
            args.Exchange, args.RoutingKey, args.ReplyCode, args.ReplyText);
        return Task.CompletedTask;
    }

    private static string MapExchangeType(RabbitExchangeKind kind) => kind switch
    {
        RabbitExchangeKind.Direct => ExchangeType.Direct,
        RabbitExchangeKind.Topic => ExchangeType.Topic,
        RabbitExchangeKind.Fanout => ExchangeType.Fanout,
        RabbitExchangeKind.Headers => ExchangeType.Headers,
        _ => ExchangeType.Direct
    };

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel is not null)
        {
            _publishChannel.BasicReturnAsync -= OnBasicReturnAsync;
            await _publishChannel.DisposeAsync();
        }
        _channelLock.Dispose();
    }
}
