using AspireApp.Api.Infrastructure;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] UserLogin model, CancellationToken ct) =>
        (await authService.LoginAsync(model, HttpContext.GetClientIp(), ct)).ToActionResult(this);

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] UserRegister model, CancellationToken ct) =>
        (await authService.RegisterAsync(model, HttpContext.GetClientIp(), ct)).ToActionResult(this);

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct) =>
        (await authService.RefreshAsync(request, HttpContext.GetClientIp(), ct)).ToActionResult(this);

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var result = await authService.LogoutAsync(request, HttpContext.GetClientIp(), ct);
        return result.ToActionResult(this, _ => NoContent());
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await authService.GetCurrentAsync(userId.Value, ct);
        return result.ToActionResult(this);
    }
}
