using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Contracts.EventBus;

public enum MessageBusProvider
{
    RabbitMq,
    Kafka
}

public enum RabbitExchangeKind
{
    Direct,
    Topic,
    Fanout,
    Headers
}

public sealed class MessageBusOptions
{
    public const string SectionName = "EventBus";

    /// <summary>Broker selection: RabbitMq or Kafka.</summary>
    [Required]
    public MessageBusProvider Provider { get; set; } = MessageBusProvider.RabbitMq;

    /// <summary>Default exchange (RabbitMQ) used when publishing.</summary>
    public string DefaultExchange { get; set; } = "aspireapp.events";

    /// <summary>Connection-name used by Aspire integration (Rabbit/Kafka).</summary>
    public string ConnectionName { get; set; } = "messaging";

    /// <summary>Rabbit-specific topology / reliability tuning.</summary>
    public RabbitMqOptions RabbitMq { get; set; } = new();
}

/// <summary>
/// Configuration for the RabbitMQ broker: exchange topology, durability,
/// publisher confirms, mandatory publishing and dead-letter wiring.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>Exchange type used when declaring <see cref="MessageBusOptions.DefaultExchange"/>.</summary>
    public RabbitExchangeKind ExchangeType { get; set; } = RabbitExchangeKind.Direct;

    /// <summary>Durable exchange survives broker restarts. Default true.</summary>
    public bool ExchangeDurable { get; set; } = true;

    /// <summary>Auto-deleted exchanges disappear when no queues are bound. Default false.</summary>
    public bool ExchangeAutoDelete { get; set; }

    /// <summary>Durable queues survive broker restarts. Default true.</summary>
    public bool QueueDurable { get; set; } = true;

    /// <summary>Exclusive queues can only be consumed by the declaring connection. Default false.</summary>
    public bool QueueExclusive { get; set; }

    /// <summary>Auto-deleted queues disappear when the last consumer disconnects. Default false.</summary>
    public bool QueueAutoDelete { get; set; }

    /// <summary>
    /// Persist messages as <c>delivery_mode = 2</c> so the broker writes them to disk.
    /// Combined with durable queues guarantees survival across broker crashes.
    /// </summary>
    public bool PersistMessages { get; set; } = true;

    /// <summary>
    /// Wait for the broker to ack each publish (publisher confirms). When true the channel
    /// is opened with confirmation tracking enabled and <c>BasicPublishAsync</c> awaits
    /// a broker ack before completing.
    /// </summary>
    public bool PublisherConfirms { get; set; } = true;

    /// <summary>
    /// When true, the broker returns un-routable messages via <c>basic.return</c>
    /// (logged as warnings) instead of silently dropping them.
    /// </summary>
    public bool Mandatory { get; set; } = true;

    /// <summary>
    /// Auto-declare a dead-letter exchange and per-queue dead-letter queue for every
    /// destination so rejected / expired / overflowed messages are not lost.
    /// </summary>
    public bool DeadLetter { get; set; } = true;

    /// <summary>Suffix appended to <see cref="MessageBusOptions.DefaultExchange"/> for the DLX. Default ".dlx".</summary>
    public string DeadLetterExchangeSuffix { get; set; } = ".dlx";

    /// <summary>Suffix appended to each queue name for the matching DLQ. Default ".dlq".</summary>
    public string DeadLetterQueueSuffix { get; set; } = ".dlq";

    /// <summary>Optional per-message TTL (milliseconds) applied to declared queues. <c>null</c> = no TTL.</summary>
    public int? MessageTtlMs { get; set; }

    /// <summary>Optional max number of messages a queue can hold before older ones are dead-lettered. <c>null</c> = unbounded.</summary>
    public int? MaxQueueLength { get; set; }
}
