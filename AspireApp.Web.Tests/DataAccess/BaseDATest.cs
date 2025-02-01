using AspireApp.Api.Tests.Extensions;
using AspireApp.DataAccess.Implementations;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.Api.Tests.DataAccess;

public abstract class BaseDATest<TEntity, TID> where TEntity : BaseEntity<TID>
                                               where TID : struct
{
    protected AppDbContext _context = null!;
    protected DbContextOptions<AppDbContext> _options = null!;
    protected BaseDA<TEntity, TID> _baseDA = null!;

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(_options);
        _baseDA = new BaseDA<TEntity, TID>(_context);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var entity = Activator.CreateInstance<TEntity>();
        var propertyName = entity.GetType().GetProperty("Name");

        // Set entity properties
        propertyName?.SetValue(entity, "Test Entity");

        TID id = entity.SetId<TEntity, TID>();

        // Act
        await _baseDA.AddAsync(entity);
        await _baseDA.SaveChangesAsync();
        var result = await _baseDA.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Entity", propertyName?.GetValue(result)?.ToString());
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        _context.Set<TEntity>().AddRange(Activator.CreateInstance<TEntity>(), Activator.CreateInstance<TEntity>());
        await _context.SaveChangesAsync();

        // Act
        var result = await _baseDA.GetAllAsync();

        // Assert
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    public async Task Update_ShouldModifyEntity()
    {
        // TODO: si no existe la propiedad name en la entidad va a fallar

        // Arrange
        var entity = Activator.CreateInstance<TEntity>();
        var propertyName = entity.GetType().GetProperty("Name");

        // Set initial entity properties
        propertyName?.SetValue(entity, "Original");

        TID id = entity.SetId<TEntity, TID>();

        await _baseDA.AddAsync(entity);
        await _baseDA.SaveChangesAsync();

        // Act
        propertyName?.SetValue(entity, "Updated");

        _baseDA.Update(entity);
        await _baseDA.SaveChangesAsync();

        var result = await _baseDA.GetByIdAsync(id);

        // Assert
        Assert.AreEqual("Updated", propertyName?.GetValue(result)?.ToString());
    }

    [TestMethod]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var entity = Activator.CreateInstance<TEntity>();
        var propertyName = entity.GetType().GetProperty("Name");
        var propertyId = entity.GetType().GetProperty("Id");

        // Set entity properties
        propertyName?.SetValue(entity, "To Delete");

        if (propertyId is not null)
        {
            var newId = Activator.CreateInstance<TID>();
            propertyId.SetValue(entity, newId);
        }

        await _baseDA.AddAsync(entity);
        await _baseDA.SaveChangesAsync();

        // Act
        _baseDA.Delete(entity);
        await _baseDA.SaveChangesAsync();
        var result = await _baseDA.GetByIdAsync(Activator.CreateInstance<TID>());

        // Assert
        Assert.IsNull(result);
    }
}