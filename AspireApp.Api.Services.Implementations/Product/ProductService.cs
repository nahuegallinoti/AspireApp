using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Dto = AspireApp.Api.Domain.Models;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Implementations.Product;

public class ProductService(IProductDA productDA, ProductMapper mapper, IMemoryCache cache) : BaseService<Ent.Product, Dto.Product, long>(productDA, mapper, cache), IProductService
{
    private readonly IProductDA _productDA = productDA;

}