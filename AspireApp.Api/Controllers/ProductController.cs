using AspireApp.Api.Models.App;
using AspireApp.Application.Contracts.EventBus;
using AspireApp.Application.Contracts.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController(IProductService productService, IMessageBus messageBus, ILogger<ProductController> logger)
    :
    BaseController<Product, long, IProductService>(productService, messageBus, logger)
{
    //private readonly IRabbitMqService _rabbit = rabbitMqService;
    //private readonly IProductService _productService = productService;
    //public override async Task<IActionResult> Add([FromBody] Product model, CancellationToken ct = default)
    //{
    //    await _productService.AddAsync(model, ct);
    //    //await _rabbit.SendMessage(new() { Message = model.Name }, $"cola-{model.Name}");

    //    return CreatedAtAction(nameof(Add), new { id = model.Id }, model);
    //}
}