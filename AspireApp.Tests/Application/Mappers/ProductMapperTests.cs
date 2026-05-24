using AspireApp.Application.Mappers;
using ProductEntity = AspireApp.Domain.Entities.Product;
using ProductModel = AspireApp.Application.Models.App.Product;

namespace AspireApp.Tests.Application.Mappers;

public class ProductMapperTests
{
    private readonly ProductMapper _mapper = new();

    [Fact]
    public void Round_trip_preserves_data()
    {
        var model = new ProductModel { Id = 7, Name = "Pencil", Description = "HB" };

        var entity = _mapper.ToEntity(model);
        var back = _mapper.ToModel(entity);

        back.Should().BeEquivalentTo(model);
    }

    [Fact]
    public void ToModelList_maps_every_entity()
    {
        var entities = new[]
        {
            new ProductEntity { Id = 1, Name = "A" },
            new ProductEntity { Id = 2, Name = "B" }
        };

        var models = _mapper.ToModelList(entities).ToList();

        models.Should().HaveCount(2);
        models.Select(m => m.Id).Should().BeEquivalentTo([1L, 2L]);
    }
}
