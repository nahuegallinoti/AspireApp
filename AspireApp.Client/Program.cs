using AspireApp.Client.ApiClients;
using AspireApp.Client.Components;
using AspireApp.Client.Handlers;
using AspireApp.Client.Services;
using AspireApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// Debe ser scoped o transient. Los delegating handler no pueden ser singleton porque ocurre un error al cabo de un rato.
builder.Services.AddScoped<JWTTokenHandler>();

// Debe ser singleton. Si es transient o scoped no lleva el token en la solicitud.
builder.Services.AddSingleton<JWTTokenProvider>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    // Es el nombre que se le da en el program de Aspire (api)
    client.BaseAddress = new Uri("https+http://api");
})
.AddHttpMessageHandler<JWTTokenHandler>();

builder.Services.AddLogging(options =>
{
    options.AddConsole();  // Loguear a la consola
    // Agregar otras opciones de logging (ej. archivos, bases de datos, etc.)
});

// Puede ser scoped, transient o singleton aparentemente
builder.Services.AddScoped<LoginApiClient>();
builder.Services.AddScoped<RegisterApiClient>();
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<ShowApiClient>();
builder.Services.AddScoped<RabbitMqApiClient>();
builder.Services.AddScoped<RabbitMqSenderService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();