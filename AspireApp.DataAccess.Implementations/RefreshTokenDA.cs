using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.DataAccess.Implementations;

public class RefreshTokenDA(AppDbContext context) : BaseDA<RefreshToken, Guid>(context), IRefreshTokenDA
{
    public Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct) =>
        _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

    public async Task<int> RevokeAllForUserAsync(Guid userId, string reason, string? ip, CancellationToken ct)
    {
        var active = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedUtc == null)
            .ToListAsync(ct);

        if (active.Count == 0)
            return 0;

        var now = DateTimeOffset.UtcNow;
        foreach (var token in active)
        {
            token.RevokedUtc = now;
            token.RevokedByIp = ip;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(ct);
        return active.Count;
    }

    public async Task<int> DeleteExpiredAsync(DateTimeOffset olderThanUtc, CancellationToken ct)
    {
        var expired = await _context.RefreshTokens
            .Where(rt => rt.ExpiresUtc < olderThanUtc)
            .ToListAsync(ct);

        if (expired.Count == 0)
            return 0;

        _context.RefreshTokens.RemoveRange(expired);
        await _context.SaveChangesAsync(ct);
        return expired.Count;
    }
}
