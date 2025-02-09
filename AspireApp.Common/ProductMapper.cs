using AspireApp.Api.Models.App;
using Ent = AspireApp.Entities;

namespace AspireApp.Core.Mappers;

public sealed class ProductMapper : BaseMapper<Product, Ent.Product>
{
    public ProductMapper() { }

    public override Product ToModel(Ent.Product entity)
    {
        if (entity is null) return new();

        return new Product
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public override Ent.Product ToEntity(Product model)
    {
        if (model is null) return new();

        return new Ent.Product
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
    }
}