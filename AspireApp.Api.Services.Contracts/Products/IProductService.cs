using AspireApp.DataAccess.Contracts;
using AspireApp.Entities;

namespace AspireApp.Application.Contracts.Products;

public interface IProductService : IBaseService<Product, IProductDA>
{
}