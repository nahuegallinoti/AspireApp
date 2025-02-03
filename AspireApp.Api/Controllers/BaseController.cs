using AspireApp.Application.Contracts.Base;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

public abstract class BaseController<T, TID, TService> : ControllerBase
    where T : BaseEntity<TID>
    where TID : struct
    where TService : IBaseService<T, TID, IBaseDA<T, TID>>
{
    protected readonly TService _service;

    protected BaseController(TService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(TID id)
    {
        var entity = await _service.GetByIdAsync(id);
        return entity is not null ? Ok(entity) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] T entity)
    {
        await _service.AddAsync(entity);
        await _service.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public IActionResult Update(TID id, [FromBody] T entity)
    {
        if (!id.Equals(entity.Id))
        {
            return BadRequest("ID mismatch");
        }

        _service.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(TID id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        _service.Delete(entity);
        await _service.SaveChangesAsync();
        return NoContent();
    }
}
