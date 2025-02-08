using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspireApp.Application.Implementations.Extensions;

public static class JsonSerializerExtensions
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}