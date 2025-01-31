using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerDatos()
    {
        return Ok(new { Mensaje = "Capardo" });
    }
}