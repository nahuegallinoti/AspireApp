using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Models.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductController(IProductService productService, IMessageBus messageBus, ILogger<ProductController> logger)
    : BaseController<Product, long, IProductService>(productService, messageBus, logger);
