# AspireApp

Modern boilerplate built on **.NET 10 + .NET Aspire**, with Blazor Server, Clean Architecture and Railway Oriented Programming.

It targets people who want a ready-to-extend starting point for modular monoliths or small microservice setups. Out of the box you get JWT + Google SSO authentication, refresh-token rotation with server-side session storage, role-based authorization, hybrid caching (Redis + memory), an interchangeable **RabbitMQ ↔ Kafka** message bus selectable by configuration, OpenTelemetry tracing/metrics/logs, and unit + integration tests using xUnit + FluentAssertions + NSubstitute.

---

## Stack

| Layer          | Pieces                                                                                          |
| -------------- | ----------------------------------------------------------------------------------------------- |
| Runtime        | .NET 10, C# latest                                                                              |
| Orchestration  | .NET Aspire 13.3.x (AppHost + client integrations)                                              |
| API            | ASP.NET Core Controllers + native OpenAPI + Scalar UI                                           |
| Frontend       | Blazor Server (interactive Razor components) + Bootstrap 5 + Bootstrap Icons + TypeScript build |
| Persistence    | EF Core 10 (InMemory by default — drop-in for SQL Server / PostgreSQL / etc.)                   |
| Caching        | `HybridCache` (L1 memory) + Redis (L2 distributed) via Aspire                                   |
| Messaging      | `RabbitMQ.Client 7` **or** `Confluent.Kafka 2` — one binary, switch at runtime                  |
| Observability  | OpenTelemetry (ASP.NET, HTTP, EF Core, Redis, runtime)                                          |
| Authentication | JWT bearer + cookie + Google OIDC. Refresh-token rotation with hashed storage and revocation.  |
| Authorization  | Role-based policies (`AdminOnly`, `AuthenticatedUser`)                                          |
| Validation     | DataAnnotations + FluentValidation                                                              |
| Tests          | xUnit v3, FluentAssertions, NSubstitute                                                         |
| CLI scaffolds  | `AspireApp.Tools.Generator` (Spectre.Console)                                                   |

Package versions are centralized in [Directory.Packages.props](Directory.Packages.props); shared MSBuild properties (TargetFramework, Nullable, analyzers) in [Directory.Build.props](Directory.Build.props).

---

## Project layout

```
AspireApp/
├── AspireApp.AppHost                          Aspire orchestration: Redis, RabbitMQ/Kafka, API, Web.
├── AspireApp.ServiceDefaults                  Shared OTel / health-check / service-discovery defaults.
│
├── AspireApp.Domain.Entities                  Pure domain entities (User, Role, UserRole, Product, Show, RefreshToken).
├── AspireApp.Domain.ROP                       Result<T>, Unit, Bind/Map for Railway Oriented Programming.
│
├── AspireApp.Application.Contracts            Service contracts + Options (JWT, Identity, EventBus, SSO).
├── AspireApp.Application.Models               DTOs + request/response shapes.
├── AspireApp.Application.Mappers              Manual Entity ↔ Model mappers.
├── AspireApp.Application.Persistence          Persistence interfaces consumed by Application.
├── AspireApp.Application.Implementations      Business logic: Auth, Users, Roles, Product, Show, EventBus.
│
├── AspireApp.DataAccess.Implementations       EF Core: AppDbContext, DAs, seeding.
│
├── AspireApp.Api                              HTTP layer: controllers, JWT auth, OpenAPI/Scalar, global error handler.
│
├── AspireApp.Client                           Blazor Server frontend: cookie+JWT auth, admin pages, public pages.
├── AspireApp.Client.ApiClients                Typed HttpClient wrappers consumed by Blazor.
│
├── AspireApp.Tests                            xUnit test suite (application, mappers, auth, ROP, DA).
└── AspireApp.Tools.Generator                  Spectre CLI to scaffold a new entity across every layer.
```

Architectural rule of thumb:

```
Domain   ← no dependencies
Application   → Domain
Application.Persistence (contracts)   →   DataAccess.Implementations (EF Core)
Api      → Application (composition root for the backend)
Client   → Api (over HTTP) + Application.Models (shared DTOs)
AppHost  →   orchestrates Api + Client + Redis + broker
```

---

## Quick start

### Prerequisites

- **.NET 10 SDK**
- **Docker Desktop** (Aspire provisions Redis + RabbitMQ/Kafka as containers)

### Run

```pwsh
dotnet run --project AspireApp.AppHost
```

The Aspire dashboard URL is printed to the console. From the dashboard you can open every running resource:

- **Aspire dashboard** — process logs, traces, metrics, env vars
- **Redis** + **RedisInsight** UI
- **RabbitMQ Management UI** *or* **Kafka-UI** (depending on `EventBus:Provider`)
- **API** — exposes:
  - `GET /scalar` — modern OpenAPI UI
  - `GET /openapi/v1.json` — native OpenAPI spec
  - `GET /health` and `GET /alive` (development only)
