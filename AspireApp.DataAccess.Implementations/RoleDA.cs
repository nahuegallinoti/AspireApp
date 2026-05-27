using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public sealed class RoleDA(AppDbContext context) : BaseDA<Role, Guid>(context), IRoleDA
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken ct)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return Context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, ct);
    }

    public async Task<IReadOnlyList<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct)
    {
        var normalized = names.Select(n => n.Trim().ToUpperInvariant()).Distinct().ToArray();
        return await Context.Roles
            .Where(r => normalized.Contains(r.NormalizedName))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string name, CancellationToken ct)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return Context.Roles.AsNoTracking().AnyAsync(r => r.NormalizedName == normalized, ct);
    }
}
