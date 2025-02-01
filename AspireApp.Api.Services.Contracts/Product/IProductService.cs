using AspireApp.Application.Contracts.Base;
using AspireApp.DataAccess.Contracts;
using Ent = AspireApp.Entities;

namespace AspireApp.Application.Contracts.Product;

public interface IProductService : IBaseService<Ent.Product, long, IProductDA>
{
}