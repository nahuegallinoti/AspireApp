using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface IRoleDA : IBaseDA<Role, Guid>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct);
    Task<IReadOnlyList<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct);
    Task<bool> ExistsAsync(string name, CancellationToken ct);
}
