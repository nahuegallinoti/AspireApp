using AspireApp.Api.Infrastructure;
using AspireApp.Application.Contracts.Roles;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Produces("application/json")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await roleService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        (await roleService.GetByIdAsync(id, ct)).ToActionResult(this);

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken ct) =>
        (await roleService.CreateAsync(request, ct)).ToActionResult(this);

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct) =>
        (await roleService.UpdateAsync(id, request, ct)).ToActionResult(this);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await roleService.DeleteAsync(id, ct)).ToActionResult(this, _ => NoContent());
}
