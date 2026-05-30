# AspireApp.Tools.Generator

> 🌐 [Español](./README.md) · **English**

CLI for scaffolding entities in the AspireApp solution. Generates every layer (Domain, Application, Infrastructure, API, Client) and patches the shared files (DI, AppDbContext, NavMenu, Program.cs) in a single run. The generated Blazor screens use **Bootstrap 5 + Bootstrap Icons** with smart per-type filters, sortable columns, optionally hidden table columns, and server-side pagination when `server` filter mode is chosen.

> 🛠 **Going to modify the generator code?** Start with [`ARCHITECTURE.en.md`](./ARCHITECTURE.en.md) — it covers the pipeline, file layout, conventions, and recipes for common cases (adding a type, a template, a token, a mutator).

## Usage

### Interactive mode

Without arguments it walks you through every choice:

```pwsh
dotnet run --project AspireApp.Tools.Generator
```

- **Name and Id type** first (the name is normalized to PascalCase automatically, e.g. `customer` → `Customer`).
- **Property editor** with a live table and an **Add / Edit / Remove / Done** menu: you add fields, and if you got something wrong (type, required, filter, …) you **edit that row** instead of starting over. Per field you pick a name, a type and — in a single multi-select — the **Required / Filterable / Show in table / Sortable** options (with sensible defaults pre-checked based on the type).
- Then: Blazor pages, NavLink, event bus and **client/server** filter mode (plus page size in server mode).

### Non-interactive mode

```pwsh
dotnet run --project AspireApp.Tools.Generator -- generate Order `
  --id long `
  --prop "Total:decimal:required:filter:sort" `
  --prop "Notes:string:hidden" `
  --prop "PlacedAt:DateTime:required:filter:sort" `
  --filter-mode server `
  --page-size 25 `
  --event-bus `
  --yes
```

### Options

| Option                | Description                                                                                            |
| --------------------- | ------------------------------------------------------------------------------------------------------ |
| `ENTITY_NAME`         | Singular PascalCase entity name (e.g. `Order`).                                                        |
| `--id <ID_TYPE>`      | Id type: `long`, `int` or `Guid`. Defaults to `long`.                                                  |
| `-p, --prop <PROP>`   | Property in the form `Name:type[:flag1[:flag2...]]`. Repeatable. Flags listed below.                   |
| `--icon <ICON>`       | Override the Bootstrap Icon class (without the `bi-` prefix). Auto-detected from the name otherwise.   |
| `--accent <ACCENT>`   | Override the Bootstrap accent: `primary`, `success`, `info`, `warning`, `danger`, `secondary`.         |
| `--filter-mode <M>`   | `client` (default) or `server`. Server adds a Filter DTO, `POST /query` endpoint and full pagination.  |
| `--page-size <N>`     | Default page size for server mode. Defaults to 25.                                                     |
| `--no-ui`             | Skip the Blazor pages (`Index` + `Edit`). Asked interactively.                                         |
| `--no-nav`            | Skip adding the `NavLink` to `NavMenu.razor`.                                                          |
| `--no-auth`           | Skip decorating the controller with `[Authorize]`.                                                     |
| `--event-bus`         | Inject `IMessageBus` and publish an event when a new instance is created. Asked interactively.         |
| `--dry-run`           | Show the plan without writing anything to disk.                                                        |
| `-y, --yes`           | Run non-interactively, skip confirmation prompts.                                                      |
| `--root <PATH>`       | Explicit path to the solution root (auto-discovered by default).                                       |

### Property flags (`--prop "Name:type:flag1:flag2..."`)

| Flag                                 | Effect                                                                                  |
| ------------------------------------ | --------------------------------------------------------------------------------------- |
| `required`                           | `[Required]` on the model + `IsRequired()` in EF + asterisk in the form.                |
| `filter` / `filterable`              | Appears in the **Filters** card on the Index with a type-appropriate control.           |
| `nofilter`                           | Hidden from filters (default for non-strings).                                          |
| `hidden` / `hide` / `nolist`         | Hidden from the Index **table** (still shows up in the Edit form).                      |
| `list`                               | Visible in the table (default).                                                         |
| `sort` / `sortable`                  | Click-to-sort column header in the table (default).                                     |
| `nosort`                             | Column is not sortable.                                                                 |

Defaults: `required=false`, `filterable=true` only for strings, `showInList=true`, `sortable=true`. In interactive mode these are toggled per field in a multi-select (with those defaults pre-checked) and remain editable afterwards from the property menu.

