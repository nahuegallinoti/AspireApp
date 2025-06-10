using AspireApp.Api.Models;
using AspireApp.Api.Models.Rabbit;
using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Contracts.EventBus;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

/// <summary>
/// Base controller providing common CRUD operations for API endpoints.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TID">The type of the model identifier.</typeparam>
/// <typeparam name="TService">The service type handling the operations.</typeparam>
public abstract class BaseController<TModel, TID, TService>(TService service, IMessageBus messageBus)
    : ControllerBase where TModel : BaseModel<TID>
                    where TID : struct
                    where TService : IBaseService<TModel, TID>
{
    protected readonly TService _service = service;
    private readonly IMessageBus _messageBus = messageBus;

    /// <summary>
    /// Retrieves all models.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves an model by its ID.
    /// </summary>
    /// <param name="id">The modelidentifier.</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(TID id)
    {
        var model = await _service.GetByIdAsync(id);
        return model is not null ? Ok(model) : NotFound();
    }

    /// <summary>
    /// Adds a new model.
    /// </summary>
    /// <param name="model">The model to add.</param>
    [HttpPost]
    public virtual async Task<IActionResult> Add([FromBody] TModel model, CancellationToken ct = default)
    {
        await _service.AddAsync(model, ct);

        await _messageBus.PublishAsync(new RabbitMessage() { Message = $"Creado el pibe {model.Id}" }, topic: $"{model.GetType().Name}");

        //Incluye en los headers el Location con la URL para obtener el recurso recién creado
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    /// <summary>
    /// Updates an existing model.
    /// </summary>
    /// <param name="id">The model identifier.</param>
    /// <param name="model">The updated modeldata.</param>
    [HttpPut("{id}")]
    public IActionResult Update(TID id, [FromBody] TModel model)
    {
        if (!id.Equals(model.Id))
        {
            return BadRequest("ID mismatch");
        }

        _service.Update(model);
        return NoContent();
    }

    /// <summary>
    /// Deletes an model by its ID.
    /// </summary>
    /// <param name="id">The model identifier.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TID id, CancellationToken ct = default)
    {
        var model = await _service.GetByIdAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        _service.Delete(model);
        await _service.SaveChangesAsync(ct);
        return NoContent();
    }
}