- **Blazor frontend** — `webfrontend` with external HTTP endpoints

### Default admin

`Identity.SeedAdmin` in `AspireApp.Api/appsettings.json` seeds a local admin on first boot:

| Email                    | Password        |
| ------------------------ | --------------- |
| `admin@aspireapp.local`  | `ChangeMe!2026` |

Change the password before going anywhere near a real deployment, or set `Identity:SeedAdmin:Enabled` to `false` and create the admin through your own flow.

### Tests

```pwsh
dotnet test
```

---

## The dual RabbitMQ ↔ Kafka bus

One compilation, one binary — the broker is picked by configuration:

```json
// AspireApp.AppHost/appsettings.json
{ "EventBus": { "Provider": "RabbitMq" } }   // or "Kafka"
```

What happens behind the scenes:

1. The **AppHost** reads the setting and provisions the matching container (RabbitMQ + management plugin, **or** Kafka + Kafka-UI).
2. The same setting (`EventBus__Provider`) is propagated to the API as an env var.
3. In the API, `AddMessageBus()` reads the value, wires up the matching Aspire integration (`AddRabbitMQClient` or `AddKafkaProducer`) and registers the right `IMessageBus`.
4. Application code talks only to `IMessageBus` — `topic` becomes a RabbitMQ routing key (against a durable direct exchange named `aspireapp.events`) or a Kafka topic.

```csharp
public interface IMessageBus
{
    Task<Result<string>> PublishAsync<TMessage>(TMessage message, string topic, CancellationToken ct = default)
        where TMessage : class;
}
```

To swap brokers: edit the setting, restart the AppHost. No code changes.

---

## Authentication and authorization

Two parallel pieces:

- **API side** (JWT)
  - Local register / login → access + refresh tokens.
  - Refresh tokens are stored hashed (SHA-256) in the database, single-use, rotated on every refresh.
  - Account lockout after N failed attempts (`Identity:MaxFailedAccessAttempts`).
  - Password hashing with PBKDF2 (600 000 iterations by default — `Identity:PasswordIterations`).
  - External login (Google OIDC) at `POST /api/auth/external/google` — id_token preferred, access_token fallback.
  - Role claims emitted in the JWT; policies `AdminOnly` and `AuthenticatedUser` defined centrally.

- **Web side** (Blazor)
  - Cookie scheme `AspireApp.Cookies` stores identity claims only.
  - Access + refresh tokens live in an `IAuthSessionStore` backed by `HybridCache` (Redis L2 + memory L1).
  - `JwtTokenHandler` attaches the access token to outgoing API calls and transparently refreshes on 401.
  - Optional “Sign in with Google” via `Microsoft.AspNetCore.Authentication.Google`, exchanging the Google id_token for an AspireApp JWT.

### Google SSO setup

The AppHost picks credentials up from environment variables (priority) or `appsettings`/user-secrets:

| Variable                  | Purpose                                                      |
| ------------------------- | ------------------------------------------------------------ |
| `GOOGLE_CLIENT_ID`        | OAuth 2.0 client ID from the Google Cloud Console            |
| `GOOGLE_CLIENT_SECRET`    | OAuth 2.0 client secret                                      |
| `SSO_GOOGLE_ENABLED`      | `true`/`false` override. If unset, SSO turns on when both ID and secret are present |

Both the API and the Blazor app receive the same `Sso__Google__*` env vars, so the frontend can present the “Continue with Google” button and the backend can validate Google id_tokens.

---

## Patterns used

### Railway Oriented Programming

`Result<T>` carries a value, errors and an `HttpStatusCode`. Combine flows with `Bind`/`Map` synchronously or asynchronously. Helpers `Result.NotFound<T>`, `Result.Conflict<T>`, `Result.Unauthorized<T>` keep HTTP semantics intact end to end.

```csharp
public Task<Result<AuthenticationResult>> Login(UserLogin user, CancellationToken ct) =>
    user.Validate()
        .Bind(validated => dependencies.VerifyUserPasswordAsync(validated, ct))
        .Bind(dependencies.CreateToken);
```

`ResultExtensions.ToActionResult(this Result<T>, ControllerBase)` translates the result into an `ObjectResult` carrying the correct status code, or `Problem(...)` for failures.

### Options validated on startup

JWT, Identity, SSO and EventBus all use `AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()`. Missing keys fail startup loudly instead of silently producing bad runtime behaviour.

### Composition Root

Each project exposes an `AddXxx(this IServiceCollection|IHostApplicationBuilder)` extension; `Api/Program.cs` composes them:

