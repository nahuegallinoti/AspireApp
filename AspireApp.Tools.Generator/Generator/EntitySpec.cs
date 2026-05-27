namespace AspireApp.Tools.Generator.Generator;

internal sealed record EntitySpec(
    string Name,
    string IdType,
    IReadOnlyList<PropertySpec> Properties,
    bool GenerateBlazorPage,
    bool RegisterInNavMenu,
    bool RequireAuth,
    string? IconOverride = null,
    string? AccentOverride = null)
{
    public string Lower => Name.ToLowerInvariant();
    public string Camel => char.ToLowerInvariant(Name[0]) + Name[1..];
    public string Plural => SimplePluralize(Name);
    public string PluralLower => Plural.ToLowerInvariant();
    public string PluralCamel => char.ToLowerInvariant(Plural[0]) + Plural[1..];

    /// <summary>Bootstrap Icons class name (without the leading "bi-") for this entity.</summary>
    public string Icon => IconOverride ?? IconPicker.PickFor(Name);

    /// <summary>Bootstrap theme accent (primary, success, info, warning, danger).</summary>
    public string Accent => AccentOverride ?? AccentPicker.PickFor(Name);

    private static string SimplePluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular)) return singular;
        if (singular.EndsWith('y') && singular.Length > 1 && !"aeiou".Contains(singular[^2]))
            return singular[..^1] + "ies";
        if (singular.EndsWith('s') || singular.EndsWith('x') || singular.EndsWith("ch", StringComparison.Ordinal) || singular.EndsWith("sh", StringComparison.Ordinal))
            return singular + "es";
        return singular + "s";
    }
}
