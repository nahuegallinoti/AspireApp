using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class UserDA(AppDbContext context) : BaseDA<User, Guid>(context), IUserDA
{
    public Task<bool> ExistsAsync(string email, CancellationToken ct) =>
        _context.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
}
