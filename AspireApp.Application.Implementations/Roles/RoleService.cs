using AspireApp.Application.Contracts.Roles;
using AspireApp.Application.Implementations.Extensions;
using AspireApp.Application.Mappers;
using AspireApp.Application.Models.Roles;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using RoleEntity = AspireApp.Domain.Entities.Role;

namespace AspireApp.Application.Implementations.Roles;

internal sealed class RoleService(IRoleDA roleDA, RoleMapper mapper) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct)
    {
        var roles = await roleDA.GetAllAsync(ct);
        return roles.Select(mapper.ToModel).ToList();
    }

    public async Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var role = await roleDA.GetByIdAsync(id, ct);
        return role is null ? Result.NotFound<RoleDto>("Role not found.") : mapper.ToModel(role).Success();
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct)
    {
        var validation = request.Validate();
        if (validation.IsFailure)
            return Result.Failure<RoleDto>(validation.Errors, validation.HttpStatusCode);

        if (await roleDA.ExistsAsync(request.Name, ct))
            return Result.Conflict<RoleDto>("Role already exists.");

        var entity = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            NormalizedName = request.Name.Trim().ToUpperInvariant(),
            Description = request.Description,
            IsSystem = false
        };

        await roleDA.AddAsync(entity, ct);
        await roleDA.SaveChangesAsync(ct);
        return mapper.ToModel(entity).Success();
    }

    public async Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct)
    {
        var role = await roleDA.GetByIdAsync(id, ct);
        if (role is null)
            return Result.NotFound<RoleDto>("Role not found.");

        if (role.IsSystem)
            return Result.Failure<RoleDto>("System roles cannot be modified.", System.Net.HttpStatusCode.Conflict);

        role.Description = request.Description;
        roleDA.Update(role);
        await roleDA.SaveChangesAsync(ct);
        return mapper.ToModel(role).Success();
    }

    public async Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var role = await roleDA.GetByIdAsync(id, ct);
        if (role is null)
            return Result.NotFound<Unit>("Role not found.");

        if (role.IsSystem)
            return Result.Failure("System roles cannot be deleted.", System.Net.HttpStatusCode.Conflict);

        roleDA.Delete(role);
        await roleDA.SaveChangesAsync(ct);
        return Result.Success();
    }
}
