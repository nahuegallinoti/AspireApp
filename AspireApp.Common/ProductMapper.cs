using AspireApp.Application.Models.App;
using AspireApp.Core.Mappers;
using AspireApp.Domain.Entities;
using Dto = AspireApp.Application.Models;

namespace AspireApp.Application.Mappers;

public sealed class ProductMapper : BaseMapper<Dto.App.Product, Domain.Entities.Product>
{
    public ProductMapper() { }

    public override Dto.App.Product ToModel(Domain.Entities.Product entity)
    {
        if (entity is null) return new();

        return new Dto.App.Product
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public override Domain.Entities.Product ToEntity(Dto.App.Product model)
    {
        if (model is null) return new();

        return new Domain.Entities.Product
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
    }
}