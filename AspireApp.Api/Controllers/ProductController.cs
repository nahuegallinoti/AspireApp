using AspireApp.Application.Contracts.Product;
using AspireApp.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : BaseController<Product, long, IProductService>(productService)
{
    protected readonly IProductService _productService;
}