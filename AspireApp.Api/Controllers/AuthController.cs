using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.User;
using AspireApp.Application.Models.Auth.User;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IRegisterUserService registerService, ILoginService loginService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin model, CancellationToken ct)
    {
        var result = await loginService.Login(model, ct);
        return result.Success
            ? Ok(result.Value)
            : Problem(detail: string.Join("; ", result.Errors), statusCode: (int)result.HttpStatusCode);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegister model, CancellationToken ct)
    {
        var result = await registerService.RegisterAsync(model, ct);
        return result.Success
            ? CreatedAtAction(nameof(Register), result.Value)
            : Problem(detail: string.Join("; ", result.Errors), statusCode: (int)result.HttpStatusCode);
    }
}
