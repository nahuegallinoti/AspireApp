namespace AspireApp.Tools.Generator.Generator;

internal sealed record PropertySpec(string Name, string Type, bool Required)
{
    public string CamelName => char.ToLowerInvariant(Name[0]) + Name[1..];

    public bool IsString => Type.Equals("string", StringComparison.OrdinalIgnoreCase);

    public string DefaultSuffix => Type switch
    {
        "string" => " = string.Empty;",
        _ => string.Empty
    };

    public bool IsNumeric => Type is "int" or "long" or "short" or "byte" or "decimal" or "double" or "float";

    public bool IsBool => Type == "bool";

    public bool IsDateTime => Type is "DateTime" or "DateTimeOffset" or "DateOnly" or "TimeOnly";

    /// <summary>
    /// Razor input component name for this property.
    /// </summary>
    public string RazorInputComponent => Type switch
    {
        "string" => "InputText",
        "int" or "long" or "short" or "byte" => "InputNumber",
        "decimal" or "double" or "float" => "InputNumber",
        "bool" => "InputCheckbox",
        "DateTime" or "DateTimeOffset" => "InputDate",
        "DateOnly" => "InputDate",
        _ => "InputText"
    };

    public static PropertySpec Parse(string raw)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(raw);
        var parts = raw.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new ArgumentException($"Invalid property '{raw}'. Expected format 'Name:type[:required]'.");

        var name = parts[0];
        var type = NormalizeType(parts[1]);
        var required = parts.Length > 2 && parts[2].Equals("required", StringComparison.OrdinalIgnoreCase);

        return new PropertySpec(Capitalize(name), type, required);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string NormalizeType(string type) => type.ToLowerInvariant() switch
    {
        "string" => "string",
        "int" or "int32" => "int",
        "long" or "int64" => "long",
        "short" or "int16" => "short",
        "byte" => "byte",
        "decimal" => "decimal",
        "double" => "double",
        "float" or "single" => "float",
        "bool" or "boolean" => "bool",
        "guid" => "Guid",
        "datetime" => "DateTime",
        "datetimeoffset" => "DateTimeOffset",
        "dateonly" => "DateOnly",
        "timeonly" => "TimeOnly",
        _ => type
    };
}
