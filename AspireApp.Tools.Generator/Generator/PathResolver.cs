namespace AspireApp.Tools.Generator.Generator;

internal sealed class PathResolver
{
    public string SolutionRoot { get; }

    public PathResolver(string solutionRoot)
    {
        SolutionRoot = solutionRoot ?? throw new ArgumentNullException(nameof(solutionRoot));
    }

    /// <summary>
    /// Walks upwards from the given start directory until a folder containing the .slnx is found.
    /// </summary>
    public static PathResolver Discover(string? startDirectory = null)
    {
        var current = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (current.GetFiles("*.slnx").Length > 0)
                return new PathResolver(current.FullName);
            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the AspireApp solution root (no .slnx file found while walking up from " +
            (startDirectory ?? Directory.GetCurrentDirectory()) + ").");
    }

    public string Combine(params string[] segments) =>
        Path.Combine([SolutionRoot, .. segments]);

    public string DomainEntity(string entity) =>
        Combine("AspireApp.Domain.Entities", $"{entity}.cs");

    public string ApplicationModel(string entity) =>
        Combine("AspireApp.Application.Models", "App", $"{entity}.cs");

    public string ApplicationFilter(string entity) =>
        Combine("AspireApp.Application.Models", "App", $"{entity}Filter.cs");

    public string ApplicationContract(string entity) =>
        Combine("AspireApp.Application.Contracts", entity, $"I{entity}Service.cs");

    public string ApplicationService(string entity) =>
        Combine("AspireApp.Application.Implementations", entity, $"{entity}Service.cs");

    public string ApplicationMapper(string entity) =>
        Combine("AspireApp.Application.Mappers", $"{entity}Mapper.cs");

    public string ApplicationPersistence(string entity) =>
        Combine("AspireApp.Application.Persistence", $"I{entity}DA.cs");

    public string DataAccess(string entity) =>
        Combine("AspireApp.DataAccess.Implementations", $"{entity}DA.cs");

    public string ApiController(string entity) =>
        Combine("AspireApp.Api", "Controllers", $"{entity}Controller.cs");

    public string ApiClient(string entity) =>
        Combine("AspireApp.Client.ApiClients", $"{entity}ApiClient.cs");

    public string BlazorIndexRazor(string entity) =>
        Combine("AspireApp.Client", "Components", "Pages", $"{entity}Index.razor");

    public string BlazorIndexCs(string entity) =>
        Combine("AspireApp.Client", "Components", "Pages", $"{entity}Index.razor.cs");

    public string BlazorEditRazor(string entity) =>
        Combine("AspireApp.Client", "Components", "Pages", $"{entity}Edit.razor");

    public string BlazorEditCs(string entity) =>
        Combine("AspireApp.Client", "Components", "Pages", $"{entity}Edit.razor.cs");

    public string AppDbContext =>
        Combine("AspireApp.DataAccess.Implementations", "AppDbContext.cs");

    public string DataAccessDI =>
        Combine("AspireApp.DataAccess.Implementations", "DependencyInjection.cs");

    public string ApplicationDI =>
        Combine("AspireApp.Application.Implementations", "DependencyInjection.cs");

    public string MappersDI =>
        Combine("AspireApp.Application.Mappers", "DependencyInjection.cs");

    public string ClientProgram =>
        Combine("AspireApp.Client", "Program.cs");

    public string NavMenu =>
        Combine("AspireApp.Client", "Components", "Layout", "NavMenu.razor");
}
