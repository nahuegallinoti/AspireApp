using AspireApp.Application.Mappers;

namespace AspireApp.Tests.Application.Mappers;

public class BaseMapperTests
{
    private sealed class StubMapper : BaseMapper<string, int>
    {
        public override string ToModel(int entity) => entity.ToString();
        public override int ToEntity(string model) => int.Parse(model);
    }

    private readonly StubMapper _mapper = new();

    [Fact]
    public void DefaultUpdateEntityThrowsDescriptiveException()
    {
        var action = () => _mapper.UpdateEntity(1, "2");

        action.Should().Throw<NotSupportedException>().WithMessage("*StubMapper*");
    }

    [Fact]
    public void NullListsReturnEmptySequences()
    {
        _mapper.ToModelList(null!).Should().BeEmpty();
        _mapper.ToEntityList(null!).Should().BeEmpty();
    }

    [Fact]
    public void ListsAreProjectedInOrder()
    {
        _mapper.ToModelList([2, 1, 3]).Should().ContainInOrder("2", "1", "3");
        _mapper.ToEntityList(["2", "1", "3"]).Should().ContainInOrder(2, 1, 3);
    }
}
