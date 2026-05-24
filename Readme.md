# AspireApp

Boilerplate moderno sobre **.NET 10 + .NET Aspire**, Blazor Server, Clean Architecture y Railway Oriented Programming.

Está pensado como punto de partida para microservicios o monolitos modulares: viene con autenticación JWT, caché híbrido (Redis + memoria), un **message bus intercambiable RabbitMQ ↔ Kafka** seleccionable por configuración, observabilidad con OpenTelemetry y tests unitarios e integración con xUnit + FluentAssertions + NSubstitute.

---

## Stack

- .NET 10 / C# latest
- .NET Aspire 13.3.x (AppHost + integraciones cliente)
- ASP.NET Core (Controllers + Microsoft.AspNetCore.OpenApi + Scalar UI)
- Blazor Server (Razor Components)
- Entity Framework Core 10 (InMemory por defecto — listo para Sql/Pg)
- Redis (Distributed Cache + HybridCache nivel 2)
- RabbitMQ.Client 7 **o** Confluent.Kafka 2 — un solo binario, decide en runtime
- OpenTelemetry (tracing + metrics + logs, instrumentación EF Core / HTTP / Redis)
- JWT bearer auth con `IOptions<JwtOptions>` validado en startup
- xUnit v3, FluentAssertions, NSubstitute

Versiones centralizadas en [Directory.Packages.props](Directory.Packages.props). Propiedades comunes (TargetFramework, Nullable, analyzers) en [Directory.Build.props](Directory.Build.props).

---

## Estructura

```
0.Domain/
  AspireApp.Domain.Entities       Entidades puras del dominio
  AspireApp.Domain.ROP            Result<T> + Bind/Map (Railway Oriented Programming)

1.Application/
  AspireApp.Application.Contracts       Interfaces de servicios + Options (Jwt, EventBus)
  AspireApp.Application.Implementations Lógica de negocio + DependencyInjection.AddApplicationServices
  AspireApp.Application.Mappers         Mappers manuales (Entity ↔ Model)
  AspireApp.Application.Models          DTOs / payloads HTTP
  AspireApp.Application.Persistence     Interfaces de DataAccess (IBaseDA, IUserDA, ...)

2.Infrastructure/
  AspireApp.DataAccess.Implementations  EF Core + AppDbContext + AddDataAccess

3.Presentation/
  AspireApp.Api                          Controllers, JWT, OpenAPI, Scalar UI, GlobalExceptionHandler

4.Client/
  AspireApp.Client                       Blazor Server (Razor Components)
  AspireApp.Client.ApiClients            Tipados sobre HttpClient con ROP

Host/
  AspireApp.AppHost                      Orquesta Redis + RabbitMQ/Kafka + Api + WebFrontend
  AspireApp.ServiceDefaults              Health checks, OpenTelemetry, resilience, service discovery

Tests/
  AspireApp.Tests                        xUnit + FluentAssertions + NSubstitute
```

Cada capa **solo** referencia las capas inferiores (Domain ← Application ← Infra/Presentation). `ServiceDefaults` es transversal y no conoce la app.

---

## Quick start

### Prerrequisitos
- .NET 10 SDK
- Docker Desktop (para los contenedores de Redis / RabbitMQ / Kafka que levanta el AppHost)

### Correr la solución completa

```pwsh
dotnet run --project AspireApp.AppHost
```

Eso levanta:
- **Aspire dashboard** (URL en la consola)
- **Redis** + **RedisInsight** UI
- **RabbitMQ** (con management UI) **o** **Kafka** (con Kafka-UI) — depende del setting
- **API** en `https+http://api`
- **Blazor frontend** con endpoints externos

La API expone:
- `/scalar` — UI moderna de OpenAPI
- `/openapi/v1.json` — spec OpenAPI nativa de ASP.NET Core
- `/health`, `/alive` (solo en Development)

### Tests

```pwsh
dotnet test
```

---

## El bus dual RabbitMQ ↔ Kafka

Una sola compilación, un solo binario. El provider se elige por configuración:

```json
// AspireApp.AppHost/appsettings.json
{
  "EventBus": { "Provider": "RabbitMq" }  // o "Kafka"
}
```

- El **AppHost** lee el setting y provisiona el contenedor correcto (RabbitMQ con management plugin **o** Kafka con Kafka-UI).
- Le pasa al API la variable `EventBus__Provider` para que la app sepa qué cliente armar.
- En el **API**, `AddMessageBus()` resuelve la integración Aspire (`AddRabbitMQClient` / `AddKafkaProducer`) y registra la implementación correcta de `IMessageBus`.

El controller y el código de aplicación trabajan siempre contra `IMessageBus`:

```csharp
public interface IMessageBus
{
    Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic, CancellationToken ct = default)
        where TMessage : class;
}
```

`topic` se interpreta como **routing key** en Rabbit (exchange direct duradero `aspireapp.events`) o como **topic** en Kafka.

Para alternar el broker basta con cambiar el setting y volver a correr `dotnet run --project AspireApp.AppHost`.

---

## Patrones clave

### Railway Oriented Programming (ROP)
`Result<T>` con valor, errores y `HttpStatusCode`. Combinable con `Bind`/`Map` síncrono o async. Helpers `Result.NotFound<T>`, `Result.Conflict<T>`, `Result.Unauthorized<T>` para no perder el código HTTP.

```csharp
public Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken ct) =>
    user.Validate()
        .Bind(validated => dependencies.VerifyUserPasswordAsync(validated, ct))
        .Bind(dependencies.CreateToken);
```

### Options validados en startup
JWT y EventBus usan `AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()`. Si falta una key obligatoria, la app no arranca.

### Composición desde el Composition Root
Cada proyecto expone su `AddXxx(this IServiceCollection|IHostApplicationBuilder)`. El `Api/Program.cs` los compone:

```csharp
builder.AddJwtAuthentication();
builder.AddCaching();
builder.Services.AddDataAccess();
builder.Services.AddApplicationServices(builder.Configuration);
builder.AddMessageBus();
```

### Caché en dos niveles
`HybridCache` (memoria L1) + Redis (L2 distribuido vía Aspire). El `BaseService` invalida por tag al crear, evitando datos rancios.

### OpenAPI nativo + Scalar
`AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference("/scalar")`. Sin Swashbuckle.

### Manejo global de errores
`IExceptionHandler` + `UseExceptionHandler()` + `AddProblemDetails()`. Toda excepción no manejada termina en un `ProblemDetails` JSON estándar.

---

## Probar el flujo de demo

1. `POST /api/auth/register` con `{ "email", "password", "name", "surname" }`.
2. `POST /api/auth/login` → te devuelve un token.
3. `POST /api/product` con `Authorization: Bearer <token>` → crea un producto **y** publica un evento al broker configurado.
4. Mirá la cola/topic `product` en la UI del broker (Management de Rabbit o Kafka-UI).

---

## Cambiar la DB real

Editá `AspireApp.DataAccess.Implementations/DependencyInjection.cs` y reemplazá `UseInMemoryDatabase` por `UseSqlServer` / `UseNpgsql` / etc. En `AppHost`, agregá el recurso (`builder.AddSqlServer("db")`) y referencialo desde el `api`.

---

## Convenciones

- `internal sealed` para clases de implementación. Solo los contratos son `public`.
- `CancellationToken ct` en todo método async público.
- Sin try/catch decorativos: las excepciones propagan al `GlobalExceptionHandler`.
- Sin comentarios obvios. Solo se documenta el *por qué* cuando no es deducible del código.
