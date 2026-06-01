using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AspireApp.Application.Contracts.Auth;
using AspireApp.Domain.ROP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AspireApp.Infrastructure.Identity;

/// <summary>
/// Validates Google tokens via OIDC id_token (preferred) or OAuth access_token + UserInfo.
/// </summary>
internal sealed class GoogleIdentityValidator(
    IOptions<SsoOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleIdentityValidator> logger) : IExternalIdentityValidator
{
    private readonly GoogleSsoOptions _options = options.Value.Google;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager = CreateConfigManager(options.Value.Google);

    public string Provider => "Google";

    public Task<Result<ExternalIdentity>> ValidateIdTokenAsync(string idToken, CancellationToken ct) =>
        ValidateIdTokenCoreAsync(idToken, ct);

    public async Task<Result<ExternalIdentity>> ValidateAccessTokenAsync(string accessToken, CancellationToken ct)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(_options.ClientId))
            return Result.Failure<ExternalIdentity>("Google SSO is not enabled.", System.Net.HttpStatusCode.BadRequest);

        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<ExternalIdentity>("Access token is required.", System.Net.HttpStatusCode.BadRequest);

        try
        {
            var client = httpClientFactory.CreateClient(nameof(GoogleIdentityValidator));
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Google userinfo failed with status {Status}.", response.StatusCode);
                return Result.Unauthorized<ExternalIdentity>("Invalid Google access token.");
            }

            var profile = await response.Content.ReadFromJsonAsync<GoogleUserInfo>(ct);
            if (profile is null || string.IsNullOrEmpty(profile.Sub) || string.IsNullOrEmpty(profile.Email))
                return Result.Failure<ExternalIdentity>("Google userinfo is missing required claims.", System.Net.HttpStatusCode.Unauthorized);

            return new ExternalIdentity(
                Provider,
                profile.Sub,
                profile.Email,
                profile.EmailVerified,
                profile.GivenName,
                profile.FamilyName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google access_token validation failed.");
            return Result.Unauthorized<ExternalIdentity>("Invalid Google access token.");
        }
    }

    private async Task<Result<ExternalIdentity>> ValidateIdTokenCoreAsync(string idToken, CancellationToken ct)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(_options.ClientId))
            return Result.Failure<ExternalIdentity>("Google SSO is not enabled.", System.Net.HttpStatusCode.BadRequest);

        if (string.IsNullOrWhiteSpace(idToken))
            return Result.Failure<ExternalIdentity>("id_token is required.", System.Net.HttpStatusCode.BadRequest);

        try
        {
            var config = await _configManager.GetConfigurationAsync(ct);

            var clientId = _options.ClientId!;
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = [config.Issuer, "https://accounts.google.com", "accounts.google.com"],
                ValidateAudience = true,
                AudienceValidator = (audiences, token, _) => MatchesGoogleClientId(audiences, token, clientId),
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
            logger.LogWarning(ex, "Google id_token validation failed.");
            return Result.Unauthorized<ExternalIdentity>("Invalid Google id_token.");
        }
    }

    private static bool MatchesGoogleClientId(IEnumerable<string> audiences, SecurityToken token, string clientId)
    {
        if (audiences.Any(a => string.Equals(a, clientId, StringComparison.Ordinal)))
            return true;

        if (token is JwtSecurityToken jwt
            && jwt.Payload.TryGetValue("azp", out var azp)
            && string.Equals(azp?.ToString(), clientId, StringComparison.Ordinal))
            return true;

        return false;
    }

    private static ConfigurationManager<OpenIdConnectConfiguration> CreateConfigManager(GoogleSsoOptions google)
    {
        var metadataAddress = google.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
        return new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
    }

    private sealed class GoogleUserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }
}
