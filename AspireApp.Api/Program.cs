using AspireApp.DataAccess.Implementations;
using AspireApp.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar la autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,  // Cambiado de false a true para validar el Audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],  // Configura el Issuer
        ValidAudience = jwtSettings["Audience"],  // Configura el Audience
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new ArgumentNullException("Jwt:Key")))
    };
});

builder.Services.RegisterMappers();
builder.Services.RegisterDataAccess();
builder.Services.RegisterAppServices();
builder.Services.RegisterRabbitService();

builder.Services.AddLogging(options =>
{
    options.AddConsole();  // Loguear a la consola
    // Agregar otras opciones de logging (ej. archivos, bases de datos, etc.)
});

// CACHE

// Memory cache. No es necesario si uso hybrid
//builder.Services.AddMemoryCache(); 

// Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// Hybrid cache - buscará primero en in memory (cache level 1), si no en redis (cache level 2) si está configurado
builder.Services.AddHybridCache();

builder.Services.AddAuthorization();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

//builder.Services.AddScoped(provider =>
//    provider.GetService<AppDbContext>() ?? throw new ArgumentNullException(nameof(AppDbContext)))
//    .AddSingleton(sp =>
//    {
//        var builder = new DbContextOptionsBuilder<AppDbContext>();
//        return builder.Options;
//    })
//    .AddScoped(sp =>
//    {
//        return new AppDbContext(sp.GetRequiredService<DbContextOptions<AppDbContext>>());
//    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AspireAppDb"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

// Mapea controlador product y esos automáticamente
app.MapControllers();

app.Run();