```csharp
builder.AddServiceDefaults();
builder.AddJwtAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.AddCaching();
builder.Services.AddDataAccess();
builder.Services.AddApplicationServices(builder.Configuration);
builder.AddMessageBus();
```

### Two-level cache

`HybridCache` (in-memory L1) + Redis (L2 distributed via Aspire). `BaseService<TEntity, TModel, TID>` invalidates the cache by tag on `AddAsync` and after `SaveChangesAsync` on `Update` / `Delete`, so reads never go stale.

### OpenAPI + Scalar

`AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference("/scalar")`. No Swashbuckle.

### Global error handling

`IExceptionHandler` + `UseExceptionHandler()` + `AddProblemDetails()`. Any unhandled exception becomes a JSON `ProblemDetails`. In production the `Detail` field is suppressed to avoid leaking exception messages.

### Restrictive CORS in production

`Api/Program.cs` reads `Cors:AllowedOrigins` (string array). When that setting is populated, only those origins are allowed and `AllowCredentials` is enabled. When empty, the API falls back to `SetIsOriginAllowed(_ => true)` *without* credentials — fine for dev, useless to attackers.

---

## Demo flow

1. `POST /api/auth/register` with `{ "email", "password", "name", "surname" }`.
2. `POST /api/auth/login` → get an access + refresh token.
3. `POST /api/product` with `Authorization: Bearer <accessToken>` → creates a product **and** publishes a `Product <id> created` event to the broker.
4. Check the queue/topic named `product` in the broker UI (RabbitMQ Management or Kafka-UI).
5. `POST /api/auth/refresh` with the refresh token → get a fresh pair (the previous refresh token is revoked atomically).

The Blazor frontend exposes the same demo end-to-end: `/login` → `/product` (or `/show`, `/eventbus`) → admin pages at `/admin/users` and `/admin/roles`.

---

## Switching to a real database

EF Core ships with the InMemory provider for zero-friction local runs. To switch:

1. Add the provider package you want (`Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, etc.) to `AspireApp.DataAccess.Implementations`.
2. Edit `AspireApp.DataAccess.Implementations/DependencyInjection.cs` and replace `UseInMemoryDatabase` with `UseSqlServer` / `UseNpgsql` / etc.
3. In `AspireApp.AppHost/Program.cs`, add the database resource (`builder.AddSqlServer("db")` or `builder.AddPostgres("db")`) and reference it from `api` with `.WithReference(db).WaitFor(db)`.
4. Create EF Core migrations (`dotnet ef migrations add Initial -p AspireApp.DataAccess.Implementations -s AspireApp.Api`) and replace `EnsureCreatedAsync` in `DbSeeder` with `MigrateAsync`.

---

## Scaffolding a new entity

The CLI scaffolder generates every layer + wires DI + adds a NavMenu link for a new entity in a single run:

```pwsh
dotnet run --project AspireApp.Tools.Generator -- generate Order `
  --id long `
  --prop "Total:decimal:required" `
  --prop "Notes:string" `
  --prop "PlacedAt:DateTime:required" `
  --yes
```

It is idempotent: shared files (DI, DbContext, NavMenu) are mutated only when the entry is missing. See [AspireApp.Tools.Generator/README.md](AspireApp.Tools.Generator/README.md) for the full command reference, available tokens and a description of the generated Blazor pages.

---

## Conventions

- `internal sealed` for concrete service implementations; only contracts are `public`.
- `CancellationToken ct` on every `async` public method (no defaults).
- Exceptions propagate to `GlobalExceptionHandler`; no decorative try/catch.
- Domain entities and DTOs use `MaxLength` annotations so EF Core can build proper column types.
- Roles are normalized to uppercase in the database (`NormalizedName`) for case-insensitive lookup against any provider.
- All timestamps go through `TimeProvider` (registered as `TimeProvider.System` by default) for testability.

---

## Troubleshooting

- **The API doesn’t come up because `Jwt:Key` is missing.** `AddJwtAuthentication` validates the JWT options on startup. Either set the env vars (`JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`) before running the AppHost, or leave the dev defaults in `AspireApp.Api/appsettings.json`.
- **Google login redirects with `Google did not return authentication tokens.`** Make sure `GOOGLE_CLIENT_ID` + `GOOGLE_CLIENT_SECRET` are set (or in user-secrets), and add `https://<your-host>/signin-google` to the *Authorized redirect URIs* of the OAuth client in Google Cloud Console.
- **RabbitMQ container won’t start.** Aspire forwards Docker errors directly to the dashboard. If port 5672 is busy, change the binding in `AspireApp.AppHost/Program.cs`.
- **Cache feels stale after an update.** `BaseService` invalidates by tag after `SaveChangesAsync`. If you bypass it (e.g. by calling a DA directly), invalidate the cache yourself or read with `AsNoTracking()` for one-off scenarios.
