namespace AspireApp.Web.Handlers;

public class JWTTokenProvider
{
    private string? _token;

    public string? GetToken() => _token;
    public void SetToken(string token) => _token = token;
    public void ClearToken() => _token = null;
}