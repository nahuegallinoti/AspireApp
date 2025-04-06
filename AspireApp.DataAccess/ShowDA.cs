using AspireApp.DataAccess.Contracts;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entities;

namespace AspireApp.DataAccess.Implementations;

public class ShowDA(AppDbContext context) : BaseDA<Show, long>(context), IShowDA
{

}
