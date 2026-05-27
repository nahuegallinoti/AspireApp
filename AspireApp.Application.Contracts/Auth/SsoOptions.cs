using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Contracts.Auth;

public sealed class SsoOptions
{
    public const string SectionName = "Sso";

    public GoogleSsoOptions Google { get; set; } = new();
}

public sealed class GoogleSsoOptions
{
    public bool Enabled { get; set; }

    public string Authority { get; set; } = "https://accounts.google.com";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    [Range(0, 600)]
    public int ClockSkewSeconds { get; set; } = 30;
}
