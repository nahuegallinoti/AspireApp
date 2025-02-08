using AspireApp.Core.ROP;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AspireApp.Web.ApiClients;

public abstract class BaseApiClient(IHttpClientFactory httpClientFactory, string clientName)
{
    protected readonly HttpClient _httpClient = httpClientFactory.CreateClient(clientName);
    public record ProblemDetails(int Status, string? Detail, string? Type, string? Title, string? Instance);

    /// <summary>
    /// Envía una solicitud POST a la URL especificada con los datos indicados.
    /// </summary>
    public Task<Result<T>> PostAsync<T, U>(string url, U data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Post, url, JsonContent.Create(data), cancellationToken);

    /// <summary>
    /// Envía una solicitud GET a la URL especificada.
    /// </summary>
    public Task<Result<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Get, url, cancellationToken: cancellationToken);

    /// <summary>
    /// Envía una solicitud PUT a la URL especificada con los datos indicados.
    /// </summary>
    public Task<Result<T>> PutAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Put, url, JsonContent.Create(data), cancellationToken);

    /// <summary>
    /// Envía una solicitud DELETE a la URL especificada.
    /// </summary>
    public Task<Result<T>> DeleteAsync<T>(string url, CancellationToken cancellationToken = default) =>
        SendRequestAsync<T>(HttpMethod.Delete, url, cancellationToken: cancellationToken);

    /// <summary>
    /// Envía una solicitud HTTP con el método, URL y contenido especificados.
    /// </summary>
    private async Task<Result<T>> SendRequestAsync<T>(HttpMethod method,string url,HttpContent? content = null,CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url) { Content = content };
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                ImmutableArray<string> errorMessages = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => await ExtractErrorMessage(response),
                    HttpStatusCode.Unauthorized => ["Credenciales inválidas"],
                    HttpStatusCode.InternalServerError => ["Error en el servidor. Por favor intente más tarde"],
                    _ => ["Error desconocido"]
                };

                return Result.Failure<T>(errorMessages, response.StatusCode);
            }

            var resultObj = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

            return resultObj is not null
                ? Result.Success(resultObj, response.StatusCode)
                : Result.Failure<T>(["Respuesta vacía o nula del servidor."], response.StatusCode);
        }

        catch (HttpRequestException ex)
        {
            ImmutableArray<string> errorMessages = ex.StatusCode switch
            {
                HttpStatusCode.BadRequest => ["Error de solicitud incorrecta"],
                HttpStatusCode.NotFound => ["Recurso no encontrado"],
                _ => [$"Error de conexión: {ex.Message}"]
            };

            return Result.Failure<T>(errorMessages, ex.StatusCode ?? HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>([$"Error inesperado: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Extrae el mensaje de error de la respuesta HTTP.
    /// </summary>
    private static async Task<ImmutableArray<string>> ExtractErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return [$"Error HTTP {response.StatusCode}"];

            try
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrEmpty(problemDetails?.Detail))
                    return [problemDetails.Detail];
            }
            catch
            {
                // Si no se puede parsear como ProblemDetails, se devuelve el contenido original.
            }

            // Si no se pudo parsear o el contenido está vacío, devolvemos el contenido original.
            return [content];
        }
        catch
        {
            return ["Ocurrió un error desconocido al procesar la respuesta del servidor."];
        }
    }
}