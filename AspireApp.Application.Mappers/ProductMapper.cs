using ProductModel = AspireApp.Application.Models.App.Product;
using ProductEntity = AspireApp.Domain.Entities.Product;

namespace AspireApp.Application.Mappers;

public sealed class ProductMapper : BaseMapper<ProductModel, ProductEntity>
{
    public override ProductModel ToModel(ProductEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description
    };

    public override ProductEntity ToEntity(ProductModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description
    };
}
