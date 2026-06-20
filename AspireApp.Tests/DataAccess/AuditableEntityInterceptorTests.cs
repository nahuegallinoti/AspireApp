using AspireApp.DataAccess.Implementations;
using AspireApp.DataAccess.Implementations.Interceptors;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Tests.DataAccess;

public class AuditableEntityInterceptorTests
{
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static AppDbContext CreateContext(DateTimeOffset now) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"audit-{Guid.NewGuid():N}")
            .AddInterceptors(new AuditableEntityInterceptor(new FixedTimeProvider(now)))
            .Options);

    [Fact]
    public async Task SaveChangesStampsAllAddedAuditableEntitiesWithTheSameTime()
    {
        var now = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        await using var context = CreateContext(now);
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" };
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "user@example.com", NormalizedEmail = "USER@EXAMPLE.COM",
            Name = "Test", Surname = "User"
        };

        context.AddRange(role, user, new Product { Name = "Product" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        role.CreatedUtc.Should().Be(now);
        user.CreatedUtc.Should().Be(now);
    }

    [Fact]
    public async Task SaveChangesStampsUpdatedTimeWithoutChangingCreatedTime()
    {
        var created = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        await using var context = CreateContext(now);
        var role = new Role
        {
            Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN", CreatedUtc = created
        };
        context.Attach(role);
        role.Description = "Updated";
        context.Entry(role).State = EntityState.Modified;

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        role.CreatedUtc.Should().Be(created);
        role.UpdatedUtc.Should().Be(now);
    }
}
