using AspireApp.Application.Models.Roles;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct);

    Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken ct);

    Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken ct);

    Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct);
}
