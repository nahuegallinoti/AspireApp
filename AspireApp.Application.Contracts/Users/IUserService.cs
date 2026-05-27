using AspireApp.Application.Models.Users;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Users;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken ct);

    Task<UserListPage> ListAsync(int page, int pageSize, string? search, CancellationToken ct);

    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct);

    Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct);

    Task<Result<UserDto>> AssignRolesAsync(Guid id, AssignRolesRequest request, CancellationToken ct);

    Task<Result<Unit>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
}

public sealed record UserListPage(IReadOnlyList<UserDto> Items, int Total, int Page, int PageSize);
