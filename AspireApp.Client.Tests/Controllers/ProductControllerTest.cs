﻿using AspireApp.Api.Controllers;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Contracts.Rabbit;
using Moq;
using Dto = AspireApp.Api.Models.App;

namespace AspireApp.Tests.Client.Controllers;

[TestClass]
public class ProductControllerTest : BaseControllerTest<Dto.Product, long, ProductController, IProductService>
{
    private Mock<IRabbitMqService> _rabbitMock = null!;

    [TestInitialize]
    public void Init()
    {
        _serviceMock = new Mock<IProductService>();
        _rabbitMock = new Mock<IRabbitMqService>();
        _controller = CreateController();
    }

    protected override ProductController CreateController()
    {
        return new ProductController(_serviceMock.Object, _rabbitMock.Object);
    }

}