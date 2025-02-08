using AspireApp.Application.Contracts.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dto = AspireApp.Api.Domain.Models;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController(IProductService productService) : BaseController<Dto.Product, long, IProductService>(productService)
{

}