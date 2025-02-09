﻿using AspireApp.Api.Domain;
using AspireApp.Api.Tests.Extensions;
using AspireApp.Application.Implementations.Base;
using AspireApp.Core.Mappers;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.Entities.Base;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
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

    private Mock<HybridCache> _cache = null!;

    protected void InitializeMapper(BaseMapper<TM, TE> mapper)
    {
        _mapper = mapper;
    }

    private static TE CreateInstanceEntity() => Activator.CreateInstance<TE>();
    private static TM CreateInstanceModel() => Activator.CreateInstance<TM>();

    [TestInitialize]
    public void Setup()
    {
        _baseDAMock = new(MockBehavior.Default);
        _cache = new(MockBehavior.Default);
        _baseService = new BaseService<TE, TM, TID>(_baseDAMock.Object, _mapper, _cache.Object);
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        List<TE> expectedEntities = [CreateInstanceEntity(), CreateInstanceEntity()];

        _baseDAMock.Setup(repo => repo.GetAllAsync(CancellationToken.None))
                   .ReturnsAsync(expectedEntities);

        // Act
        IEnumerable<TM> result = await _baseService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEntities.Count, result.Count());

        _baseDAMock.Verify(repo => repo.GetAllAsync(CancellationToken.None), Times.Once);
    }

    // TODO: Terminar

    [TestMethod]
    public async Task GetByIdAsync_ShouldReturnEntity_FromCache()
    {
        // Arrange
        TE entity = CreateInstanceEntity();
        TID id = entity.SetId<TE, TID>();

        string modelName = typeof(TM).Name;
        string cacheKey = $"{modelName}:{id}";

        // Configurar el mock del caché para que devuelva la entidad cuando se le consulte
        _cache.Setup(expression: c => c.GetOrCreateAsync(
            cacheKey,
            It.IsAny<string>(),
            It.IsAny<Func<string, CancellationToken, ValueTask<TE>>>(),
            null,
            It.IsAny<IEnumerable<string>?>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(entity);

        // Act
        TM? retrievedEntity = await _baseService.GetByIdAsync(id);

        // Assert
        Assert.IsNotNull(retrievedEntity);
        Assert.AreEqual(entity.Id, retrievedEntity.Id);

        // Verificar que NO se haya llamado al repositorio ya que el valor proviene de la caché
        _baseDAMock.Verify(repo => repo.GetByIdAsync(id), Times.Never);

        // Verificar que la caché fue consultada correctamente
        _cache.Verify(c => c.GetOrCreateAsync(
            cacheKey,
            It.IsAny<string>(),
            It.IsAny<Func<string, CancellationToken, ValueTask<TE>>>(),
            null,
            It.IsAny<IEnumerable<string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        model.SetId<TM, TID>();
        model.GetType().GetProperty("Name")?.SetValue(model, "nagu");

        // Se usa It.IsAny<TE> para que el mock acepte cualquier instancia de TEntity
        _baseDAMock.Setup(repo => repo.AddAsync(It.IsAny<TE>(), CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        await _baseService.AddAsync(model, CancellationToken.None);

        // Assert
        _baseDAMock.Verify(repo => repo.AddAsync(It.IsAny<TE>(), CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public void Delete_ShouldRemoveEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        _baseDAMock.Setup(repo => repo.Delete(It.IsAny<TE>()));

        // Act
        _baseService.Delete(model);

        // Assert
        _baseDAMock.Verify(repo => repo.Delete(It.IsAny<TE>()), Times.Once);
    }

    [TestMethod]
    public void Update_ShouldModifyEntity()
    {
        // Arrange
        TM model = CreateInstanceModel();

        _baseDAMock.Setup(repo => repo.Update(It.IsAny<TE>()));

        // Act
        _baseService.Update(model);

        // Assert
        _baseDAMock.Verify(repo => repo.Update(It.IsAny<TE>()), Times.Once);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldCommitChanges()
    {
        // Arrange
        _baseDAMock.Setup(repo => repo.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        await _baseService.SaveChangesAsync(CancellationToken.None);

        // Assert
        _baseDAMock.Verify(repo => repo.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}