using AspireApp.DataAccess.Contracts;
using Ent = AspireApp.Entities;

namespace AspireApp.Api.Tests.Application.Product;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Ent.Product, long, IProductDA>
{
}
