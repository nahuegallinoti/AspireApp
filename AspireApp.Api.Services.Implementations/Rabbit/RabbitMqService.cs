using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Contracts.Rabbit;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

namespace AspireApp.Application.Implementations.Rabbit;

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly IConfiguration _configuration;
    private const string ExchangeName = "AspireWach";

    // Registro de topologías ya configuradas por routingKey
    private readonly ConcurrentDictionary<string, bool> _topologyConfigured = new();

    public RabbitMqService(ILogger<RabbitMqService> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        var rabbitMqSettings = _configuration.GetSection("RabbitMqSettings");

        _factory = new ConnectionFactory
        {
            UserName = rabbitMqSettings["UserName"] ?? "guest",
            Password = rabbitMqSettings["Password"] ?? "guest",
            VirtualHost = rabbitMqSettings["VirtualHost"] ?? "/",
            HostName = rabbitMqSettings["HostName"] ?? "localhost",
            Port = int.TryParse(rabbitMqSettings["Port"], out var port) ? port : 5672,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _logger = logger;
    }

    public Task<Result<string>> SendMessage(RabbitMessage message, string routingKey)
    {
        return ValidateMessage(message)
               .Bind(validMessage => PublishMessage(validMessage, routingKey));
    }

    private static Result<RabbitMessage> ValidateMessage(RabbitMessage message)
        => message.Validate();

    // Asegura que para la routingKey se declare el exchange, cola y binding, solo la primera vez.
    private async Task EnsureTopologyAsync(string routingKey)
    {
        // Si ya se configuró, salimos.
        if (_topologyConfigured.ContainsKey(routingKey))
            return;

        // Usamos una conexión y canal temporales para configurar la topología
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // Declarar el exchange (Direct, duradero, sin auto-delete)
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);

        // Declarar la cola con nombre igual a routingKey (duradera, no exclusiva, sin auto-delete)
        await channel.QueueDeclareAsync(
            queue: routingKey,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Realizar el binding entre la cola y el exchange, usando una clave de enrutamiento (por ejemplo, agregando un sufijo)
        await channel.QueueBindAsync(
            queue: routingKey,
            exchange: ExchangeName,
            routingKey: $"{routingKey}_rk",
            arguments: null);

        // Marcar la routingKey como configurada
        _topologyConfigured.TryAdd(routingKey, true);
        _logger.LogInformation($"Topología configurada para la routingKey: {routingKey}");
    }

    private async Task<Result<string>> PublishMessage(RabbitMessage message, string routingKey)
    {
        try
        {
            // Asegurar la topología (solo se configura la primera vez para cada routingKey)
            await EnsureTopologyAsync(routingKey);

            // Crear conexión y canal para la publicación
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            var body = Encoding.UTF8.GetBytes(message.Message);

            // Crear directamente un objeto BasicProperties
            var properties = new BasicProperties
            {
                ContentType = "text/plain",
                DeliveryMode = DeliveryModes.Persistent,
                UserId = "ElNagu"
            };

            // Publicar el mensaje usando la clave de enrutamiento configurada
            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: $"{routingKey}_rk",
                mandatory: true,
                basicProperties: properties,
                body: body);

            _logger.LogInformation($"Mensaje enviado a '{ExchangeName}' con routing key '{routingKey}_rk': {message.Message}");
            return message.Message.Success();
        }
        catch (Exception ex)
        {
            var errors = GetErrorMessages(ex);
            _logger.LogError(ex, $"Errores formateados: {errors}");
            return Result.Failure<string>("Ocurrió un error interno. Intente más tarde :)");
        }
    }

    private static ImmutableArray<string> GetErrorMessages(Exception ex)
    {
        var messages = new List<string> { $"Error: {ex.Message}" };
        Exception? inner = ex.InnerException;
        while (inner is not null)
        {
            messages.Add($"Inner Exception: {inner.Message}");
            inner = inner.InnerException;
        }
        return ImmutableArray.Create(messages.ToArray());
    }
}
