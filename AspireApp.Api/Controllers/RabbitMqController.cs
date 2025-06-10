using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Contracts.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageBusController(IMessageBus messageBus) : ControllerBase
{
    private readonly IMessageBus _messageBus = messageBus;

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] RabbitMessage message)
    {
        var result = await _messageBus.PublishAsync(message, topic: "Nagugu");

        return result.Success
            ? Ok(result.Value)
            : Problem(
                detail: string.Join("; ", result.Errors),
                statusCode: (int)result.HttpStatusCode
            );
    }
}