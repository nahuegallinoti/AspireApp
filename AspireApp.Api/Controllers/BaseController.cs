using AspireApp.Api.Infrastructure;
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
    IMessageBus? messageBus,
    ILogger logger) : ControllerBase
    where TModel : BaseModel<TID>
    where TID : struct
    where TService : IBaseService<TModel, TID>
{
    protected TService Service { get; } = service;
    protected ILogger Logger { get; } = logger;
    private readonly IMessageBus? _messageBus = messageBus;

    /// <summary>
    /// Overload for entities that don't publish to the event bus.
    /// Forwards to the primary constructor with a <c>null</c> message bus.
    /// </summary>
    protected BaseController(TService service, ILogger logger)
        : this(service, messageBus: null, logger)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await Service.GetAllAsync(ct));

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
            return result.ToActionResult(this);

        await PublishCreatedEventAsync(result.Value, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(TID id, [FromBody] TModel model, CancellationToken ct)
    {
        if (!id.Equals(model.Id))
            return BadRequest("Route id does not match payload id.");

        Service.Update(model);
        await Service.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TID id, CancellationToken ct)
    {
        var model = await Service.GetByIdAsync(id, ct);
        if (model is null)
            return NotFound();

        Service.Delete(model);
        await Service.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task PublishCreatedEventAsync(TModel model, CancellationToken ct)
    {
        if (_messageBus is null)
            return;

        var topic = typeof(TModel).Name.ToLowerInvariant();
        var publishResult = await _messageBus.PublishAsync(
            new EventMessage { Message = $"{typeof(TModel).Name} {model.Id} created" },
            topic,
            ct);

        if (!publishResult.Success)
            Logger.LogWarning("Failed to publish '{Topic}' event: {Errors}",
                topic, string.Join("; ", publishResult.Errors));
    }
}
