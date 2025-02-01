using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Implementations
{
    public class ProductDA(AppDbContext context) : BaseDA<Product>(context)
    {
    }
}
