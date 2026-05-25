namespace AspireApp.Tools.Generator.Generator;

internal sealed record EntitySpec(
    string Name,
    string IdType,
    IReadOnlyList<PropertySpec> Properties,
    bool GenerateBlazorPage,
    bool RegisterInNavMenu,
    bool RequireAuth)
{
    public string Lower => Name.ToLowerInvariant();
    public string Camel => char.ToLowerInvariant(Name[0]) + Name[1..];
    public string Plural => SimplePluralize(Name);
    public string PluralLower => Plural.ToLowerInvariant();
    public string PluralCamel => char.ToLowerInvariant(Plural[0]) + Plural[1..];

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
