using AspireApp.Api.Domain.Auth;
using AspireApp.Api.Domain.Auth.User;
using AspireApp.Application.Contracts.Login;
using AspireApp.Application.Contracts.RegisterUser;
using AspireApp.Core.Mappers;
using AspireApp.Entidad;
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

        return Ok(new AuthenticationResult() { Token = result.Value });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegister model, CancellationToken cancellationToken = default)
    {
        Usuario usuario = _usuarioMapper.ToEntity(model);

        await _registerUserService.AddUser(usuario, cancellationToken);

        UserRegister modelResult = _usuarioMapper.ToModel(usuario);

        return Ok(modelResult);
    }

}