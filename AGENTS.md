# AGENTS.md

## Cursor Cloud specific instructions

### Overview

AspireApp is a .NET 9 Aspire-based full-stack web application following Clean Architecture. It consists of:

- **AspireApp.Api** — ASP.NET Core Web API (JWT auth, Swagger, EF Core InMemory DB)
- **AspireApp.Client** — Blazor Server frontend
- **AspireApp.AppHost** — .NET Aspire orchestrator (starts both API and Client with service discovery)

### Prerequisites

- **.NET 9 SDK** installed at `$HOME/.dotnet` with `DOTNET_ROOT` and `PATH` configured in `~/.bashrc`.
- **Aspire workload** installed via `dotnet workload install aspire`.

### Running the application

The Client uses Aspire service discovery (`https+http://api`), so you **must** run through the AppHost — not the individual projects:

```bash
export ASPIRE_ALLOW_UNSECURED_TRANSPORT=true
dotnet run --project AspireApp.AppHost --launch-profile http
```

This starts the Aspire dashboard on `http://localhost:15205`, the API on `http://localhost:5462`, and the Client on `http://localhost:5038`. The DCP (Distributed Control Plane) proxy handles port assignments matching `launchSettings.json`.

### Gotchas

- **Kafka dependency**: The `BaseController.Add` method calls `IMessageBus.PublishAsync` (currently wired to `KafkaMessageBus`). Without a Kafka broker on `localhost:29092`, POST/PUT operations on Products and Shows will hang. GET and Auth endpoints work without Kafka.
- **Redis**: Configured for `localhost:6379`. The app starts without Redis; HybridCache falls back to L1 (in-memory) only, but operations that explicitly hit Redis will fail.
- **ASPIRE_ALLOW_UNSECURED_TRANSPORT=true** is required for the `http` launch profile since the AppHost enforces HTTPS by default.

### Build / Test / Lint

- **Build**: `dotnet build AspireApp.sln`
- **Test**: `dotnet test AspireApp.sln` — 56 unit tests (MSTest + Moq), no external services needed.
- **Lint**: No dedicated linter configured; the build itself performs compiler warnings/nullable checks.

### API quick-start

1. Register: `POST /api/Auth/register` with `{"email","password","name","surname"}`
2. Login: `POST /api/Auth/login` with `{"email","password"}` → returns `{"token":"..."}`
3. Use `Authorization: Bearer <token>` header for authenticated endpoints (`/api/Product`, `/api/Show`).
