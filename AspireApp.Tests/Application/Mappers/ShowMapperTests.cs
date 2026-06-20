using AspireApp.Application.Mappers;
using ShowEntity = AspireApp.Domain.Entities.Show;
using ShowModel = AspireApp.Application.Models.App.Show;

namespace AspireApp.Tests.Application.Mappers;

public class ShowMapperTests
{
    private readonly ShowMapper _mapper = new();

    [Fact]
    public void RoundTripPreservesData()
    {
        var model = new ShowModel { Id = 7, Name = "Show", Description = "Description" };

        _mapper.ToModel(_mapper.ToEntity(model)).Should().BeEquivalentTo(model);
    }

    [Fact]
    public void ToModelListMapsEveryEntity()
    {
        var models = _mapper.ToModelList([
            new ShowEntity { Id = 1, Name = "A" },
            new ShowEntity { Id = 2, Name = "B" }
        ]).ToList();

        models.Select(x => x.Id).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public void UpdateEntityMutatesExistingEntityWithoutChangingId()
    {
        var entity = new ShowEntity { Id = 1, Name = "Old", Description = "Old" };
        var reference = entity;

        _mapper.UpdateEntity(entity, new ShowModel { Id = 99, Name = "New", Description = "Updated" });

        entity.Should().BeSameAs(reference);
        entity.Id.Should().Be(1);
        entity.Name.Should().Be("New");
        entity.Description.Should().Be("Updated");
    }
}
