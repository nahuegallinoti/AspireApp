using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Login;
using AspireApp.Application.Contracts.RegisterUser;
using AspireApp.Core.Mappers;
using AspireApp.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IRegisterUserService registerUserService, ILoginUserService loginUserService, UsuarioMapper usuarioMapper) : ControllerBase
{
    private readonly IRegisterUserService _registerUserService = registerUserService;
    private readonly ILoginUserService _loginUserService = loginUserService;
    private readonly UsuarioMapper _usuarioMapper = usuarioMapper;

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
        Usuario usuario = _usuarioMapper.ToEntity(model);

        var result = await _registerUserService.AddUser(usuario, cancellationToken);

        return result.Success
            ? CreatedAtAction(nameof(Register), new { id = usuario.Id }, _usuarioMapper.ToModel(usuario))
            : Problem(
                detail: string.Join("; ", result.Errors),
                statusCode: (int)result.HttpStatusCode
            );
    }

}