using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Models.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageBusController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] EventMessage message, CancellationToken ct)
    {
        var result = await messageBus.PublishAsync(message, topic: "demo", ct);
        return result.Success
            ? Ok(result.Value)
            : Problem(detail: string.Join("; ", result.Errors), statusCode: (int)result.HttpStatusCode);
    }
}
