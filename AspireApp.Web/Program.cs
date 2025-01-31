using AspireApp.ServiceDefaults;
using AspireApp.Web.ApiClients;
using AspireApp.Web.Components;
using AspireApp.Web.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// Debe ser singleton. Si es transient o scoped no lleva el token en la solicitud.
builder.Services.AddSingleton<JWTTokenHandler>();
builder.Services.AddSingleton<JWTTokenProvider>();


builder.Services.AddHttpClient("ApiClient", client =>
{
    // Es el nombre que se le da en el program de Aspire (api)
    client.BaseAddress = new Uri("https+http://api");
})
.AddHttpMessageHandler<JWTTokenHandler>();


// Puede ser scoped, transient o singleton aparentemente
builder.Services.AddScoped<WeatherApiClient>();
builder.Services.AddScoped<LoginApiClient>();
builder.Services.AddScoped<RegisterApiClient>();

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