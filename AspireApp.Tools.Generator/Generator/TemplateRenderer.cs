using System.Globalization;
using System.Reflection;
using System.Text;

namespace AspireApp.Tools.Generator.Generator;

internal sealed class TemplateRenderer
{
    private readonly string _templatesRoot;

    public TemplateRenderer()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Could not determine assembly directory.");
        _templatesRoot = Path.Combine(assemblyDir, "Templates");
    }

    public string Render(string templateFileName, EntitySpec entity)
    {
        var path = Path.Combine(_templatesRoot, templateFileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Template not found: {path}");

        var content = File.ReadAllText(path);
        return Replace(content, entity);
    }

    private static string Replace(string content, EntitySpec entity)
    {
        var tokens = BuildTokenMap(entity);

        var sb = new StringBuilder(content);
        foreach (var (key, value) in tokens)
            sb.Replace("{{" + key + "}}", value);

        return sb.ToString();
    }

    private static Dictionary<string, string> BuildTokenMap(EntitySpec entity) => new(StringComparer.Ordinal)
    {
        ["ENTITY"] = entity.Name,
        ["entity"] = entity.Lower,
        ["ENTITY_CAMEL"] = entity.Camel,
        ["ENTITY_PLURAL"] = entity.Plural,
        ["entity_plural"] = entity.PluralLower,
        ["ID_TYPE"] = entity.IdType,
        ["ID_ROUTE_CONSTRAINT"] = BuildIdRouteConstraint(entity.IdType),
        ["PROPS_ENTITY"] = BuildEntityProps(entity),
        ["PROPS_MODEL"] = BuildModelProps(entity),
        ["PROPS_DBCONFIG"] = BuildDbConfigProps(entity),
        ["PROPS_MAPPER_TO_MODEL"] = BuildMapperLines(entity),
        ["PROPS_MAPPER_TO_ENTITY"] = BuildMapperLines(entity),
        ["PROPS_FORM_FIELDS"] = BuildFormFields(entity),
        ["PROPS_TABLE_HEAD"] = BuildTableHead(entity),
        ["PROPS_TABLE_BODY"] = BuildTableBody(entity),
        ["PROPS_FILTER_FIELDS"] = BuildFilterFields(entity),
        ["PROPS_FILTER_STATE"] = BuildFilterState(entity),
        ["PROPS_FILTER_LOGIC"] = BuildFilterLogic(entity),
        ["PROPS_RESET_LOGIC"] = BuildResetLogic(entity),
        ["AUTHORIZE_ATTR"] = entity.RequireAuth ? "[Authorize]\n" : string.Empty,
        ["AUTHORIZE_USING"] = entity.RequireAuth ? "using Microsoft.AspNetCore.Authorization;\n" : string.Empty,
        ["DISPLAY_NAME_EXPR"] = BuildDisplayNameExpression(entity),
    };

    /// <summary>
    /// Builds an expression for a "display name" of an entity instance, used in headers.
    /// Picks the first string property called "Name" (case-insensitive) or falls back to the first string property.
    /// If none, falls back to "#{Id}".
    /// </summary>
    private static string BuildDisplayNameExpression(EntitySpec entity)
    {
        var displayProp =
            entity.Properties.FirstOrDefault(p => p.IsString && p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
            ?? entity.Properties.FirstOrDefault(p => p.IsString && p.Name.Equals("Title", StringComparison.OrdinalIgnoreCase))
            ?? entity.Properties.FirstOrDefault(p => p.IsString);

        return displayProp is null
            ? "$\"#{Id}\""
            : $"!string.IsNullOrWhiteSpace(Model?.{displayProp.Name}) ? Model!.{displayProp.Name} : $\"#{{Id}}\"";
    }

    private static string BuildIdRouteConstraint(string idType) => idType switch
    {
        "long" => ":long",
        "int" => ":int",
        "Guid" => ":guid",
        _ => string.Empty,
    };

    private static string BuildEntityProps(EntitySpec entity)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            var p = entity.Properties[i];
            sb.Append(CultureInfo.InvariantCulture, $"    public {p.Type} {p.Name} {{ get; set; }}{p.DefaultSuffix}");
            if (i < entity.Properties.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildModelProps(EntitySpec entity)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            var p = entity.Properties[i];
            if (p.Required)
                sb.AppendLine("    [Required(ErrorMessage = \"Field {0} is required.\")]");
            sb.Append(CultureInfo.InvariantCulture, $"    public {p.Type} {p.Name} {{ get; set; }}{p.DefaultSuffix}");
            if (i < entity.Properties.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildDbConfigProps(EntitySpec entity)
    {
        var configurable = entity.Properties.Where(p => p.IsString).ToArray();
        if (configurable.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < configurable.Length; i++)
        {
            var p = configurable[i];
            var max = p.Required ? 256 : 2000;
            sb.Append(CultureInfo.InvariantCulture, $"            entity.Property(x => x.{p.Name}).HasMaxLength({max})");
            if (p.Required) sb.Append(".IsRequired()");
            sb.Append(';');
            if (i < configurable.Length - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildMapperLines(EntitySpec entity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        Id = source.Id,");
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            var p = entity.Properties[i];
            sb.Append(CultureInfo.InvariantCulture, $"        {p.Name} = source.{p.Name}");
            if (i < entity.Properties.Count - 1) sb.AppendLine(",");
        }
        return sb.ToString();
    }

    private static string BuildFormFields(EntitySpec entity)
    {
        if (entity.Properties.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            var p = entity.Properties[i];
            var requiredMark = p.Required ? " *" : string.Empty;
            var label = $"{p.Name}{requiredMark}";

            if (p.IsBool)
            {
                sb.AppendLine("                    <div class=\"col-md-6\">");
                sb.AppendLine("                        <div class=\"form-check mt-4\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                            <InputCheckbox id=\"{p.CamelName}\" class=\"form-check-input\" @bind-Value=\"Model.{p.Name}\" />");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                            <label for=\"{p.CamelName}\" class=\"form-check-label\">{label}</label>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                            <ValidationMessage For=\"() => Model.{p.Name}\" class=\"text-danger small d-block\" />");
                sb.AppendLine("                        </div>");
                sb.Append("                    </div>");
            }
            else
            {
                var componentName = p.RazorInputComponent;
                var colClass = p.IsString ? "col-md-12" : "col-md-6";
                sb.AppendLine(CultureInfo.InvariantCulture, $"                    <div class=\"{colClass}\">");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                        <label for=\"{p.CamelName}\" class=\"form-label\">{label}</label>");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                        <{componentName} id=\"{p.CamelName}\" class=\"form-control\" @bind-Value=\"Model.{p.Name}\" />");
                sb.AppendLine(CultureInfo.InvariantCulture, $"                        <ValidationMessage For=\"() => Model.{p.Name}\" class=\"text-danger small\" />");
                sb.Append("                    </div>");
            }

            if (i < entity.Properties.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildTableHead(EntitySpec entity)
    {
        if (entity.Properties.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"                    <th scope=\"col\">{entity.Properties[i].Name}</th>");
            if (i < entity.Properties.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildTableBody(EntitySpec entity)
    {
        if (entity.Properties.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        for (var i = 0; i < entity.Properties.Count; i++)
        {
            var p = entity.Properties[i];
            var formatted = p.IsDateTime
                ? $"@item.{p.Name}.ToString(\"yyyy-MM-dd\")"
                : $"@item.{p.Name}";
            sb.Append(CultureInfo.InvariantCulture, $"                        <td>{formatted}</td>");
            if (i < entity.Properties.Count - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static IEnumerable<PropertySpec> FilterableProperties(EntitySpec entity) =>
        entity.Properties.Where(p => p.IsString);

    private static string BuildFilterFields(EntitySpec entity)
    {
        var filterable = FilterableProperties(entity).ToArray();
        if (filterable.Length == 0)
        {
            return "            <div class=\"col-12\"><p class=\"text-muted small mb-0\">No hay campos filtrables.</p></div>";
        }

        var sb = new StringBuilder();
        for (var i = 0; i < filterable.Length; i++)
        {
            var p = filterable[i];
            sb.AppendLine(CultureInfo.InvariantCulture, $"            <div class=\"col-md-4\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                <label for=\"filter_{p.CamelName}\" class=\"form-label small text-muted\">{p.Name}</label>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"                <input id=\"filter_{p.CamelName}\" type=\"text\" class=\"form-control form-control-sm\" @bind=\"filter_{p.CamelName}\" @bind:event=\"oninput\" placeholder=\"Buscar por {p.Name.ToLowerInvariant()}...\" />");
            sb.Append("            </div>");
            if (i < filterable.Length - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildFilterState(EntitySpec entity)
    {
        var filterable = FilterableProperties(entity).ToArray();
        if (filterable.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < filterable.Length; i++)
        {
            var p = filterable[i];
            sb.Append(CultureInfo.InvariantCulture, $"    private string filter_{p.CamelName} = string.Empty;");
            if (i < filterable.Length - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildFilterLogic(EntitySpec entity)
    {
        var filterable = FilterableProperties(entity).ToArray();
        if (filterable.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < filterable.Length; i++)
        {
            var p = filterable[i];
            sb.AppendLine(CultureInfo.InvariantCulture, $"        if (!string.IsNullOrWhiteSpace(filter_{p.CamelName}))");
            sb.Append(CultureInfo.InvariantCulture, $"            q = q.Where(x => x.{p.Name} != null && x.{p.Name}.Contains(filter_{p.CamelName}, StringComparison.OrdinalIgnoreCase));");
            if (i < filterable.Length - 1) sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string BuildResetLogic(EntitySpec entity)
    {
        var filterable = FilterableProperties(entity).ToArray();
        if (filterable.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < filterable.Length; i++)
        {
            var p = filterable[i];
            sb.Append(CultureInfo.InvariantCulture, $"        filter_{p.CamelName} = string.Empty;");
            if (i < filterable.Length - 1) sb.AppendLine();
        }
        return sb.ToString();
    }
}
