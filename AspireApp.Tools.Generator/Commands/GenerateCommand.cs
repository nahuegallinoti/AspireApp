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

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new FigletText("Aspire Gen").Color(Color.MediumPurple1));
        AnsiConsole.MarkupLine("[grey]Generates Domain → Application → Infrastructure → Api → Client files for a new entity.[/]");
        AnsiConsole.WriteLine();

        var paths = settings.Root is { Length: > 0 }
            ? new PathResolver(settings.Root)
            : PathResolver.Discover();

        AnsiConsole.MarkupLine($"[grey]Solution root:[/] [aqua]{paths.SolutionRoot.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

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

        await PreviewPlanAsync(plan, entity);

        if (!settings.Yes && !settings.DryRun && !AnsiConsole.Confirm("[yellow]Proceed with generation?[/]"))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled by user.[/]");
            return 0;
        }

        var renderer = new TemplateRenderer();
        var totalCreated = 0;
        var totalMutated = 0;
        var totalSkipped = 0;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Creating files[/]");

        foreach (var creation in plan.Creations)
        {
            var rendered = renderer.Render(creation.TemplateName, entity);

            if (File.Exists(creation.TargetPath))
            {
                AnsiConsole.MarkupLine($"  [yellow]· {Rel(creation.TargetPath, paths)} (already exists, skipped)[/]");
                totalSkipped++;
                continue;
            }

            if (settings.DryRun)
            {
                AnsiConsole.MarkupLine($"  [aqua]+ {Rel(creation.TargetPath, paths)}[/] [grey](dry-run)[/]");
                totalCreated++;
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(creation.TargetPath)!);
            await File.WriteAllTextAsync(creation.TargetPath, rendered, cancellationToken);
            AnsiConsole.MarkupLine($"  [green]+ {Rel(creation.TargetPath, paths)}[/]");
            totalCreated++;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Updating shared files[/]");

        foreach (var mutator in plan.Mutators)
        {
            if (!File.Exists(mutator.TargetPath))
            {
                AnsiConsole.MarkupLine($"  [red]✗ {Rel(mutator.TargetPath, paths)} not found[/]");
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
                AnsiConsole.MarkupLine($"  [red]✗ {Rel(mutator.TargetPath, paths)}: {ex.Message.EscapeMarkup()}[/]");
                continue;
            }

            if (!result.Changed)
            {
                AnsiConsole.MarkupLine($"  [grey]· {Rel(mutator.TargetPath, paths)} ({result.Description.EscapeMarkup()})[/]");
                continue;
            }

            if (!settings.DryRun)
                await File.WriteAllTextAsync(mutator.TargetPath, result.NewContent, cancellationToken);

            var marker = settings.DryRun ? "[grey](dry-run)[/]" : string.Empty;
            AnsiConsole.MarkupLine($"  [green]~ {Rel(mutator.TargetPath, paths)}[/] [grey]({result.Description.EscapeMarkup()})[/] {marker}");
            totalMutated++;
        }

        AnsiConsole.WriteLine();
        var rule = new Rule("[green]Done[/]") { Justification = Justify.Left };
        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine(
            $"[green]✓[/] {totalCreated} created, [aqua]{totalMutated}[/] modified, [yellow]{totalSkipped}[/] skipped.");

        if (settings.DryRun)
            AnsiConsole.MarkupLine("[yellow]Dry-run: nothing was written to disk.[/]");
        else
            AnsiConsole.MarkupLine($"[grey]Tip: run[/] [white]dotnet build[/] [grey]to confirm everything compiles.[/]");

        return 0;
    }

    private static EntitySpec BuildEntitySpec(Settings settings)
    {
        var name = settings.EntityName;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Entity name[/] (PascalCase, singular):")
                    .Validate(static s => string.IsNullOrWhiteSpace(s)
                        ? ValidationResult.Error("Required")
                        : char.IsLower(s[0])
                            ? ValidationResult.Error("Must start with an uppercase letter")
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
                    .Title("[bold]Id type[/]")
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
            AnsiConsole.MarkupLine("[grey]Add properties (empty name to finish).[/]");
            while (true)
            {
                var propName = AnsiConsole.Prompt(
                    new TextPrompt<string>("[grey]Property name (or empty to finish):[/]")
                        .AllowEmpty());

                if (string.IsNullOrWhiteSpace(propName)) break;

                var propType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[grey]Type for[/] [yellow]{propName}[/]")
                        .AddChoices("string", "int", "long", "decimal", "double", "bool", "DateTime", "Guid"));

                var required = AnsiConsole.Confirm($"[grey]Is[/] [yellow]{propName}[/] [grey]required?[/]", defaultValue: true);
                properties.Add(new PropertySpec(Capitalize(propName), propType, required));
            }
        }

        if (properties.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No properties specified. Generating with empty body (you can add them later).[/]");
        }

        var interactive = !settings.Yes;

        bool generateBlazor;
        if (settings.NoUi)
            generateBlazor = false;
        else if (interactive)
            generateBlazor = AnsiConsole.Confirm(
                "[bold]Generate Blazor pages[/] [grey](Index with filters/grid + Edit/Detail + delete)?[/]",
                defaultValue: true);
        else
            generateBlazor = true;

        bool registerNav;
        if (settings.NoNav || !generateBlazor)
            registerNav = false;
        else if (interactive)
            registerNav = AnsiConsole.Confirm(
                "[bold]Add a NavLink in NavMenu.razor[/]?",
                defaultValue: true);
        else
            registerNav = true;

        var requireAuth = !settings.NoAuth;

        return new EntitySpec(name, idType, properties, generateBlazor, registerNav, requireAuth);
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static void RenderEntitySummary(EntitySpec entity)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().Width(20))
            .AddColumn();

        grid.AddRow("[grey]Entity[/]", $"[bold]{entity.Name}[/]");
        grid.AddRow("[grey]Plural[/]", entity.Plural);
        grid.AddRow("[grey]Id type[/]", entity.IdType);
        grid.AddRow("[grey]Properties[/]",
            entity.Properties.Count == 0
                ? "[red]<none>[/]"
                : string.Join(", ", entity.Properties.Select(p => $"{p.Name}:{p.Type}{(p.Required ? "*" : "")}")));
        grid.AddRow("[grey]Blazor page[/]", entity.GenerateBlazorPage ? "[green]yes[/]" : "[grey]no[/]");
        grid.AddRow("[grey]NavMenu[/]", entity.GenerateBlazorPage && entity.RegisterInNavMenu ? "[green]yes[/]" : "[grey]no[/]");
        grid.AddRow("[grey]Authorize[/]", entity.RequireAuth ? "[green]yes[/]" : "[grey]no[/]");

        AnsiConsole.Write(new Panel(grid).Header("[bold]Entity[/]"));
        AnsiConsole.WriteLine();
    }

    private static async Task PreviewPlanAsync(GenerationPlan plan, EntitySpec entity)
    {
        await Task.CompletedTask;
        var tree = new Tree("[bold]Plan[/]");
        var creations = tree.AddNode($"[green]Files to create[/] ({plan.Creations.Count})");
        foreach (var c in plan.Creations)
            creations.AddNode($"[aqua]{c.FriendlyLabel}[/] [grey]→[/] {c.TargetPath.EscapeMarkup()}");

        var mutations = tree.AddNode($"[yellow]Files to modify[/] ({plan.Mutators.Count})");
        foreach (var m in plan.Mutators)
            mutations.AddNode(m.TargetPath.EscapeMarkup());

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private static string Rel(string fullPath, PathResolver paths) =>
        Path.GetRelativePath(paths.SolutionRoot, fullPath).EscapeMarkup();
}
