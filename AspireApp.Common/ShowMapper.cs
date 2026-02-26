using AspireApp.Core.Mappers;
using Show = AspireApp.Application.Models.App.Show;

namespace AspireApp.Application.Mappers;

public sealed class ShowMapper : BaseMapper<Show, Domain.Entities.Show>
{
    public ShowMapper() { }

    public override Show ToModel(Domain.Entities.Show entity)
    {
        if (entity is null) return new();

        return new Show
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public override Domain.Entities.Show ToEntity(Show model)
    {
        if (model is null) return new();

        return new Domain.Entities.Show
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
    }
}