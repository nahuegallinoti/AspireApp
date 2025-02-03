using AspireApp.Application.Contracts.Base;
using AspireApp.Application.Contracts.Product;
using AspireApp.Application.Implementations.Base;
using AspireApp.Application.Implementations.Product;
using AspireApp.DataAccess.Contracts;
using AspireApp.DataAccess.Contracts.Base;
using AspireApp.DataAccess.Implementations.Base;
using AspireApp.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : BaseController<Product, long, IProductService>
{
    public ProductController(IProductDA productService) : base(productService)
    {
    }
}