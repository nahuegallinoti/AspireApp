using AspireApp.Api.Infrastructure;
using AspireApp.Application.Implementations;
using AspireApp.Application.Implementations.EventBus;
using AspireApp.DataAccess.Implementations;
using AspireApp.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.AddJwtAuthentication();
builder.Services.AddAuthorizationPolicies();

builder.AddCaching();

builder.Services.AddDataAccess();
builder.Services.AddApplicationServices(builder.Configuration);
builder.AddMessageBus();

builder.Services.AddCors(options =>
{
    var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();
        if (allowed is { Length: > 0 })
            policy.WithOrigins(allowed).AllowCredentials();
        else
            policy.SetIsOriginAllowed(_ => true); // dev fallback (no credentials)
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await SeedDatabaseAsync(app);

app.UseExceptionHandler();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", options =>
    {
        options.WithTitle("AspireApp API")
               .WithTheme(ScalarTheme.BluePlanet);
    });
}

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}
