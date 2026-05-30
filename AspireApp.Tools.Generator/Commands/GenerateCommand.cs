using System.ComponentModel;
using System.Globalization;
using AspireApp.Tools.Generator.Generator;
using AspireApp.Tools.Generator.Generator.Mutators;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AspireApp.Tools.Generator.Commands;

internal sealed class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[ENTITY_NAME]")]
        [Description("Singular PascalCase entity name (e.g. Order).")]
        public string? EntityName { get; init; }

        [CommandOption("--id <ID_TYPE>")]
        [Description("Id type: long, int or Guid. Defaults to long.")]
        public string? IdType { get; init; }

        [CommandOption("-p|--prop <PROPERTY>")]
        [Description("Property in the form 'Name:type[:flag1[:flag2...]]'. Flags: required, filter|nofilter, hidden|list, sort|nosort. Repeatable.")]
        public string[]? Properties { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Override the Bootstrap Icon class (without the bi- prefix). Auto-detected by default.")]
        public string? Icon { get; init; }

        [CommandOption("--accent <ACCENT>")]
        [Description("Override the Bootstrap accent (primary, success, info, warning, danger, secondary). Deterministic by default.")]
        public string? Accent { get; init; }

        [CommandOption("--filter-mode <MODE>")]
        [Description("Where filtering/sorting/paging happens: 'client' (default) loads everything and filters in browser. 'server' sends a filter DTO to the API, with paging.")]
        public string? FilterMode { get; init; }

        [CommandOption("--page-size <N>")]
        [Description("Default page size for server-mode pagination (ignored for client mode). Defaults to 25.")]
        public int? PageSize { get; init; }

        [CommandOption("--no-ui")]
        [Description("Do not generate a Blazor page.")]
        public bool NoUi { get; init; }

        [CommandOption("--no-nav")]
        [Description("Do not register a NavLink in NavMenu.razor.")]
        public bool NoNav { get; init; }

        [CommandOption("--no-auth")]
        [Description("Do not decorate the controller with the Authorize attribute.")]
        public bool NoAuth { get; init; }

        [CommandOption("--event-bus")]
        [Description("Publish an event to the message bus when a new entity is created. In interactive mode you are asked.")]
        public bool EventBus { get; init; }

        [CommandOption("--dry-run")]
        [Description("Plan and preview without writing anything to disk.")]
        public bool DryRun { get; init; }

        [CommandOption("-y|--yes")]
        [Description("Run non-interactively. Skip confirmation prompts.")]
        public bool Yes { get; init; }

        [CommandOption("--root <PATH>")]
        [Description("Optional explicit path to the solution root.")]
        public string? Root { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        RenderBanner();

        var paths = settings.Root is { Length: > 0 }
            ? new PathResolver(settings.Root)
            : PathResolver.Discover();

        RenderContextBar(paths, settings);

        EntitySpec entity;
        try
        {
            entity = BuildEntitySpec(settings, paths);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        RenderEntitySummary(entity);

        var plan = GenerationPlan.Build(entity, paths);

        RenderPlan(plan, paths);

        if (!settings.Yes && !settings.DryRun && !AnsiConsole.Confirm("[bold mediumpurple1]❯[/] [bold]¿Proceder con la generación?[/]", true))
        {
            AnsiConsole.MarkupLine("[grey]✗ Cancelado por el usuario.[/]");
            return 0;
        }

        var renderer = new TemplateRenderer();
        var totals = await ExecutePlanAsync(plan, entity, paths, renderer, settings, cancellationToken);

        RenderResult(totals, settings);

        return 0;
    }

    private static void RenderBanner()
    {
        AnsiConsole.WriteLine();
        var fig = new FigletText("AspireApp").Color(Color.MediumPurple1);
        AnsiConsole.Write(new Padder(fig).Padding(1, 0, 1, 0));

        var tagline = new Markup(
            "  [grey]✦[/] [bold mediumpurple1]Generador de CRUD[/] [grey]·[/] [grey]capas[/] " +
            "[magenta]Domain[/] [grey]›[/] [blue]Application[/] [grey]›[/] [violet]Infra[/] [grey]›[/] " +
            "[gold1]Api[/] [grey]›[/] [aqua]Client[/]  [grey]✦[/]");
        AnsiConsole.Write(tagline);
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
    }

    private static void RenderContextBar(PathResolver paths, Settings settings)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[grey]▸ Root[/]", $"[aqua]{paths.SolutionRoot.EscapeMarkup()}[/]");
        if (settings.DryRun)
            grid.AddRow("[grey]▸ Modo[/]", "[bold yellow on grey15]  DRY-RUN  [/] [grey]no se escribe nada en disco[/]");
        else
            grid.AddRow("[grey]▸ Modo[/]", "[bold green]✔ APPLY[/] [grey]se escribirán archivos en disco[/]");
        if (settings.Yes)
            grid.AddRow("[grey]▸ Prompts[/]", "[grey]desactivados (--yes)[/]");

        AnsiConsole.Write(new Padder(grid).Padding(2, 0, 0, 1));
    }

    private static async Task<Totals> ExecutePlanAsync(
        GenerationPlan plan,
        EntitySpec entity,
        PathResolver paths,
        TemplateRenderer renderer,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var totals = new Totals();

        // Token values are constant for the entity, so build them once and reuse for every file.
        var tokens = TemplateRenderer.BuildTokens(entity);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold mediumpurple1]✦ Generando[/]").RuleStyle("grey39").LeftJustified());
        AnsiConsole.WriteLine();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("mediumpurple1"))
            .StartAsync("Preparando...", async ctx =>
            {
                foreach (var creation in plan.Creations)
                {
                    var layer = LayerInfo.FromLabel(creation.FriendlyLabel);
                    ctx.Status($"[{layer.Color}]{layer.Icon} {layer.Name}[/] [grey]·[/] {creation.FriendlyLabel.EscapeMarkup()}");
                    ctx.Refresh();

                    if (File.Exists(creation.TargetPath))
                    {
                        AnsiConsole.MarkupLine($"  [yellow]○[/] [{layer.Color}]{layer.Icon} {layer.Tag}[/] [grey]{Rel(creation.TargetPath, paths)}[/]  [yellow](existe, omitido)[/]");
                        totals.Skipped++;
                        continue;
                    }

                    var rendered = renderer.Render(creation.TemplateName, tokens);

                    if (settings.DryRun)
                    {
                        AnsiConsole.MarkupLine($"  [aqua]✦[/] [{layer.Color}]{layer.Icon} {layer.Tag}[/] {Rel(creation.TargetPath, paths)}  [grey](dry-run)[/]");
                        totals.Created++;
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(creation.TargetPath)!);
                    await File.WriteAllTextAsync(creation.TargetPath, rendered, cancellationToken);
                    AnsiConsole.MarkupLine($"  [green]✚[/] [{layer.Color}]{layer.Icon} {layer.Tag}[/] {Rel(creation.TargetPath, paths)}");
                    totals.Created++;
                }

                ctx.Status("[orange3]✎ Actualizando archivos compartidos...[/]");
                ctx.Spinner(Spinner.Known.Aesthetic);
                ctx.Refresh();

                foreach (var mutator in plan.Mutators)
                {
                    if (!File.Exists(mutator.TargetPath))
                    {
                        AnsiConsole.MarkupLine($"  [red]✗[/] [red]{Rel(mutator.TargetPath, paths)} no encontrado[/]");
                        totals.Failed++;
                        continue;
                    }

                    var source = await File.ReadAllTextAsync(mutator.TargetPath, cancellationToken);
                    MutationResult result;
                    try
                    {
                        result = mutator.Mutate(source, entity);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"  [red]✗[/] [red]{Rel(mutator.TargetPath, paths)}: {ex.Message.EscapeMarkup()}[/]");
                        totals.Failed++;
                        continue;
                    }

                    if (!result.Changed)
                    {
                        AnsiConsole.MarkupLine($"  [grey]·[/] [orange3]✎ Shared[/] [grey]{Rel(mutator.TargetPath, paths)}  ({result.Description.EscapeMarkup()})[/]");
                        continue;
                    }

                    if (!settings.DryRun)
                        await File.WriteAllTextAsync(mutator.TargetPath, result.NewContent, cancellationToken);

                    var marker = settings.DryRun ? "  [grey](dry-run)[/]" : string.Empty;
                    AnsiConsole.MarkupLine($"  [green]✎[/] [orange3]✎ Shared[/] {Rel(mutator.TargetPath, paths)}  [grey]({result.Description.EscapeMarkup()})[/]{marker}");
                    totals.Mutated++;
                }
            });

        return totals;
    }

    private static void RenderResult(Totals totals, Settings settings)
    {
        AnsiConsole.WriteLine();

        var hasErrors = totals.Failed > 0;
        var statusColor = hasErrors ? "red" : "green";
        var statusText = hasErrors ? "Completado con errores" : "Listo";
        var statusIcon = hasErrors ? "✗" : "✔";

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap());

        grid.AddRow($"[bold {statusColor}]{statusIcon}[/]", $"[bold {statusColor}]{statusText}[/]");
        grid.AddRow("[grey]✚ Creados[/]", $"[green]{totals.Created}[/]");
        grid.AddRow("[grey]✎ Actualizados[/]", $"[aqua]{totals.Mutated}[/]");
        grid.AddRow("[grey]○ Omitidos[/]", $"[yellow]{totals.Skipped}[/]");
        if (totals.Failed > 0)
            grid.AddRow("[grey]✗ Fallidos[/]", $"[red]{totals.Failed}[/]");

        var panel = new Panel(grid)
        {
            Header = new PanelHeader("[bold] ▣ Resumen [/]"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(hasErrors ? Color.Red : Color.Green),
            Padding = new Padding(2, 1, 2, 1),
        };
        AnsiConsole.Write(panel);

        if (settings.DryRun)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]→ Dry-run: nada fue escrito en disco. Re-corré sin [/][bold yellow]--dry-run[/][yellow] para aplicar.[/]");
            return;
        }

        if (totals.Failed > 0)
            return;

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold mediumpurple1]❯❯ Próximos pasos[/]").RuleStyle("grey39").LeftJustified());

        var next = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(2)).AddColumn();
        next.AddRow("[green]❯[/] [bold green]1[/]", "[white]dotnet build[/] [grey]— compilar y validar[/]");
        next.AddRow("[green]❯[/] [bold green]2[/]", "[white]dotnet ef migrations add Add<Entity>[/] [grey]— si necesitás migración de DB[/]");
        next.AddRow("[green]❯[/] [bold green]3[/]", "[white]dotnet run --project AspireApp.AppHost[/] [grey]— levantar la app[/]");
        AnsiConsole.Write(new Padder(next).Padding(2, 1, 0, 1));
    }

    // ----------------------------------------------------------------------------------
    // Spec building — one resolver per decision so the flow reads top-to-bottom.
    // ----------------------------------------------------------------------------------

    private static EntitySpec BuildEntitySpec(Settings settings, PathResolver paths)
    {
        var interactive = !settings.Yes;

        // Resolve flag-only values first so a bad --icon/--accent fails fast, before any prompt.
        var icon = ResolveIcon(settings);
        var accent = ResolveAccent(settings);

        var name = ResolveEntityName(settings, paths, interactive);
        var idType = ResolveIdType(settings, interactive);
        var properties = ResolveProperties(settings, interactive);

        if (properties.Count == 0)
            AnsiConsole.MarkupLine("[yellow]→ Sin propiedades. Se generará con el body vacío (podés agregarlas después).[/]");

        var generateBlazor = ResolveBlazor(settings, interactive);
        var registerNav = ResolveNav(settings, interactive, generateBlazor);
        var requireAuth = !settings.NoAuth;
        var useEventBus = ResolveEventBus(settings, interactive);
        var filterMode = ResolveFilterMode(settings, interactive, properties);
        var pageSize = ResolvePageSize(settings, interactive, filterMode);

        return new EntitySpec(
            name,
            idType,
            properties,
            generateBlazor,
            registerNav,
            requireAuth,
            useEventBus,
            filterMode,
            pageSize,
            icon,
            accent);
    }

    private static string ResolveEntityName(Settings settings, PathResolver paths, bool interactive)
    {
        var name = settings.EntityName?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            if (!interactive)
                throw new ArgumentException("Falta el nombre de la entidad. Pasalo como argumento (ej: 'generate Order') o no uses --yes.");

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold mediumpurple1]✦ Definición de la entidad[/]").RuleStyle("grey39").LeftJustified());
            AnsiConsole.WriteLine();

            name = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold mediumpurple1]❯[/] Nombre de la entidad [grey](PascalCase, singular)[/]")
                    .PromptStyle("aqua")
                    .Validate(static raw => Naming.IsValidIdentifier(raw?.Trim())
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Sólo letras, dígitos o '_', empezando con letra o '_'")));
        }
        else if (!Naming.IsValidIdentifier(name))
        {
            throw new ArgumentException($"Nombre de entidad inválido '{name}'. Usá sólo letras, dígitos o '_', empezando con letra o '_'.");
        }

        name = Naming.Capitalize(name.Trim());

        // Heads-up if the slice already exists. Existing files are skipped and shared files are
        // not duplicated, so this is informational — but better to know before generating.
        if (interactive && File.Exists(paths.DomainEntity(name)))
            AnsiConsole.MarkupLine($"  [yellow]○[/] [grey]Ya existe[/] [white]{name.EscapeMarkup()}[/][grey]: los archivos existentes se omiten y los compartidos no se duplican.[/]");

        return name;
    }

    private static string ResolveIdType(Settings settings, bool interactive)
    {
        var idType = settings.IdType?.Trim();
        if (string.IsNullOrWhiteSpace(idType))
        {
            idType = interactive
                ? AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("[bold mediumpurple1]❯[/] Tipo del Id")
                    .HighlightStyle(Style.Parse("aqua"))
                    .AddChoices("long", "int", "Guid"))
                : "long";
        }

        return idType.ToLowerInvariant() switch
        {
            "long" or "int64" => "long",
            "int" or "int32" => "int",
            "guid" => "Guid",
            _ => throw new ArgumentException($"Tipo de Id no soportado '{idType}'. Usá long, int o Guid.")
        };
    }

    private static List<PropertySpec> ResolveProperties(Settings settings, bool interactive)
    {
        if (settings.Properties is { Length: > 0 })
            return [.. settings.Properties.Select(PropertySpec.Parse)];

        return interactive ? CollectPropertiesInteractive() : [];
    }

    private static bool ResolveBlazor(Settings settings, bool interactive)
    {
        if (settings.NoUi) return false;
        if (!interactive) return true;
        return AnsiConsole.Confirm("[bold mediumpurple1]❯[/] ¿Generar [bold]pantallas Blazor[/] [grey](Index + Edit)[/]?", true);
    }

    private static bool ResolveNav(Settings settings, bool interactive, bool generateBlazor)
    {
        if (settings.NoNav || !generateBlazor) return false;
        if (!interactive) return true;
        return AnsiConsole.Confirm("[bold mediumpurple1]❯[/] ¿Agregar un [bold]NavLink[/] en NavMenu.razor?", true);
    }

    private static bool ResolveEventBus(Settings settings, bool interactive)
    {
        if (settings.EventBus) return true;
        if (!interactive) return false;
        return AnsiConsole.Confirm("[bold mediumpurple1]❯[/] ¿Publicar un evento al [bold]event bus[/] cuando se cree una nueva instancia?", false);
    }

    private static string? ResolveIcon(Settings settings)
    {
        var icon = settings.Icon?.Trim();
        if (string.IsNullOrEmpty(icon)) return null;
        if (icon.StartsWith("bi-", StringComparison.OrdinalIgnoreCase)) icon = icon[3..];
        return string.IsNullOrEmpty(icon) ? null : icon;
    }

    private static string? ResolveAccent(Settings settings)
    {
        var accent = settings.Accent?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(accent)) return null;
        if (accent is not ("primary" or "success" or "info" or "warning" or "danger" or "secondary"))
            throw new ArgumentException($"Acento no soportado '{accent}'. Usá primary, success, info, warning, danger o secondary.");
        return accent;
    }

    // ----------------------------------------------------------------------------------
    // Interactive property editor — add / edit / remove with a live table, so a wrong
    // answer (required, filter, …) is fixed by editing the row, not by restarting.
    // ----------------------------------------------------------------------------------

    private static readonly string[] PropertyTypes =
        ["string", "int", "long", "decimal", "double", "bool", "DateTime", "Guid"];

    private const string FlagRequired = "Requerido";
    private const string FlagFilter = "Filtrable en el Index";
    private const string FlagList = "Mostrar en la tabla";
    private const string FlagSort = "Columna ordenable";

    private enum PropertyAction { Add, Edit, Remove, Done }

    private static List<PropertySpec> CollectPropertiesInteractive()
    {
        var properties = new List<PropertySpec>();

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold mediumpurple1]✎ Propiedades[/]").RuleStyle("grey39").LeftJustified());
        AnsiConsole.MarkupLine("[grey]Agregá, editá o quitá campos. La tabla refleja el estado actual.[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            RenderPropertiesTable(properties);

            var action = AnsiConsole.Prompt(new SelectionPrompt<PropertyAction>()
                .Title("[grey]¿Qué querés hacer?[/]")
                .HighlightStyle(Style.Parse("aqua"))
                .UseConverter(a => a switch
                {
                    PropertyAction.Add => "✚  Agregar propiedad",
                    PropertyAction.Edit => "✎  Editar una propiedad",
                    PropertyAction.Remove => "−  Quitar una propiedad",
                    _ => "❯  Listo, continuar",
                })
                .AddChoices(BuildActionChoices(properties.Count)));

            switch (action)
            {
                case PropertyAction.Add:
                    properties.Add(PromptForProperty(existing: null, siblings: properties));
                    break;

                case PropertyAction.Edit:
                    var toEdit = ChooseProperty(properties, "editar");
                    if (toEdit is not null)
                    {
                        var idx = properties.IndexOf(toEdit);
                        properties[idx] = PromptForProperty(existing: toEdit, siblings: properties);
                    }
                    break;

                case PropertyAction.Remove:
                    var toRemove = ChooseProperty(properties, "quitar");
                    if (toRemove is not null)
                        properties.Remove(toRemove);
                    break;

                default:
                    return properties;
            }
        }
    }

    private static PropertyAction[] BuildActionChoices(int count) =>
        count > 0
            ? [PropertyAction.Add, PropertyAction.Edit, PropertyAction.Remove, PropertyAction.Done]
            : [PropertyAction.Add, PropertyAction.Done];

    private static PropertySpec? ChooseProperty(IReadOnlyList<PropertySpec> properties, string verb)
    {
        // -1 is the "go back" sentinel.
        var choices = Enumerable.Range(0, properties.Count).Append(-1);

        var idx = AnsiConsole.Prompt(new SelectionPrompt<int>()
            .Title($"[grey]¿Cuál querés {verb}?[/]")
            .HighlightStyle(Style.Parse("aqua"))
            .UseConverter(i => i < 0
                ? "↩  Volver"
                : $"{i + 1}. {properties[i].Name} ({properties[i].Type})")
            .AddChoices(choices));

        return idx < 0 ? null : properties[idx];
    }

    private static PropertySpec PromptForProperty(PropertySpec? existing, IReadOnlyList<PropertySpec> siblings)
    {
        var isEdit = existing is not null;

        var namePrompt = new TextPrompt<string>(isEdit
                ? "  [grey]·[/] Nombre [grey](Enter mantiene el actual)[/]"
                : "  [grey]·[/] Nombre de la propiedad")
            .PromptStyle("white")
            .Validate(candidate => ValidatePropertyName(candidate, existing, siblings));

        if (isEdit)
            namePrompt.DefaultValue(existing!.Name).ShowDefaultValue();

        var name = Naming.Capitalize(AnsiConsole.Prompt(namePrompt).Trim());

        var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"    [grey]Tipo para[/] [yellow]{name.EscapeMarkup()}[/]")
            .HighlightStyle(Style.Parse("aqua"))
            .AddChoices(TypeChoices(existing?.Type)));

        var flags = BuildFlagPrompt(name, existing, type);
        var selected = AnsiConsole.Prompt(flags);

        var showInList = selected.Contains(FlagList);
        return new PropertySpec(
            Name: name,
            Type: type,
            Required: selected.Contains(FlagRequired),
            Filterable: selected.Contains(FlagFilter),
            ShowInList: showInList,
            Sortable: showInList && selected.Contains(FlagSort));
    }

    /// <summary>Type list with the current type (when editing) floated to the top so the cursor starts on it.</summary>
    private static IEnumerable<string> TypeChoices(string? currentType)
    {
        if (string.IsNullOrEmpty(currentType))
            return PropertyTypes;

        return PropertyTypes.Contains(currentType)
            ? PropertyTypes.OrderByDescending(t => t == currentType)
            : PropertyTypes.Prepend(currentType);
    }

    private static MultiSelectionPrompt<string> BuildFlagPrompt(string name, PropertySpec? existing, string type)
    {
        var prompt = new MultiSelectionPrompt<string>()
            .Title($"    [grey]Opciones para[/] [yellow]{name.EscapeMarkup()}[/]")
            .NotRequired()
            .HighlightStyle(Style.Parse("aqua"))
            .InstructionsText("[grey](espacio = marcar · Enter = confirmar)[/]")
            .AddChoices(FlagRequired, FlagFilter, FlagList, FlagSort);

        // Pre-select sensible defaults for new props, or the current values when editing.
        var required = existing?.Required ?? false;
        var filterable = existing?.Filterable ?? PropertySpec.DefaultFilterableFor(type);
        var showInList = existing?.ShowInList ?? true;
        var sortable = existing is null || (existing.ShowInList && existing.Sortable);

        if (required) prompt.Select(FlagRequired);
        if (filterable) prompt.Select(FlagFilter);
        if (showInList) prompt.Select(FlagList);
        if (sortable) prompt.Select(FlagSort);

        return prompt;
    }

    private static ValidationResult ValidatePropertyName(string raw, PropertySpec? existing, IReadOnlyList<PropertySpec> siblings)
    {
        var name = raw?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return ValidationResult.Error("El nombre es obligatorio");

        if (!Naming.IsValidIdentifier(name))
            return ValidationResult.Error("Sólo letras, dígitos o '_', empezando con letra o '_'");

        var capitalized = Naming.Capitalize(name);
        var duplicate = siblings.Any(p =>
            !ReferenceEquals(p, existing) &&
            string.Equals(p.Name, capitalized, StringComparison.OrdinalIgnoreCase));

        return duplicate
            ? ValidationResult.Error($"Ya existe una propiedad '{capitalized}'")
            : ValidationResult.Success();
    }

    // ----------------------------------------------------------------------------------
    // Filter mode / page size
    // ----------------------------------------------------------------------------------

    private static FilterMode ResolveFilterMode(Settings settings, bool interactive, IReadOnlyList<PropertySpec> properties)
    {
        var raw = settings.FilterMode?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(raw))
        {
            return raw switch
            {
                "client" or "cli" => FilterMode.Client,
                "server" or "srv" or "api" => FilterMode.Server,
                _ => throw new ArgumentException($"Modo de filtrado no soportado '{settings.FilterMode}'. Usá 'client' o 'server'."),
            };
        }

        if (!interactive)
            return FilterMode.Client;

        var hasFilterable = properties.Any(p => p.Filterable);
        var description = hasFilterable
            ? "[grey](algunos campos son filtrables)[/]"
            : "[grey](sin campos filtrables; igual podés elegir server para tener paginación)[/]";

        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"[bold mediumpurple1]❯[/] Modo de filtrado/paginación del Index {description}")
            .HighlightStyle(Style.Parse("aqua"))
            .AddChoices(
                "client  (carga todo y filtra en el navegador)",
                "server  (manda filtro a la API + paginación)"));

        return choice.StartsWith("server", StringComparison.Ordinal) ? FilterMode.Server : FilterMode.Client;
    }

    private static int ResolvePageSize(Settings settings, bool interactive, FilterMode mode)
    {
        if (settings.PageSize is > 0)
            return settings.PageSize.Value;

        if (mode == FilterMode.Client || !interactive)
            return 25;

        return AnsiConsole.Prompt(new TextPrompt<int>("[bold mediumpurple1]❯[/] Tamaño de página por defecto")
            .DefaultValue(25)
            .ShowDefaultValue()
            .Validate(static n => n is > 0 and <= 500
                ? ValidationResult.Success()
                : ValidationResult.Error("Debe ser entre 1 y 500")));
    }

    // ----------------------------------------------------------------------------------
    // Preview rendering
    // ----------------------------------------------------------------------------------

    private static void RenderEntitySummary(EntitySpec entity)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold mediumpurple1]◆ Entity preview[/]").RuleStyle("grey39").LeftJustified());
        AnsiConsole.WriteLine();

        var accentColor = MapAccentToSpectre(entity.Accent);

        var headerGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        headerGrid.AddRow(
            $"[bold {accentColor} on grey15]   bi-{entity.Icon.EscapeMarkup()}   [/]",
            $"[bold white]{entity.Name.EscapeMarkup()}[/]  [grey]·[/] plural [aqua]{entity.Plural.EscapeMarkup()}[/]");
        headerGrid.AddRow("[grey]# Id type[/]", $"[white]{entity.IdType}[/]");
        headerGrid.AddRow("[grey]● Acento[/]", $"[{accentColor}]●[/] [white]{entity.Accent}[/]");
        headerGrid.AddRow("[grey]▣ Blazor UI[/]", entity.GenerateBlazorPage ? "[green]✔ sí[/]" : "[grey]✘ no[/]");
        headerGrid.AddRow("[grey]▸ NavMenu[/]", entity.GenerateBlazorPage && entity.RegisterInNavMenu ? "[green]✔ sí[/]" : "[grey]✘ no[/]");
        headerGrid.AddRow("[grey]★ Authorize[/]", entity.RequireAuth ? "[green]✔ sí[/]" : "[grey]✘ no[/]");
        headerGrid.AddRow("[grey]✉ Event bus[/]", entity.UseEventBus ? "[green]✔ sí (publica al crear)[/]" : "[grey]✘ no[/]");
        headerGrid.AddRow("[grey]⛃ Filtrado[/]", entity.IsServerFiltering
            ? $"[aqua]server[/] [grey](pageSize {entity.PageSize})[/]"
            : "[white]client[/] [grey](carga todo, filtra en browser)[/]");

        AnsiConsole.Write(new Panel(headerGrid)
        {
            Header = new PanelHeader($"[bold] {entity.Name.EscapeMarkup()} [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.MediumPurple1),
            Padding = new Padding(2, 1, 2, 1),
        });

        if (entity.Properties.Count > 0)
            AnsiConsole.Write(BuildPropertiesTable(entity.Properties, "[bold]✎ Propiedades[/]"));
        else
            AnsiConsole.MarkupLine("[grey](sin propiedades)[/]");

        AnsiConsole.WriteLine();
    }

    private static void RenderPropertiesTable(IReadOnlyList<PropertySpec> properties)
    {
        if (properties.Count == 0)
        {
            AnsiConsole.MarkupLine("  [grey]· Todavía no agregaste propiedades.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        AnsiConsole.Write(BuildPropertiesTable(properties, title: null));
        AnsiConsole.WriteLine();
    }

    /// <summary>Single source of truth for the property table, used by the live editor and the final preview.</summary>
    private static Table BuildPropertiesTable(IReadOnlyList<PropertySpec> properties, string? title)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey39)
            .AddColumn("[grey]#[/]")
            .AddColumn("[bold]Nombre[/]")
            .AddColumn("[bold]Tipo[/]")
            .AddColumn("[bold]Req[/]")
            .AddColumn("[bold]Filtra[/]")
            .AddColumn("[bold]Lista[/]")
            .AddColumn("[bold]Ordena[/]");

        if (title is not null)
            table.Title(title);

        for (var i = 0; i < properties.Count; i++)
        {
            var p = properties[i];
            table.AddRow(
                $"[grey]{i + 1}[/]",
                $"[white]{p.Name.EscapeMarkup()}[/]",
                $"[aqua]{p.Type.EscapeMarkup()}[/]",
                Check(p.Required),
                Check(p.Filterable),
                Check(p.ShowInList),
                Check(p.ShowInList && p.Sortable));
        }

        return table;
    }

    private static string Check(bool on) => on ? "[green]✔[/]" : "[grey]·[/]";

    private static string MapAccentToSpectre(string accent) => accent switch
    {
        "primary" => "blue",
        "success" => "green",
        "info" => "aqua",
        "warning" => "yellow",
        "danger" => "red",
        "secondary" => "grey",
        _ => "white",
    };

    private static void RenderPlan(GenerationPlan plan, PathResolver paths)
    {
        AnsiConsole.Write(new Rule("[bold mediumpurple1]◈ Plan de generación[/]").RuleStyle("grey39").LeftJustified());
        AnsiConsole.WriteLine();

        var tree = new Tree(
            $"[bold]▸ Archivos[/] [grey]·[/] [green]✚ {plan.Creations.Count}[/] [grey]a crear[/], " +
            $"[aqua]✎ {plan.Mutators.Count}[/] [grey]a actualizar[/]")
        {
            Style = new Style(Color.Grey50),
        };

        var byLayer = plan.Creations
            .GroupBy(c => LayerInfo.FromLabel(c.FriendlyLabel))
            .OrderBy(g => g.Key.Order);

        foreach (var group in byLayer)
        {
            var layerNode = tree.AddNode($"[{group.Key.Color}]{group.Key.Icon}[/] [bold {group.Key.Color}]{group.Key.Name}[/] [grey]({group.Count()})[/]");
            foreach (var c in group)
                layerNode.AddNode($"[green]✚[/] [white]{Path.GetFileName(c.TargetPath).EscapeMarkup()}[/]  [grey]{Rel(Path.GetDirectoryName(c.TargetPath)!, paths)}[/]");
        }

        if (plan.Mutators.Count > 0)
        {
            var sharedNode = tree.AddNode($"[orange3]✎[/] [bold orange3]Shared[/] [grey]({plan.Mutators.Count})[/]");
            foreach (var m in plan.Mutators)
                sharedNode.AddNode($"[aqua]✎[/] [white]{Path.GetFileName(m.TargetPath).EscapeMarkup()}[/]  [grey]{Rel(Path.GetDirectoryName(m.TargetPath)!, paths)}[/]");
        }

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private static string Rel(string fullPath, PathResolver paths) =>
        Path.GetRelativePath(paths.SolutionRoot, fullPath).EscapeMarkup();

    private sealed class Totals
    {
        public int Created { get; set; }
        public int Mutated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
    }

    private sealed record LayerInfo(string Name, string Color, int Order, string Icon, string Tag)
    {
        public static LayerInfo FromLabel(string friendlyLabel)
        {
            var prefix = friendlyLabel.Split('.', 2)[0];
            return prefix switch
            {
                "Domain"      => new("Domain",         "magenta", 1, "◆", "Domain "),
                "Application" => new("Application",    "blue",    2, "▼", "App    "),
                "DataAccess"  => new("Infrastructure", "violet",  3, "▣", "Infra  "),
                "Api"         => new("Api",            "gold1",   4, "▲", "Api    "),
                "Client"      => new("Client",         "aqua",    5, "✦", "Client "),
                _             => new(prefix,           "white",   99, "•", prefix),
            };
        }
    }
}
