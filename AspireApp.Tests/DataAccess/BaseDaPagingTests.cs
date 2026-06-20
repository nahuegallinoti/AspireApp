using AspireApp.DataAccess.Implementations;
using AspireApp.Domain.Entities;
using AspireApp.Domain.Paging;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Tests.DataAccess;

public class BaseDaPagingTests
{
    private sealed class Query : PagedQuery;

    private static AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"paging-{Guid.NewGuid():N}")
            .Options);

    [Fact]
    public async Task GetPagedReturnsStablePageAndGlobalTotal()
    {
        await using var context = CreateContext();
        context.Products.AddRange(Enumerable.Range(1, 7)
            .Reverse()
            .Select(id => new Product { Id = id, Name = $"Product {id}" }));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var sut = new ProductDA(context);

        var result = await sut.GetPagedAsync(
            new Query { Page = 2, PageSize = 3 }, TestContext.Current.CancellationToken);

        result.Items.Select(product => product.Id).Should().Equal(4, 5, 6);
        result.Total.Should().Be(7);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedNormalizesBoundsAndReturnsEmptyOutOfRangePage()
    {
        await using var context = CreateContext();
        context.Products.Add(new Product { Id = 1, Name = "Product" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var sut = new ProductDA(context);

        var clamped = await sut.GetPagedAsync(
            new Query { PageSize = 1_000 }, TestContext.Current.CancellationToken);
        var outOfRange = await sut.GetPagedAsync(
            new Query { Page = 10, PageSize = 5 }, TestContext.Current.CancellationToken);

        clamped.PageSize.Should().Be(500);
        outOfRange.Items.Should().BeEmpty();
        outOfRange.Total.Should().Be(1);
    }
}
