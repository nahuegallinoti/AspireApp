using AspireApp.Tools.Generator.Generator.Mutators;

namespace AspireApp.Tools.Generator.Generator;

internal sealed record FileCreation(string TargetPath, string TemplateName, string FriendlyLabel);

internal sealed class GenerationPlan
{
    public required IReadOnlyList<FileCreation> Creations { get; init; }
    public required IReadOnlyList<IFileMutator> Mutators { get; init; }

    public static GenerationPlan Build(EntitySpec entity, PathResolver paths)
    {
        var creations = new List<FileCreation>
        {
            new(paths.DomainEntity(entity.Name), "Domain.Entity.scriban", "Domain.Entity"),
            new(paths.ApplicationModel(entity.Name), "Application.Model.scriban", "Application.Model"),
            new(paths.ApplicationContract(entity.Name), "Application.Contract.scriban", "Application.Contract"),
            new(paths.ApplicationService(entity.Name), "Application.Service.scriban", "Application.Service"),
            new(paths.ApplicationMapper(entity.Name), "Application.Mapper.scriban", "Application.Mapper"),
            new(paths.ApplicationPersistence(entity.Name), "Application.Persistence.scriban", "Application.Persistence"),
            new(paths.DataAccess(entity.Name), "DataAccess.scriban", "DataAccess"),
            new(paths.ApiController(entity.Name), "Api.Controller.scriban", "Api.Controller"),
            new(paths.ApiClient(entity.Name), "Client.ApiClient.scriban", "Client.ApiClient"),
        };

        if (entity.GenerateBlazorPage)
        {
            creations.Add(new(paths.BlazorIndexRazor(entity.Name), "Client.Index.razor.scriban", "Client.Index.razor"));
            creations.Add(new(paths.BlazorIndexCs(entity.Name), "Client.Index.razor.cs.scriban", "Client.Index.razor.cs"));
            creations.Add(new(paths.BlazorEditRazor(entity.Name), "Client.Edit.razor.scriban", "Client.Edit.razor"));
            creations.Add(new(paths.BlazorEditCs(entity.Name), "Client.Edit.razor.cs.scriban", "Client.Edit.razor.cs"));
        }

        var mutators = new List<IFileMutator>
        {
            new DbContextMutator(paths.AppDbContext),
            new DiRegistrationMutator(
                paths.DataAccessDI,
                usingLines: [],
                registrationLine: $"        services.AddScoped<I{entity.Name}DA, {entity.Name}DA>();",
                markerForLastRegistration: "AddScoped<I"),
            new DiRegistrationMutator(
                paths.ApplicationDI,
                usingLines:
                [
                    $"using AspireApp.Application.Contracts.{entity.Name};",
                    $"using AspireApp.Application.Implementations.{entity.Name};",
                ],
                registrationLine: $"        services.AddScoped<I{entity.Name}Service, {entity.Name}Service>();",
                markerForLastRegistration: "services.AddScoped<I"),
            new DiRegistrationMutator(
                paths.MappersDI,
                usingLines: [],
                registrationLine: $"        services.AddSingleton<{entity.Name}Mapper>();",
                markerForLastRegistration: "AddSingleton<"),
            new DiRegistrationMutator(
                paths.ClientProgram,
                usingLines: [],
                registrationLine: $"builder.Services.AddScoped<{entity.Name}ApiClient>();",
                markerForLastRegistration: "builder.Services.AddScoped<"),
        };

        if (entity.GenerateBlazorPage && entity.RegisterInNavMenu)
            mutators.Add(new NavMenuMutator(paths.NavMenu));

        return new GenerationPlan
        {
            Creations = creations,
            Mutators = mutators,
        };
    }
}
