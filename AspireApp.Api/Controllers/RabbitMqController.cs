using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Contracts.Rabbit;
using AspireApp.Application.Implementations.Rabbit;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RabbitMqController(IRabbitMqService rabbitMqService) : ControllerBase
{
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] RabbitMessage message)
    {
        var result = await _rabbitMqService.SendMessage(message, "Nagugu");

        return result.Success
                    ? Ok(result.Value)
                    : Problem(
                detail: string.Join("; ", result.Errors),
                statusCode: (int)result.HttpStatusCode
            );
    }
}
