using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Models;
using AspireApp.Tests.Client.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public abstract class BaseControllerTest<T, TID, TController, TService>
        where T : BaseModel<TID>
        where TID : struct
        where TController : BaseController<T, TID, TService>
        where TService : class, IBaseService<T, TID>
{
    protected Mock<TService> _serviceMock = null!;
    protected TController _controller = null!;

    protected abstract TController CreateController();

    // Test para obtener todos los elementos
    [TestMethod]
    public async Task GetAll_ShouldReturnOkResult_WithListOfEntities()
    {
        var entities = new List<T>
            {
                Activator.CreateInstance<T>(),
                Activator.CreateInstance<T>()
            };
        _serviceMock.Setup(service => service.GetAllAsync(CancellationToken.None)).ReturnsAsync(entities);

        var result = await _controller.GetAll();

        Assert.IsInstanceOfType<OkObjectResult>(result);

        var okResult = result as OkObjectResult;

        Assert.AreEqual(entities, okResult?.Value);
    }

    [TestMethod]
    public async Task GetById_ShouldReturnOkResult_WhenEntityExists()
    {
        // Arrange
        var id = Activator.CreateInstance<TID>();
        var model = Activator.CreateInstance<T>();
        model.Id = id!;
        _serviceMock.Setup(service => service.GetByIdAsync(id)).ReturnsAsync(model);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var value = (result as OkObjectResult)?.Value as T;
        Assert.IsNotNull(value);
        Assert.AreEqual(id, value?.Id);
    }

    [TestMethod]
    public async Task GetById_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        var id = Activator.CreateInstance<TID>();
        _serviceMock.Setup(service => service.GetByIdAsync(id)).ReturnsAsync(null as T);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Add_ShouldReturnCreatedAtAction_WhenEntityIsAdded()
    {
        // Arrange
        var model = Activator.CreateInstance<T>();

        _serviceMock.Setup(service => service.AddAsync(model, CancellationToken.None)).ReturnsAsync(model);
        _serviceMock.Setup(service => service.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Add(model);

        // Assert
        Assert.IsInstanceOfType<CreatedAtActionResult>(result);

        var created = result as CreatedAtActionResult;

        Assert.AreEqual(201, created?.StatusCode);
        Assert.AreEqual(model, created?.Value);
    }

    [TestMethod]
    public void Update_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var id = Activator.CreateInstance<TID>(); //Setea id default
        var model = Activator.CreateInstance<T>();

        model.SetId<T, TID>(); //Setea id aleatorio

        // Act
        var result = _controller.Update(id, model);

        // Assert
        Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task Delete_ShouldReturnNoContent_WhenEntityIsDeleted()
    {
        // Arrange
        var id = Activator.CreateInstance<TID>();
        var model = Activator.CreateInstance<T>();
        model.Id = id!;
        _serviceMock.Setup(service => service.GetByIdAsync(id)).ReturnsAsync(model);
        _serviceMock.Setup(service => service.Delete(model));
        _serviceMock.Setup(service => service.SaveChangesAsync(CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task Delete_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        var id = Activator.CreateInstance<TID>();
        _serviceMock.Setup(service => service.GetByIdAsync(id)).ReturnsAsync(null as T);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }
}
