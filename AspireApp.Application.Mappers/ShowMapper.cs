using ShowModel = AspireApp.Application.Models.App.Show;
using ShowEntity = AspireApp.Domain.Entities.Show;

namespace AspireApp.Application.Mappers;

public sealed class ShowMapper : BaseMapper<ShowModel, ShowEntity>
{
    public override ShowModel ToModel(ShowEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description
    };

    public override ShowEntity ToEntity(ShowModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description
    };

    public override void UpdateEntity(ShowEntity entity, ShowModel model)
    {
        entity.Name = model.Name;
        entity.Description = model.Description;
    }
}
