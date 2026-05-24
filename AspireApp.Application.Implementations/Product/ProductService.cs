using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using ProductEntity = AspireApp.Domain.Entities.Product;
using ProductModel = AspireApp.Application.Models.App.Product;

namespace AspireApp.Application.Implementations.Product;

internal sealed class ProductService(IProductDA productDA, ProductMapper mapper, HybridCache hybridCache)
    : BaseService<ProductEntity, ProductModel, long>(productDA, mapper, hybridCache), IProductService;
