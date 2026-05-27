namespace AspireApp.Client.ApiClients;

public static class HttpClientNames
{
    public const string Api = "ApiClient";

    /// <summary>HttpClient used for refresh/login/logout calls, without the JwtTokenHandler to avoid recursion.</summary>
    public const string ApiRaw = "ApiClientRaw";
}
