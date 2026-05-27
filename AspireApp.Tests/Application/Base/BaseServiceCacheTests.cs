using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using ProductEntity = AspireApp.Domain.Entities.Product;
using ProductModel = AspireApp.Application.Models.App.Product;

namespace AspireApp.Tests.Application.Base;

/// <summary>
/// Exercises the cache-aware behaviour shared by every concrete service through
/// <see cref="BaseService{TEntity,TModel,TID}"/>. The mapper and entity are
/// borrowed from <c>Product</c> because it is the simplest one; the assertions
/// are about cache plumbing, not domain logic.
/// </summary>
public class BaseServiceCacheTests
{
    private sealed class Sut(BaseService<ProductEntity, ProductModel, long> service, IProductDA da, HybridCache cache)
    {
        public BaseService<ProductEntity, ProductModel, long> Service { get; } = service;
        public IProductDA Da { get; } = da;
        public HybridCache Cache { get; } = cache;
    }

    private static Sut BuildSut()
    {
        var da = Substitute.For<IProductDA>();

        var services = new ServiceCollection();
        services.AddHybridCache(o =>
        {
            o.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });
        var hybridCache = services.BuildServiceProvider().GetRequiredService<HybridCache>();

        var service = new BaseService<ProductEntity, ProductModel, long>(da, new ProductMapper(), hybridCache);
        return new Sut(service, da, hybridCache);
    }

    [Fact]
    public async Task GetByIdAsyncHitsTheDataAccessOnFirstCallAndIsServedFromCacheAfterwards()
    {
        var sut = BuildSut();
        const long id = 42;
        sut.Da.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new ProductEntity { Id = id, Name = "Cached", Description = "From DA" });

        var first = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);
        var second = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        first.Should().NotBeNull();
        first!.Id.Should().Be(id);
        second.Should().NotBeNull();
        second!.Name.Should().Be("Cached");

        await sut.Da.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullAndStillCachesTheMissSoTheDataAccessIsHitOnce()
    {
        var sut = BuildSut();
        const long id = 7;
        sut.Da.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ProductEntity?)null);

        var first = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);
        var second = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        first.Should().BeNull();
        second.Should().BeNull();
        await sut.Da.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsyncDelegatesToDataAccessAndAssignsEntityId()
    {
        var sut = BuildSut();
        var input = new ProductModel { Name = "Pencil", Description = "HB" };

        sut.Da.AddAsync(Arg.Any<ProductEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<ProductEntity>();
                entity.Id = 99;
                return Task.FromResult(Result.Success(entity));
            });

        var result = await sut.Service.AddAsync(input, TestContext.Current.CancellationToken);

        result.Success.Should().BeTrue();
        result.Value.Id.Should().Be(99);
        result.Value.Name.Should().Be("Pencil");
        await sut.Da.Received(1).AddAsync(Arg.Any<ProductEntity>(), Arg.Any<CancellationToken>());
        await sut.Da.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsyncInvalidatesCacheSoFollowingGetByIdAsyncHitsTheDataAccessAgain()
    {
        var sut = BuildSut();
        const long id = 11;
        sut.Da.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(
                _ => new ProductEntity { Id = id, Name = "Before Add", Description = string.Empty },
                _ => new ProductEntity { Id = id, Name = "After Add", Description = string.Empty });

        sut.Da.AddAsync(Arg.Any<ProductEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<ProductEntity>();
                entity.Id = 999;
                return Task.FromResult(Result.Success(entity));
            });

        var before = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);
        before!.Name.Should().Be("Before Add");

        var second = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);
        second!.Name.Should().Be("Before Add", "the second call must come from the cache, not the DA");
        await sut.Da.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());

        await sut.Service.AddAsync(new ProductModel { Name = "Trigger invalidation" }, TestContext.Current.CancellationToken);

        var after = await sut.Service.GetByIdAsync(id, TestContext.Current.CancellationToken);
        after!.Name.Should().Be("After Add", "AddAsync removes the cache entries tagged with the model name");
        await sut.Da.Received(2).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsyncBypassesTheCacheWhenInsertingAndDoesNotConsultTheDataAccessGetById()
    {
        var sut = BuildSut();
        sut.Da.AddAsync(Arg.Any<ProductEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<ProductEntity>();
                entity.Id = 1;
                return Task.FromResult(Result.Success(entity));
            });

        var result = await sut.Service.AddAsync(new ProductModel { Name = "Brand new" }, TestContext.Current.CancellationToken);

        result.Success.Should().BeTrue();
        await sut.Da.DidNotReceive().GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}
