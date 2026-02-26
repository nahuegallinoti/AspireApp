using AspireApp.DataAccess.Implementations;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Domain.Entities.Base;
using AspireApp.Tests.Client.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Tests.Client.DataAccess;

public abstract class BaseDATest<TEntity, TID, TDA>
                                               where TEntity : BaseEntity<TID>
                                               where TID : struct
                                               where TDA : BaseDA<TEntity, TID>
{
    protected AppDbContext _context = null!;
    protected DbContextOptions<AppDbContext> _options = null!;
    protected TDA _dataAccess = null!;

    private static TEntity CreateInstance() => Activator.CreateInstance<TEntity>();

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Si se usa un nombre estático, usará la misma db para todos los tests
            .Options;

        _context = new AppDbContext(_options);

        _dataAccess = (TDA)Activator.CreateInstance(typeof(TDA), _context)!;
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var entity = CreateInstance();
        var propertyName = entity.GetType().GetProperty("Name");

        // Set entity properties
        propertyName?.SetValue(entity, "Test Entity");

        TID id = entity.SetId<TEntity, TID>();

        // Act
        await _dataAccess.AddAsync(entity, CancellationToken.None);
        await _dataAccess.SaveChangesAsync(CancellationToken.None);
        var result = await _dataAccess.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Entity", propertyName?.GetValue(result)?.ToString());
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        _context.Set<TEntity>().AddRange(CreateInstance(), CreateInstance());
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _dataAccess.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    public async Task Update_ShouldModifyEntity()
    {
        // TODO: si no existe la propiedad name en la entidad va a fallar

        // Arrange
        var entity = CreateInstance();
        var propertyName = entity.GetType().GetProperty("Name");

        // Set initial entity properties
        propertyName?.SetValue(entity, "Original");

        TID id = entity.SetId<TEntity, TID>();

        await _dataAccess.AddAsync(entity, CancellationToken.None);
        await _dataAccess.SaveChangesAsync(CancellationToken.None);

        // Act
        propertyName?.SetValue(entity, "Updated");

        _dataAccess.Update(entity);
        await _dataAccess.SaveChangesAsync(CancellationToken.None);

        var result = await _dataAccess.GetByIdAsync(id);

        // Assert
        Assert.AreEqual("Updated", propertyName?.GetValue(result)?.ToString());
    }

    [TestMethod]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var entity = CreateInstance();
        var propertyName = entity.GetType().GetProperty("Name");

        // Set entity properties
        propertyName?.SetValue(entity, "To Delete");

        TID id = entity.SetId<TEntity, TID>();

        await _dataAccess.AddAsync(entity, CancellationToken.None);
        await _dataAccess.SaveChangesAsync(CancellationToken.None);

        // Act
        _dataAccess.Delete(entity);
        await _dataAccess.SaveChangesAsync(CancellationToken.None);
        var result = await _dataAccess.GetByIdAsync(id);

        // Assert
        Assert.IsNull(result);
    }
}