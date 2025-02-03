using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.DataAccess.Contracts;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Implementations.Product;

public class ProductService(IProductDA productDA) : BaseServiceLong<Ent.Product>(productDA), IProductService
{
    private readonly IProductDA _productDA = productDA;

}