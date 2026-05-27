using AspireApp.Api.Infrastructure;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Models.EventBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MessageBusController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost("send")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send([FromBody] EventMessage message, CancellationToken ct) =>
        (await messageBus.PublishAsync(message, topic: "demo", ct)).ToActionResult(this);
}
