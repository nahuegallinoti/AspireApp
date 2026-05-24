using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspireApp.Application.Implementations.Extensions;

public static class AppJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
