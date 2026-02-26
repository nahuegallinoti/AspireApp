using AspireApp.Application.Persistence;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.DataAccess.Implementations;

public class ProductDA(AppDbContext context) : BaseDA<Product, long>(context), IProductDA
{

}
