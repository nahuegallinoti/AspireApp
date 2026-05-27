using AspireApp.Api.Infrastructure;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Controllers;

[ApiController]
[Route("api/auth/external")]
[Produces("application/json")]
[AllowAnonymous]
public class ExternalAuthController(IExternalAuthService externalAuthService) : ControllerBase
{
    /// <summary>
    /// Exchanges a Google id_token (or any configured SSO provider id_token) for application access + refresh tokens.
    /// </summary>
    [HttpPost("{provider}")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(string provider, [FromBody] ExternalProviderPayload payload, CancellationToken ct)
    {
        var request = new ExternalLoginRequest
        {
            Provider = provider,
            IdToken = payload?.IdToken,
            AccessToken = payload?.AccessToken
        };
        var result = await externalAuthService.LoginAsync(request, HttpContext.GetClientIp(), ct);
        return result.ToActionResult(this);
    }

    public sealed record ExternalProviderPayload(string? IdToken, string? AccessToken);
}
