using System.Security.Claims;

namespace AspireApp.Api.Infrastructure;

internal static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var id = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? httpContext.User.FindFirstValue("sub");
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public static string? GetClientIp(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var first = forwarded.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
                return first;
        }
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
