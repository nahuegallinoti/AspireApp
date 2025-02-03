using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Dto = AspireApp.Api.Domain.Models;
using Ent = AspireApp.Entities;

namespace AspireApp.Api.Tests.Application.Product;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Ent.Product, Dto.Product, long, IProductDA>
{
    [TestInitialize]
    public void SetupProductService()
    {
        InitializeMapper(new ProductMapper());
        base.Setup();
    }
}