### Supported property types

`string`, `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `bool`, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`.

## What gets generated

For an `Order` entity with `long` Id:

**Files always created (up to 13):**

- `AspireApp.Domain.Entities/Order.cs`
- `AspireApp.Application.Models/App/Order.cs`
- `AspireApp.Application.Contracts/Order/IOrderService.cs`
- `AspireApp.Application.Implementations/Order/OrderService.cs`
- `AspireApp.Application.Mappers/OrderMapper.cs`
- `AspireApp.Application.Persistence/IOrderDA.cs`
- `AspireApp.DataAccess.Implementations/OrderDA.cs`
- `AspireApp.Api/Controllers/OrderController.cs`
- `AspireApp.Client.ApiClients/OrderApiClient.cs`
- `AspireApp.Client/Components/Pages/OrderIndex.razor` *(if UI)*
- `AspireApp.Client/Components/Pages/OrderIndex.razor.cs` *(if UI)*
- `AspireApp.Client/Components/Pages/OrderEdit.razor` *(if UI)*
- `AspireApp.Client/Components/Pages/OrderEdit.razor.cs` *(if UI)*

**Extra files in `server` mode:**

- `AspireApp.Application.Models/App/OrderFilter.cs` — filter + paging + sort DTO (inherits `PagedQuery`).
- `IOrderDA`, `OrderDA`, `IOrderService`, `OrderService`, `OrderController` and `OrderApiClient` gain a `GetPagedAsync({Entity}Filter, ct)` method using LINQ-to-EF (Skip/Take, Count, OrderBy/Descending per column).
- The exposed endpoint is `POST /api/{entity}/query` with the filter in the body.

### Blazor screens

**Index** (`/order`):

- **Simple header**: icon + title + **Refresh** and **New** buttons.
- **Filters card** with smart per-type controls:
  - `string` → text input "contains".
  - `bool` → dropdown **All / Yes / No**.
  - numerics → two inputs **min – max**.
  - dates → two inputs **from – to**.
  - only for fields marked as **filterable**.
- **Simple table** with a **sortable** header (button with arrow that toggles direction on click).
- **Type-aware rendering in cells**: `bool` → "Yes/No", `DateTime` → `yyyy-MM-dd`, numerics → right-aligned monospace, `Email` → `mailto:`, `Phone`/`Mobile` → `tel:`. Id rendered as grey monospace.
- Columns flagged `hidden`/`nolist` don't appear in the table but remain editable.
- **Row buttons**: pencil (Edit) + trash (Delete).
- **Empty state**: large `bi-inbox` icon + **Clear filters** and **Create first** buttons.
- **Loading state**: centered spinner.
- **`client` mode** (default): pulls everything with `GetAllAsync`, filters and sorts in the browser, shows `N filtered of Total`.
- **`server` mode**: every filter/sort/page change fires `POST /api/{entity}/query`. Pagination shown below the table (first/prev/page indicator/next/last + page size selector `10/25/50/100`) plus a `X–Y of N` label.

**Edit / Detail** (`/order/new` or `/order/edit/{id}`):

- **Simple header**: back arrow + icon + dynamic title + `#id` badge (edit only) + **Delete** button (edit only).
- **Card** wrapping the form.
- **EditForm** with `DataAnnotationsValidator` + `ValidationSummary` + a `ValidationMessage` per field.
- **Simple inputs** (`InputText`, `InputNumber`, `InputDate`). Booleans as switches.
- **Save button** with spinner + Cancel button.

### Contextual icons and accent

The generator picks a Bootstrap Icon that fits your entity (by keyword: `Order → receipt`, `User → person-circle`, `Product → box-seam`, …). You can force one with `--icon`. The default accent is `primary` to keep the app visually cohesive; override it with `--accent`. `danger` is reserved for destructive actions.

### Modified files (idempotent, no duplication)

- `AspireApp.DataAccess.Implementations/AppDbContext.cs` — adds `DbSet<Order>` and EF config for strings (MaxLength, IsRequired).
- `AspireApp.DataAccess.Implementations/DependencyInjection.cs` — `IOrderDA → OrderDA`.
- `AspireApp.Application.Implementations/DependencyInjection.cs` — `using`s + `IOrderService → OrderService`.
- `AspireApp.Application.Mappers/DependencyInjection.cs` — registers `OrderMapper`.
- `AspireApp.Client/Program.cs` — registers `OrderApiClient`.
- `AspireApp.Client/Components/Layout/NavMenu.razor` — adds a `NavLink` pointing to `/order` with the contextual icon.

