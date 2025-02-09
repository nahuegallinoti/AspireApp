using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts;
using Dto = AspireApp.Api.Models.App;
using Ent = AspireApp.Entities;

namespace AspireApp.Tests.Client.Application.Product;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Ent.Product, Dto.Product, long, IProductDA>
{
    [TestInitialize]
    public void SetupProductService()
    {
        // Se hace esto porque Setup no puede recibir parámetros
        InitializeMapper(new ProductMapper());
        Setup();
    }
}