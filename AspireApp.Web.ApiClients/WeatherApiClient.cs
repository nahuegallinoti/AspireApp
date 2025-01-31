using System.Net.Http.Json;

namespace AspireApp.Web.ApiClients;
public class WeatherApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("ApiClient");

    public async Task<MensajeResult?> GetWeatherAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<MensajeResult>("/api/weather");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }
}

public record class MensajeResult(string? Mensaje);