using AspireApp.Application.Mappers;
using AspireApp.Application.Persistence;
using AspireApp.Core.Mappers;

namespace AspireApp.Tests.Client.Application.Product;

[TestClass]
public sealed class ProductServiceTest : BaseServiceTest<Domain.Entities.Product, AspireApp.Application.Models.App.Product, long, IProductDA>
{
    [TestInitialize]
    public void SetupProductService()
    {
        // Se hace esto porque Setup no puede recibir parámetros
        InitializeMapper(new ProductMapper());
        Setup();
    }
}