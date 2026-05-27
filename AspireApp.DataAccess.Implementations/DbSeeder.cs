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
    ILogger<DbSeeder> logger)
{
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
                CreatedUtc = DateTimeOffset.UtcNow
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

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = seed.Email,
            NormalizedEmail = normalizedEmail,
            Name = seed.Name,
            Surname = seed.Surname,
            EmailConfirmed = true,
            IsActive = true,
            CreatedUtc = DateTimeOffset.UtcNow
        };

        if (!string.IsNullOrEmpty(seed.Password))
        {
            var (hash, salt, iterations) = passwordHasher.Hash(seed.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordIterations = iterations;
        }

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName.Equals(RoleNames.Admin, StringComparison.InvariantCultureIgnoreCase), ct);
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName.Equals(RoleNames.User, StringComparison.InvariantCultureIgnoreCase), ct);

        if (adminRole is not null)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
        if (userRole is not null)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });

        context.Users.Add(user);
        logger.LogInformation("Seeded admin user {Email}", seed.Email);
    }
}
