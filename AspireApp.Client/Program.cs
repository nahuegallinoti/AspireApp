using AspireApp.Client.ApiClients;
using AspireApp.Client.Components;
using AspireApp.Client.Handlers;
using AspireApp.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddScoped<JwtTokenHandler>();
builder.Services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

builder.Services
    .AddHttpClient(HttpClientNames.Api, client =>
    {
        client.BaseAddress = new Uri("https+http://api");
    })
    .AddHttpMessageHandler<JwtTokenHandler>();

builder.Services.AddScoped<LoginApiClient>();
builder.Services.AddScoped<RegisterApiClient>();
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<ShowApiClient>();
builder.Services.AddScoped<MessageBusApiClient>();

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
