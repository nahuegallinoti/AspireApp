namespace AspireApp.Client.Handlers;

public interface IJWTTokenProvider
{
    string? GetToken();
    void SetToken(string token);
    void ClearToken();
}