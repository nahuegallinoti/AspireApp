using AspireApp.Api.Models.Rabbit;
using AspireApp.Api.Services;
using AspireApp.Application.Implementations.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/{controller}")]
public class RabbitMqController(RabbitMqService rabbitMqService) : ControllerBase
{
    private readonly RabbitMqService _rabbitMqService = rabbitMqService;

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] RabbitMessage message)
    {
        var result = await _rabbitMqService.SendMessage(message);

        return result.Success
                    ? Ok(result.Value)
                    : Problem(
                detail: string.Join("; ", result.Errors),
                statusCode: (int)result.HttpStatusCode
            );
    }
}
