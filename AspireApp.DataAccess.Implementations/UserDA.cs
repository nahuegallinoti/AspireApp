using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public sealed class UserDA(AppDbContext context) : BaseDA<User, Guid>(context), IUserDA
{
    public Task<bool> ExistsAsync(string email, CancellationToken ct)
    {
        var normalized = Normalize(email);
        return Context.Users.AsNoTracking().AnyAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = Normalize(email);
        return Context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct)
    {
        var normalized = Normalize(email);
        return Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct) =>
        Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByExternalAsync(string provider, string externalUserId, CancellationToken ct) =>
        Context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.ExternalProvider == provider && u.ExternalProviderUserId == externalUserId, ct);

    public async Task<IReadOnlyList<User>> ListWithRolesAsync(int skip, int take, string? search, CancellationToken ct) =>
        await BuildSearchQuery(search)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.CreatedUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountAsync(string? search, CancellationToken ct) =>
        BuildSearchQuery(search).CountAsync(ct);

    private IQueryable<User> BuildSearchQuery(string? search)
    {
        var query = Context.Users.AsNoTracking();

        if (string.IsNullOrWhiteSpace(search))
            return query;

        var s = Normalize(search);
        // Use NormalizedEmail + ToUpper() over Name/Surname so EF Core can translate the
        // predicate against any relational provider; StringComparison overloads are not
        // translatable.
#pragma warning disable CA1862
        return query.Where(u =>
            u.NormalizedEmail.Contains(s) ||
            (u.Name + " " + u.Surname).ToUpper().Contains(s));
#pragma warning restore CA1862
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
