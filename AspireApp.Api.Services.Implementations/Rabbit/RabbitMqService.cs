using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Contracts.Rabbit;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Core.ROP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;

namespace AspireApp.Application.Implementations.Rabbit;

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly IConfiguration _configuration;
    private const string ExchangeName = "AspireWach";

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

    private async Task<Result<string>> PublishMessage(RabbitMessage message, string routingKey)
    {
        try
        {
            // Se crea la conexión y el canal utilizando las API asíncronas recomendadas.
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Declarar el exchange (tipo Direct, duradero y sin auto-delete)
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            // Declarar la cola (duradera, compartida y sin auto-delete)
            await channel.QueueDeclareAsync(
                queue: routingKey,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Enlazar la cola al exchange usando una clave de enrutamiento (aquí se añade un sufijo, por ejemplo "_rk")
            await channel.QueueBindAsync(
                queue: routingKey,
                exchange: ExchangeName,
                routingKey: $"{routingKey}_rk",
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message.Message);

            // Publicar el mensaje en el exchange con la clave de enrutamiento especificada
            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: $"{routingKey}_rk",
                mandatory: true,
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
        List<string> messages = new() { $"Error: {ex.Message}" };

        Exception? inner = ex.InnerException;
        while (inner is not null)
        {
            messages.Add($"Inner Exception: {inner.Message}");
            inner = inner.InnerException;
        }

        return ImmutableArray.Create(messages.ToArray());
    }
}
