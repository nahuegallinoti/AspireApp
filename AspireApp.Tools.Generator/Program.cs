using System.Text;
using AspireApp.Tools.Generator.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    var app = new CommandApp<GenerateCommand>();
    app.Configure(config =>
    {
        config.SetApplicationName("aspireapp-gen");
        config.AddCommand<GenerateCommand>("generate")
            .WithAlias("g")
            .WithDescription("Generates the full file structure for a new entity.")
            .WithExample("generate", "Order")
            .WithExample("generate", "Order", "--prop", "Total:decimal:required", "--prop", "Notes:string");
    });

    return await app.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Unexpected error:[/] {ex.Message}");
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return -1;
}
