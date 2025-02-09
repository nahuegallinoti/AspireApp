using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Microsoft.Extensions.Caching.Hybrid;
using Dto = AspireApp.Api.Models.App;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Implementations.Product;

public class ProductService(IProductDA productDA, ProductMapper mapper, HybridCache hybridCache) : BaseService<Ent.Product, Dto.Product, long>(productDA, mapper, hybridCache), IProductService
{
    private readonly IProductDA _productDA = productDA;

}