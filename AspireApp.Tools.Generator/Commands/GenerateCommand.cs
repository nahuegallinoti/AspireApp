using System.ComponentModel;
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
        [Description("Property in the form Name:type or Name:type:required. Repeatable.")]
        public string[]? Properties { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Override the Bootstrap Icon class (without the bi- prefix). Auto-detected by default.")]
        public string? Icon { get; init; }

        [CommandOption("--accent <ACCENT>")]
        [Description("Override the Bootstrap accent (primary, success, info, warning, danger, secondary). Deterministic by default.")]
        public string? Accent { get; init; }

        [CommandOption("--no-ui")]
        [Description("Do not generate a Blazor page.")]
        public bool NoUi { get; init; }

        [CommandOption("--no-nav")]
        [Description("Do not register a NavLink in NavMenu.razor.")]
        public bool NoNav { get; init; }

        [CommandOption("--no-auth")]
        [Description("Do not decorate the controller with the Authorize attribute.")]
        public bool NoAuth { get; init; }

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
            entity = BuildEntitySpec(settings);
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

                    var rendered = renderer.Render(creation.TemplateName, entity);

                    if (File.Exists(creation.TargetPath))
                    {
                        AnsiConsole.MarkupLine($"  [yellow]○[/] [{layer.Color}]{layer.Icon} {layer.Tag}[/] [grey]{Rel(creation.TargetPath, paths)}[/]  [yellow](existe, omitido)[/]");
                        totals.Skipped++;
                        continue;
                    }

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

    private static EntitySpec BuildEntitySpec(Settings settings)
    {
        var name = settings.EntityName;
        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold mediumpurple1]✦ Definición de la entidad[/]").RuleStyle("grey39").LeftJustified());
            AnsiConsole.WriteLine();
            name = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold mediumpurple1]❯[/] Nombre de la entidad [grey](PascalCase, singular)[/]")
                    .PromptStyle("aqua")
                    .Validate(static s => string.IsNullOrWhiteSpace(s)
                        ? ValidationResult.Error("Requerido")
                        : char.IsLower(s[0])
                            ? ValidationResult.Error("Debe empezar con mayúscula")
                            : ValidationResult.Success()));
        }

        name = name.Trim();
        if (char.IsLower(name[0]))
            throw new ArgumentException("Entity name must be PascalCase (start with uppercase).");

        var idType = settings.IdType?.Trim();
        if (string.IsNullOrWhiteSpace(idType))
        {
            idType = settings.Yes
                ? "long"
                : AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("[bold mediumpurple1]❯[/] Tipo del Id")
                    .HighlightStyle(Style.Parse("aqua"))
                    .AddChoices("long", "int", "Guid"));
        }

        idType = idType.ToLowerInvariant() switch
        {
            "long" or "int64" => "long",
            "int" or "int32" => "int",
            "guid" => "Guid",
            _ => throw new ArgumentException($"Unsupported Id type '{idType}'. Use long, int or Guid.")
        };

        var properties = new List<PropertySpec>();

        if (settings.Properties is { Length: > 0 })
        {
            foreach (var raw in settings.Properties)
                properties.Add(PropertySpec.Parse(raw));
        }
        else if (!settings.Yes)
        {
            AnsiConsole.MarkupLine("[grey]✎ Agregá propiedades (nombre vacío para terminar).[/]");
            while (true)
            {
                var propName = AnsiConsole.Prompt(
                    new TextPrompt<string>("  [grey]·[/] Nombre de la propiedad [grey](vacío = terminar)[/]")
                        .PromptStyle("white")
                        .AllowEmpty());

                if (string.IsNullOrWhiteSpace(propName)) break;

                var propType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"    [grey]Tipo para[/] [yellow]{propName}[/]")
                        .HighlightStyle(Style.Parse("aqua"))
                        .AddChoices("string", "int", "long", "decimal", "double", "bool", "DateTime", "Guid"));

                var required = AnsiConsole.Confirm($"    [grey]¿Es[/] [yellow]{propName}[/] [grey]requerido?[/]", defaultValue: true);
                properties.Add(new PropertySpec(Capitalize(propName), propType, required));
            }
        }

        if (properties.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]→ No se especificaron propiedades. Se generará con el body vacío (podés agregarlas después).[/]");
        }

        var interactive = !settings.Yes;

        bool generateBlazor;
        if (settings.NoUi)
            generateBlazor = false;
        else if (interactive)
            generateBlazor = AnsiConsole.Confirm(
                "[bold mediumpurple1]❯[/] ¿Generar [bold]pantallas Blazor[/] [grey](Index + Edit)[/]?",
                defaultValue: true);
        else
            generateBlazor = true;

        bool registerNav;
        if (settings.NoNav || !generateBlazor)
            registerNav = false;
        else if (interactive)
            registerNav = AnsiConsole.Confirm(
                "[bold mediumpurple1]❯[/] ¿Agregar un [bold]NavLink[/] en NavMenu.razor?",
                defaultValue: true);
        else
            registerNav = true;

        var requireAuth = !settings.NoAuth;

        var icon = settings.Icon?.Trim();
        if (!string.IsNullOrEmpty(icon) && icon.StartsWith("bi-", StringComparison.OrdinalIgnoreCase))
            icon = icon[3..];

        var accent = settings.Accent?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(accent) && accent is not ("primary" or "success" or "info" or "warning" or "danger" or "secondary"))
            throw new ArgumentException($"Unsupported accent '{accent}'. Use primary, success, info, warning, danger or secondary.");

        return new EntitySpec(
            name,
            idType,
            properties,
            generateBlazor,
            registerNav,
            requireAuth,
            string.IsNullOrEmpty(icon) ? null : icon,
            string.IsNullOrEmpty(accent) ? null : accent);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

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

        AnsiConsole.Write(new Panel(headerGrid)
        {
            Header = new PanelHeader($"[bold] {entity.Name.EscapeMarkup()} [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.MediumPurple1),
            Padding = new Padding(2, 1, 2, 1),
        });

        if (entity.Properties.Count > 0)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey39)
                .Title("[bold]✎ Propiedades[/]")
                .AddColumn("[grey]#[/]")
                .AddColumn("[bold]Nombre[/]")
                .AddColumn("[bold]Tipo[/]")
                .AddColumn("[bold]Req[/]");

            for (var i = 0; i < entity.Properties.Count; i++)
            {
                var p = entity.Properties[i];
                table.AddRow(
                    $"[grey]{i + 1}[/]",
                    $"[white]{p.Name.EscapeMarkup()}[/]",
                    $"[aqua]{p.Type.EscapeMarkup()}[/]",
                    p.Required ? "[bold red]✔[/]" : "[grey]·[/]");
            }
            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[grey](sin propiedades)[/]");
        }

        AnsiConsole.WriteLine();
    }

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
