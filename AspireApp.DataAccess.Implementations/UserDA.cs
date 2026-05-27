using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class UserDA(AppDbContext context) : BaseDA<User, Guid>(context), IUserDA
{
    public Task<bool> ExistsAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _context.Users.AsNoTracking().AnyAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToUpperInvariant();
        return _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken ct) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByExternalAsync(string provider, string externalUserId, CancellationToken ct) =>
        _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.ExternalProvider == provider && u.ExternalProviderUserId == externalUserId, ct);

    public async Task<IReadOnlyList<User>> ListWithRolesAsync(int skip, int take, string? search, CancellationToken ct)
    {
        var query = _context.Users.AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToUpperInvariant();
            query = query.Where(u =>
                u.NormalizedEmail.Contains(s) ||
                (u.Name + " " + u.Surname).Contains(s, StringComparison.CurrentCultureIgnoreCase));
        }

        return await query
            .OrderBy(u => u.CreatedUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search, CancellationToken ct)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToUpperInvariant();
            query = query.Where(u =>
                u.NormalizedEmail.Contains(s) ||
                (u.Name + " " + u.Surname).Contains(s, StringComparison.CurrentCultureIgnoreCase));
        }

        return query.CountAsync(ct);
    }
}
