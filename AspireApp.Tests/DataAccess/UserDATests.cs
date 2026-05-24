using AspireApp.DataAccess.Implementations;
using AspireApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Tests.DataAccess;

public class UserDATests
{
    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"users-{Guid.NewGuid():N}")
            .Options);

    [Fact]
    public async Task ExistsAsync_returns_true_when_email_is_registered()
    {
        await using var ctx = CreateContext();
        ctx.Users.Add(new User { Id = Guid.NewGuid(), Email = "found@example.com", Name = "N", Surname = "G" });
        await ctx.SaveChangesAsync();

        var sut = new UserDA(ctx);

        (await sut.ExistsAsync("found@example.com", CancellationToken.None)).Should().BeTrue();
        (await sut.ExistsAsync("nope@example.com", CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_returns_the_persisted_user()
    {
        await using var ctx = CreateContext();
        var user = new User { Id = Guid.NewGuid(), Email = "found@example.com", Name = "N", Surname = "G" };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = new UserDA(ctx);

        var loaded = await sut.GetByEmailAsync("found@example.com", CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(user.Id);
    }
}
