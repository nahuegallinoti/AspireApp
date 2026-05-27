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
    public async Task ExistsAsyncReturnsTrueWhenEmailIsRegistered()
    {
        await using var ctx = CreateContext();
        ctx.Users.Add(new User { Id = Guid.NewGuid(), Email = "found@example.com", NormalizedEmail = "FOUND@EXAMPLE.COM", Name = "N", Surname = "G" });
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new UserDA(ctx);

        (await sut.ExistsAsync("found@example.com", CancellationToken.None)).Should().BeTrue();
        (await sut.ExistsAsync("nope@example.com", CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsyncReturnsThePersistedUser()
    {
        await using var ctx = CreateContext();
        var user = new User { Id = Guid.NewGuid(), Email = "found@example.com", NormalizedEmail = "FOUND@EXAMPLE.COM", Name = "N", Surname = "G" };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new UserDA(ctx);

        var loaded = await sut.GetByEmailAsync("found@example.com", CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(user.Id);
    }
}
