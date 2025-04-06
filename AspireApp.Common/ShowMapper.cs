using AspireApp.Api.Models.App;
using Ent = AspireApp.Entities;

namespace AspireApp.Core.Mappers;

public sealed class ShowMapper : BaseMapper<Show, Ent.Show>
{
    public ShowMapper() { }

    public override Show ToModel(Ent.Show entity)
    {
        if (entity is null) return new();

        return new Show
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public override Ent.Show ToEntity(Show model)
    {
        if (model is null) return new();

        return new Ent.Show
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
    }
}