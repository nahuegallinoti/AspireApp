using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Dto = AspireApp.Api.Domain.Models;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Implementations.Product;

public class ProductService(IProductDA productDA, ProductMapper mapper) : BaseServiceLong<Ent.Product, Dto.Product>(productDA, mapper), IProductService
{
    private readonly IProductDA _productDA = productDA;

}