## CLI experience

The terminal shows (all in Spanish in the live UI, with consistent iconography):

1. **Figlet "AspireApp" banner** + tagline with the layer chain (`Domain › Application › Infra › Api › Client`).
2. **Context bar** with `▸ Root`, `▸ Modo` (APPLY or DRY-RUN) and, if applicable, `▸ Prompts desactivados`.
3. **"Entity preview" panel**: detected icon + name + plural, `# Id type`, `● Acento`, `▣ Blazor UI`, `▸ NavMenu`, `★ Authorize`, `✉ Event bus`, `⛃ Filtrado` (client / server + pageSize). Then a **properties table** (`# / Nombre / Tipo / Req / Filtra / Lista / Ordena`).
4. **Plan tree** (`◈ Plan de generación`) with the files to create grouped by layer.
5. **Status spinner** during creation with colored layer tags and prefixes `✚` (created) / `○` (skipped) / `✎` (mutated) / `✗` (error).
6. **`▣ Resumen` panel** with totals.
7. **`❯❯ Próximos pasos` section** with suggested commands.

## Idempotency

If the entity already exists, existing files are skipped and shared files aren't duplicated. You can re-run safely.

## Templates

Templates live in [`Templates/`](./Templates/) as `*.scriban` (plain text with `{{TOKEN}}` placeholders). Available tokens:

| Token                          | Example (`Order`) / description                                          |
| ------------------------------ | ------------------------------------------------------------------------ |
| `{{ENTITY}}`                   | `Order`                                                                  |
| `{{entity}}`                   | `order`                                                                  |
| `{{ENTITY_CAMEL}}`             | `order`                                                                  |
| `{{ENTITY_PLURAL}}`            | `Orders`                                                                 |
| `{{entity_plural}}`            | `orders`                                                                 |
| `{{ID_TYPE}}`                  | `long`                                                                   |
| `{{ID_ROUTE_CONSTRAINT}}`      | `:long`, `:int`, `:guid` or empty                                        |
| `{{ENTITY_ICON}}`              | `receipt` (Bootstrap Icons, without `bi-`)                               |
| `{{ACCENT}}`                   | `primary`, `success`, …                                                  |
| `{{ACCENT_SUBTLE}}`            | `primary-subtle`, …                                                      |
| `{{PAGE_SIZE}}`                | `25`                                                                     |
| `{{PROPS_ENTITY}}`             | Entity properties                                                        |
| `{{PROPS_MODEL}}`              | Properties + DataAnnotations                                             |
| `{{PROPS_DBCONFIG}}`           | `entity.Property(...).HasMaxLength(...).IsRequired();` lines             |
| `{{PROPS_MAPPER_TO_MODEL}}`    | Mapper lines toward the model                                            |
| `{{PROPS_MAPPER_TO_ENTITY}}`   | Mapper lines toward the entity                                           |
| `{{PROPS_FORM_FIELDS}}`        | `<InputText>`, `<InputNumber>`… for the Edit form                        |
| `{{PROPS_TABLE_HEAD}}`         | `<th>` with sort button and arrow for sortable columns                   |
| `{{PROPS_TABLE_BODY}}`         | Per-type `<td>`, only for `ShowInList` properties                        |
| `{{PROPS_FILTER_FIELDS}}`      | Smart per-type inputs, bound appropriately per mode                      |
| `{{PROPS_FILTER_STATE}}`       | `filter_x*` fields (client mode)                                         |
| `{{PROPS_FILTER_LOGIC}}`       | LINQ of `ApplyFilter` (client mode)                                      |
| `{{PROPS_RESET_LOGIC}}`        | Resets the `filter_x*` (client mode)                                     |
| `{{PROPS_SORT_CASES}}`         | Switch cases for `ApplySort` (client mode)                               |
| `{{PROPS_FILTER_DTO}}`         | Properties of the `{Entity}Filter` (server mode)                         |
| `{{AUTHORIZE_ATTR}}`           | `[Authorize]\n` or empty                                                 |
| `{{AUTHORIZE_USING}}`          | Matching using directive                                                 |
| `{{EVENT_BUS_USING}}`          | `using AspireApp.Application.Contracts.EventBus;` or empty               |
| `{{EVENT_BUS_CTOR_PARAM}}`     | `, IMessageBus messageBus` or empty                                      |
| `{{EVENT_BUS_BASE_ARG}}`       | `, messageBus` or empty                                                  |
| `{{DISPLAY_NAME_EXPR}}`        | Expression that returns a "display name" used in headers                 |
| `{{PERSISTENCE_USINGS}}`       | Extra usings for `I{Entity}DA` (server mode)                             |
| `{{PERSISTENCE_BODY}}`         | Extra body of the interface (server mode)                                |
| `{{DA_USINGS}}`                | Extra usings for `{Entity}DA` (server mode)                              |
| `{{DA_BODY}}`                  | `;\n` (client) or body with `GetPagedAsync` (server)                     |
| `{{CONTRACT_USINGS}}`          | Extra usings for `I{Entity}Service`                                      |
| `{{CONTRACT_BODY}}`            | Extra body of the interface                                              |
| `{{SERVICE_USINGS}}`           | Extra usings for `{Entity}Service`                                       |
| `{{SERVICE_BODY}}`             | `;\n` (client) or body (server)                                          |
| `{{CONTROLLER_EXTRA_USINGS}}`  | Extra usings for the controller                                          |
| `{{CONTROLLER_BODY}}`          | `;\n` (client) or body with `POST /query` action (server)                |
| `{{API_CLIENT_EXTRA_USINGS}}`  | `using AspireApp.Domain.Paging;` in server mode                          |
| `{{API_CLIENT_EXTRA_METHODS}}` | Extra `GetPagedAsync(...)` for the `ApiClient` (server mode)             |

