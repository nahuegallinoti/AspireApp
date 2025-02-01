using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Contracts.User;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IRegisterUserService registerUserService, ILoginService loginUserService) : ControllerBase
{
    private readonly ILoginService _loginUserService = loginUserService;
    private readonly IRegisterUserService _registerUserService = registerUserService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin model, CancellationToken cancellationToken = default)
    {
        var result = await _loginUserService.Login(model, cancellationToken);

        return result.Success
                    ? Ok(result.Value)
                    : Problem(
                        detail: string.Join("; ", result.Errors),
                        statusCode: (int)result.HttpStatusCode
                    );
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegister model, CancellationToken cancellationToken = default)
    {
        var result = await _registerUserService.AddUser(model, cancellationToken);

        return result.Success
                    ? CreatedAtAction(nameof(Register), result.Value)
                    : Problem(
                        detail: string.Join("; ", result.Errors),
                        statusCode: (int)result.HttpStatusCode
                    );
    }

}