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
builder.Services.AddAuthorization();

builder.AddCaching();

builder.Services.AddDataAccess();
builder.Services.AddApplicationServices(builder.Configuration);
builder.AddMessageBus();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

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