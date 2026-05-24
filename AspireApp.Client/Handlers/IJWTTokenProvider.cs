namespace AspireApp.Client.Handlers;

public interface IJwtTokenProvider
{
    string? GetToken();
    void SetToken(string token);
    void ClearToken();
}
