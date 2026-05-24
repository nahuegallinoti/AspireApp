using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface IUserDA : IBaseDA<User, Guid>
{
    Task<bool> ExistsAsync(string email, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
}
