using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface IRefreshTokenDA : IBaseDA<RefreshToken, Guid>
{
    Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct);
    Task<int> RevokeAllForUserAsync(Guid userId, string reason, string? ip, CancellationToken ct);
    Task<int> DeleteExpiredAsync(DateTimeOffset olderThanUtc, CancellationToken ct);
}
