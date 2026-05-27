using System.IdentityModel.Tokens.Jwt;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AspireApp.Application.Implementations.Auth;

/// <summary>
/// Validates Google-issued id_tokens by fetching the JWKS from the OIDC discovery endpoint.
/// </summary>
internal sealed class GoogleIdentityValidator : IExternalIdentityValidator
{
    private readonly GoogleSsoOptions _options;
    private readonly ILogger<GoogleIdentityValidator> _logger;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager;

    public string Provider => "Google";

    public GoogleIdentityValidator(IOptions<SsoOptions> options, ILogger<GoogleIdentityValidator> logger)
    {
        _options = options.Value.Google;
        _logger = logger;
        var metadataAddress = _options.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
    }

    public async Task<Result<ExternalIdentity>> ValidateAsync(string idToken, CancellationToken ct)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(_options.ClientId))
            return Result.Failure<ExternalIdentity>("Google SSO is not enabled.", System.Net.HttpStatusCode.BadRequest);

        try
        {
            var config = await _configManager.GetConfigurationAsync(ct);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = [config.Issuer, "https://accounts.google.com", "accounts.google.com"],
                ValidateAudience = true,
                ValidAudience = _options.ClientId,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds),
                IssuerSigningKeys = config.SigningKeys
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(idToken, parameters, out _);

            var sub = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var emailVerified = bool.TryParse(principal.FindFirst("email_verified")?.Value, out var ev) && ev;
            var given = principal.FindFirst("given_name")?.Value;
            var family = principal.FindFirst("family_name")?.Value;

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
                return Result.Failure<ExternalIdentity>("id_token is missing required claims.", System.Net.HttpStatusCode.Unauthorized);

            return new ExternalIdentity(Provider, sub, email, emailVerified, given, family);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Google id_token validation failed.");
            return Result.Unauthorized<ExternalIdentity>("Invalid Google id_token.");
        }
    }
}
