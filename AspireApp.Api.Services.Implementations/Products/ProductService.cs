using AspireApp.Application.Contracts.Products;
using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;

namespace AspireApp.Application.Implementations.Products;

public class ProductService(IProductDA productDA) : BaseService<Product, IProductDA>(productDA), IProductService
{
    private readonly IProductDA _productDA = productDA;

}