Shared files are handled by the mutators in [`Generator/Mutators/`](./Generator/Mutators/), which detect duplicates before inserting lines.

## Shared layers (not generated) that support server-mode

So the persistence port **does not depend on the models layer** (Clean Architecture), the paging envelope lives in a dedicated Domain project:

- `AspireApp.Domain.Paging/PagedResult.cs` — immutable envelope `Items / Total / Page / PageSize` + helpers (`TotalPages`, `HasPrevious`, `HasNext`).
- `AspireApp.Domain.Paging/PagedQuery.cs` — base with `Page`, `PageSize`, `SortBy`, `SortDir` (`SortDirection.Asc|Desc`) and `Normalize()`.
- `AspireApp.Application.Models` references `AspireApp.Domain.Paging` so the generated `{Entity}Filter` can inherit `PagedQuery`. If no server-mode entity exists yet this reference may be absent — the first server-mode entity you generate requires adding `<ProjectReference Include="..\AspireApp.Domain.Paging\AspireApp.Domain.Paging.csproj" />` to `AspireApp.Application.Models.csproj`.
- `AspireApp.Domain.ROP` stays dedicated to Railway-Oriented Programming only (`Result<T>`, `Unit`).

### Server-mode flow (UI to DB)

```
Blazor Index ──► {Entity}ApiClient.GetPagedAsync(filter)
                  │  POST /api/{entity}/query  (body: {Entity}Filter)
                  ▼
              {Entity}Controller.GetPaged(filter)
                  │
                  ▼
              {Entity}Service.GetPagedAsync(filter)
                  │  builds List<Expression<Func<{Entity}, bool>>> from
                  │  the filter fields (string Contains, range, etc.)
                  ▼
              {Entity}DA.GetPagedAsync(query: PagedQuery,
                                       predicates: IEnumerable<Expression<Func<{Entity},bool>>>,
                                       ct)
                  │  EF Core: AsNoTracking → predicates.Aggregate(Where) →
                  │           OrderBy(query.SortBy) → Skip/Take → CountAsync
                  ▼
              PagedResult<{Entity}> ──► Service maps to PagedResult<{Entity}Model>
```

**Why this layout (Clean Architecture):**

- `AspireApp.Application.Persistence` (the ports / interfaces `I{Entity}DA`) only depends on Domain (`Domain.ROP` + `Domain.Paging` + `Domain.Entities`). **It does not know about `{Entity}Filter`** or anything in Application.Models.
- The `{Entity}Filter` lives in `AspireApp.Application.Models.App` (it's a transport DTO for UI/API). The Service receives it and translates it to `List<Expression<Func<TEntity, bool>>>` before crossing the port into the DA.
- The DA implements the method directly with LINQ-to-EF. Translating `query.SortBy` (a string) to `OrderBy(x => x.Foo)` happens in a per-entity `switch` inside the implementation.
