using AspireApp.Api.Tests.Extensions;
using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Implementations.Base;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Moq;

namespace AspireApp.Api.Tests.Application;

[TestClass]
public abstract class BaseServiceTest<T, TID, TDA>
    where T : BaseEntity<TID>
    where TID : struct
    where TDA : class, IBaseDA<T, TID>
{
    protected Mock<TDA> _baseDAMock = null!;
    protected IBaseService<T, TID, TDA> _baseService = null!;

    private static T CreateInstance() => Activator.CreateInstance<T>();

    [TestInitialize]
    public void Setup()
    {
        _baseDAMock = new(MockBehavior.Strict);
        _baseService = new BaseService<T, TID, TDA>(_baseDAMock.Object);
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        List<T> expectedEntities = [CreateInstance(), CreateInstance()];

        _baseDAMock.Setup(repo => repo.GetAllAsync())
                   .ReturnsAsync(expectedEntities);

        // Act
        IEnumerable<T> result = await _baseService.GetAllAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEntities.Count, result.Count());

        _baseDAMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        // Arrange
        T entity = CreateInstance();
        TID id = entity.SetId<T, TID>();

        _baseDAMock.Setup(repo => repo.GetByIdAsync(id))
                   .ReturnsAsync(entity);

        // Act
        T? retrievedEntity = await _baseService.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(retrievedEntity);
        Assert.AreEqual(entity, retrievedEntity);

        _baseDAMock.Verify(repo => repo.GetByIdAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        T entity = CreateInstance();

        _baseDAMock.Setup(repo => repo.AddAsync(entity)).Returns(Task.CompletedTask);

        // Act
        await _baseService.AddAsync(entity);

        // Assert
        _baseDAMock.Verify(repo => repo.AddAsync(entity), Times.Once);
    }

    [TestMethod]
    public void Delete_ShouldRemoveEntity()
    {
        // Arrange
        T entity = CreateInstance();

        _baseDAMock.Setup(repo => repo.Delete(entity));

        // Act
        _baseService.Delete(entity);

        // Assert
        _baseDAMock.Verify(repo => repo.Delete(entity), Times.Once);
    }

    [TestMethod]
    public void Update_ShouldModifyEntity()
    {
        // Arrange
        T entity = CreateInstance();

        _baseDAMock.Setup(repo => repo.Update(entity));

        // Act
        _baseService.Update(entity);

        // Assert
        _baseDAMock.Verify(repo => repo.Update(entity), Times.Once);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldCommitChanges()
    {
        // Arrange
        _baseDAMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _baseService.SaveChangesAsync();

        // Assert
        _baseDAMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}