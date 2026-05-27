using Microsoft.Extensions.Configuration;

namespace AspireApp.AppHost;

/// <summary>
/// Lee credenciales SSO desde variables de entorno (prioridad) o configuración (appsettings / user-secrets).
/// </summary>
public static class SsoConfiguration
{
    /// <summary>ID de cliente OAuth 2.0 de Google Cloud Console.</summary>
    public const string GoogleClientIdVariable = "GOOGLE_CLIENT_ID";

    /// <summary>Secreto de cliente OAuth 2.0 de Google Cloud Console.</summary>
    public const string GoogleClientSecretVariable = "GOOGLE_CLIENT_SECRET";

    /// <summary>true | false. Si no está definida y hay ClientId + Secret válidos, SSO se activa automáticamente.</summary>
    public const string GoogleEnabledVariable = "SSO_GOOGLE_ENABLED";

    public static GoogleSsoSettings ReadGoogle(IConfiguration configuration)
    {
        var clientId = FirstNonEmpty(
            Environment.GetEnvironmentVariable(GoogleClientIdVariable),
            configuration["Sso:Google:ClientId"]);

        var clientSecret = FirstNonEmpty(
            Environment.GetEnvironmentVariable(GoogleClientSecretVariable),
            configuration["Sso:Google:ClientSecret"]);

        var enabledOverride = FirstNonEmpty(
            Environment.GetEnvironmentVariable(GoogleEnabledVariable),
            configuration["Sso:Google:Enabled"]);

        var hasCredentials = IsUsableCredential(clientId) && IsUsableCredential(clientSecret);

        var enabled = enabledOverride switch
        {
            null => hasCredentials,
            _ when IsTruthy(enabledOverride) => hasCredentials,
            _ => false
        };

        return new GoogleSsoSettings(enabled, clientId ?? string.Empty, clientSecret ?? string.Empty);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return null;
    }

    private static bool IsUsableCredential(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.StartsWith("REEMPLAZAR", StringComparison.OrdinalIgnoreCase);

    private static bool IsTruthy(string value) =>
        value.Equals("true", StringComparison.OrdinalIgnoreCase)
        || value.Equals("1", StringComparison.OrdinalIgnoreCase)
        || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
}

public sealed record GoogleSsoSettings(bool Enabled, string ClientId, string ClientSecret);
