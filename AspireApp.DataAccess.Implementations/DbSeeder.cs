using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Models.Auth;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspireApp.DataAccess.Implementations;

public sealed class DbSeeder(
    AppDbContext context,
    IPasswordHasher passwordHasher,
    IOptions<IdentityOptions> identityOptions,
    TimeProvider timeProvider,
    ILogger<DbSeeder> logger)
{
    private static readonly string AdminNormalized = RoleNames.Admin.ToUpperInvariant();
    private static readonly string UserNormalized = RoleNames.User.ToUpperInvariant();

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Database.EnsureCreatedAsync(ct);

        await SeedRolesAsync(ct);
        await SeedAdminAsync(ct);

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        var existing = await context.Roles
            .Select(r => r.NormalizedName)
            .ToListAsync(ct);

        var now = timeProvider.GetUtcNow();

        foreach (var name in RoleNames.All)
        {
            var normalized = name.ToUpperInvariant();
            if (existing.Contains(normalized))
                continue;

            context.Roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedName = normalized,
                IsSystem = true,
                Description = $"System role: {name}",
                CreatedUtc = now
            });

            logger.LogInformation("Seeded role {Role}", name);
        }
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        var seed = identityOptions.Value.SeedAdmin;
        if (!seed.Enabled)
            return;

        var normalizedEmail = seed.Email.Trim().ToUpperInvariant();
        var exists = await context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, ct);
        if (exists)
            return;

        var now = timeProvider.GetUtcNow();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = seed.Email,
            NormalizedEmail = normalizedEmail,
            Name = seed.Name,
            Surname = seed.Surname,
            EmailConfirmed = true,
            IsActive = true,
            CreatedUtc = now
        };

        if (!string.IsNullOrEmpty(seed.Password))
        {
            var (hash, salt, iterations) = passwordHasher.Hash(seed.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordIterations = iterations;
        }

        var roles = await context.Roles
            .Where(r => r.NormalizedName == AdminNormalized || r.NormalizedName == UserNormalized)
            .ToListAsync(ct);

        foreach (var role in roles)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedUtc = now });

        context.Users.Add(user);
        logger.LogInformation("Seeded admin user {Email}", seed.Email);
    }
}
