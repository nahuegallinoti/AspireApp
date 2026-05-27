using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface IUserDA : IBaseDA<User, Guid>
{
    Task<bool> ExistsAsync(string email, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct);
    Task<User?> GetByExternalAsync(string provider, string externalUserId, CancellationToken ct);
    Task<IReadOnlyList<User>> ListWithRolesAsync(int skip, int take, string? search, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
}
