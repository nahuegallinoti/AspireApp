using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Models;
using AspireApp.Application.Models.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController<TModel, TID, TService>(
    TService service,
    IMessageBus messageBus,
    ILogger logger) : ControllerBase
    where TModel : BaseModel<TID>
    where TID : struct
    where TService : IBaseService<TModel, TID>
{
    protected TService Service { get; } = service;
    protected ILogger Logger { get; } = logger;
    private readonly IMessageBus _messageBus = messageBus;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Service.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(TID id, CancellationToken ct)
    {
        var model = await Service.GetByIdAsync(id, ct);
        return model is not null ? Ok(model) : NotFound();
    }

    [HttpPost]
    public virtual async Task<IActionResult> Add([FromBody] TModel model, CancellationToken ct)
    {
        var result = await Service.AddAsync(model, ct);

        if (!result.Success)
            return Problem(detail: string.Join("; ", result.Errors), statusCode: (int)result.HttpStatusCode);

        await _messageBus.PublishAsync(
            new EventMessage { Message = $"{typeof(TModel).Name} {result.Value.Id} created" },
            topic: typeof(TModel).Name.ToLowerInvariant(),
            ct);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public IActionResult Update(TID id, [FromBody] TModel model)
    {
        if (!id.Equals(model.Id))
            return BadRequest("Route id does not match payload id.");

        Service.Update(model);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TID id, CancellationToken ct)
    {
        var model = await Service.GetByIdAsync(id, ct);
        if (model is null) return NotFound();

        Service.Delete(model);
        await Service.SaveChangesAsync(ct);
        return NoContent();
    }
}
