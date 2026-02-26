using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using Microsoft.Extensions.Caching.Hybrid;

namespace AspireApp.Application.Implementations.Product;

public class ProductService(IProductDA productDA, ProductMapper mapper, HybridCache hybridCache) : BaseService<Domain.Entities.Product, Models.App.Product, long>(productDA, mapper, hybridCache), IProductService
{
    //private readonly IProductDA _productDA = productDA;

}