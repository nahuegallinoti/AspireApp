using AspireApp.Api.Domain;
using AspireApp.Api.Tests.Extensions;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Moq;

namespace AspireApp.Api.Tests.Application;

[TestClass]
public abstract class BaseServiceTest<TE, TM, TID, TDA>
    where TE : BaseEntity<TID>
    where TM : BaseModel<TID>
    where TID : struct
    where TDA : class, IBaseDA<TE, TID>
{
    protected Mock<TDA> _baseDAMock = null!;
    protected BaseService<TE, TM, TID> _baseService = null!;

    protected BaseMapper<TM, TE> _mapper = null!;

    protected void InitializeMapper(BaseMapper<TM, TE> mapper)
    {
        _mapper = mapper;
    }

    private static TE CreateInstance() => Activator.CreateInstance<TE>();
    private static TM CreateInstanceModel() => Activator.CreateInstance<TM>();

    [TestInitialize]
    public void Setup()
    {
        _baseDAMock = new(MockBehavior.Default);
        _baseService = new BaseService<TE, TM, TID>(_baseDAMock.Object, _mapper);
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        List<TE> expectedEntities = [CreateInstance(), CreateInstance()];

        _baseDAMock.Setup(repo => repo.GetAllAsync())
                   .ReturnsAsync(expectedEntities);

        // Act
        IEnumerable<TM> result = await _baseService.GetAllAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEntities.Count, result.Count());

        _baseDAMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        // Arrange
        TE entity = CreateInstance();
        TID id = entity.SetId<TE, TID>();

        _baseDAMock.Setup(repo => repo.GetByIdAsync(id))
                   .ReturnsAsync(entity);

        // Act
        TM? retrievedEntity = await _baseService.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(retrievedEntity);
        Assert.AreEqual(entity.Id, retrievedEntity.Id);

        _baseDAMock.Verify(repo => repo.GetByIdAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        model.SetId<TM, TID>();
        model.GetType().GetProperty("Name")?.SetValue(model, "nagu");

        TE entity = _mapper.ToEntity(model);

        _baseDAMock.Setup(repo => repo.AddAsync(entity)).Returns(Task.CompletedTask);

        // Act
        await _baseService.AddAsync(model);

        // Assert
        _baseDAMock.Verify(repo => repo.AddAsync(entity), Times.Once);
    }

    [TestMethod]
    public void Delete_ShouldRemoveEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        TE entity = _mapper.ToEntity(model);

        _baseDAMock.Setup(repo => repo.Delete(entity));

        // Act
        _baseService.Delete(model);

        // Assert
        _baseDAMock.Verify(repo => repo.Delete(entity), Times.Once);
    }

    [TestMethod]
    public void Update_ShouldModifyEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        TE entity = _mapper.ToEntity(model);

        _baseDAMock.Setup(repo => repo.Update(entity));

        // Act
        _baseService.Update(model);

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