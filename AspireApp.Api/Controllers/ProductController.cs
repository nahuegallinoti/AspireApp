using AspireApp.Api.Models.App;
using AspireApp.Application.Contracts.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController(IProductService productService) : BaseController<Product, long, IProductService>(productService)
{

}