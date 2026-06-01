using AspireApp.Application.Contracts.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspireApp.Infrastructure.Messaging;

public static class MessageBusServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IMessageBus"/> based on the <c>EventBus:Provider</c> setting.
    /// Expects the matching Aspire client integration (RabbitMQ / Kafka) to be wired
    /// upstream by the host (e.g. <c>builder.AddRabbitMQClient("messaging")</c>).
    /// </summary>
    public static IHostApplicationBuilder AddMessageBus(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<MessageBusOptions>()
            .Bind(builder.Configuration.GetSection(MessageBusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var provider = builder.Configuration
            .GetSection(MessageBusOptions.SectionName)
            .Get<MessageBusOptions>()?.Provider ?? MessageBusProvider.RabbitMq;

        var connectionName = builder.Configuration
            .GetSection(MessageBusOptions.SectionName)["ConnectionName"] ?? "messaging";

        switch (provider)
        {
            case MessageBusProvider.RabbitMq:
                builder.AddRabbitMQClient(connectionName);
                builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
                break;

            case MessageBusProvider.Kafka:
                builder.AddKafkaProducer<string, string>(connectionName);
                builder.Services.AddSingleton<IMessageBus, KafkaMessageBus>();
                break;

            default:
                throw new InvalidOperationException($"Unsupported message-bus provider '{provider}'.");
        }

        return builder;
    }
}
