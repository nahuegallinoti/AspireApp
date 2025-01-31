using System.Net.Http.Headers;

namespace AspireApp.Web.Handlers;

public class JWTTokenHandler(JWTTokenProvider jwtTokenProvider) : DelegatingHandler
{
    private readonly JWTTokenProvider _jwtTokenProvider = jwtTokenProvider;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _jwtTokenProvider.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            // Agregar el token al encabezado de la solicitud
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Llamar al siguiente handler en la cadena
        return await base.SendAsync(request, cancellationToken);
    }
}
