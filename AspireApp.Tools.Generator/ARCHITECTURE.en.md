# Generator — Architecture (for devs)

> 🌐 **English** · [Español](./ARCHITECTURE.md)

Companion to [`README.en.md`](./README.en.md). The README explains **how to use** the tool; this doc explains **how the code is wired** so you can read it without getting lost.

---

## TL;DR

```
CLI args ─► Settings ─► EntitySpec ─► GenerationPlan ─► [TemplateRenderer + IFileMutator] ─► disk
            (Spectre)   (record)      (Creations +       Templates/*.scriban + token map     IO
                                       Mutators)         + idempotent file mutations
```

Single command (`generate`) that takes one entity name, asks (or accepts) a handful of properties, and writes one full CRUD slice across **Domain → Application → Infrastructure → Api → Client** plus a Blazor Index/Edit pair. Shared files (DI registrations, NavMenu, AppDbContext) get patched in-place by idempotent mutators.

---

## Repo layout (only files that matter)

```
AspireApp.Tools.Generator/
├── Program.cs                       ← Spectre.Console.Cli bootstrap; one CommandApp<GenerateCommand>
├── Commands/
│   └── GenerateCommand.cs           ← all UX: prompts, banner, preview panel, plan rendering, execution
├── Generator/                       ← pure logic, no console IO
│   ├── EntitySpec.cs                ← the immutable record describing what to generate
│   ├── PropertySpec.cs              ← one entity field (Name, Type, Required, Filterable, ShowInList, Sortable)
│   ├── PathResolver.cs              ← knows where every target file lives in the solution
│   ├── GenerationPlan.cs            ← static Build() returns the list of (Creations, Mutators) for an EntitySpec
│   ├── TemplateRenderer.cs          ← reads Templates/*.scriban, replaces {{TOKEN}} from a token map
│   ├── IconPicker.cs                ← maps entity name → Bootstrap Icons class (deterministic)
│   ├── AccentPicker.cs              ← maps entity name → Bootstrap accent (currently always "primary")
│   └── Mutators/
│       ├── IFileMutator.cs          ← interface: TargetPath + Mutate(source, entity) → MutationResult
│       ├── DiRegistrationMutator.cs ← inserts using + AddScoped/AddSingleton lines after a marker
│       ├── DbContextMutator.cs      ← inserts DbSet<> + modelBuilder.Entity<>() config in AppDbContext
│       └── NavMenuMutator.cs        ← inserts a NavLink block before </nav>
└── Templates/                       ← *.scriban text files; **NOT** real Scriban, just {{TOKEN}} substitution
    ├── Domain.Entity.scriban
    ├── Application.Model.scriban
    ├── Application.Filter.scriban   ← server-mode only
    ├── Application.Contract.scriban
    ├── Application.Service.scriban
    ├── Application.Persistence.scriban
    ├── Application.Mapper.scriban
    ├── DataAccess.scriban
    ├── Api.Controller.scriban
    ├── Client.ApiClient.scriban
    ├── Client.Index.razor.scriban       ← client-mode index
    ├── Client.Index.razor.cs.scriban
    ├── Client.IndexServer.razor.scriban ← server-mode index
    ├── Client.IndexServer.razor.cs.scriban
    ├── Client.Edit.razor.scriban
    └── Client.Edit.razor.cs.scriban
```

Templates are copied to `bin/.../Templates/` on build (see `.csproj`). `TemplateRenderer` reads them from the assembly directory at runtime, not from the project source — so changes need a rebuild.

---

## Pipeline, step by step

### 1. Bootstrap — `Program.cs`
Sets UTF-8 console, builds a Spectre `CommandApp<GenerateCommand>`, runs. That's it.

### 2. Discover the solution root — `PathResolver.Discover()`
Walks **up** from CWD looking for any `*.slnx`. If `--root` was passed, skips the walk. Throws if not found. Every target file path the generator writes flows through this object — there are no string concatenations of paths elsewhere.

### 3. Build the spec — `GenerateCommand.BuildEntitySpec(settings)`
Either fully interactive (Spectre prompts) or fully driven by CLI flags. The output is one `EntitySpec` record with everything decided. Key resolvers:

- `ResolveFilterMode` — `client` (default) or `server`. Determines whether the Index does in-browser filtering or talks to a paged endpoint.
- `ResolvePageSize` — server-mode only; defaults to 25.
- `PropertySpec.Parse` for the `--prop` form, or per-field prompts (Required, Filterable, ShowInList, Sortable) interactively.

### 4. Build the plan — `GenerationPlan.Build(entity, paths)`
Pure function. Returns a record with two lists:

- **Creations**: `(TargetPath, TemplateName, FriendlyLabel)`. Every file the generator will write (skipped if it exists).
- **Mutators**: list of `IFileMutator`. Every shared file that needs an idempotent patch.

This is where the **conditional templates** live (server-mode adds `Application.Filter.scriban` and swaps the Index templates) — `GenerationPlan.Build` is the single source of truth for "which files for this config".

### 5. Render the preview — `RenderEntitySummary` + `RenderPlan`
All Spectre. Two panels (entity summary + plan tree grouped by layer). If not `--yes` and not `--dry-run`, prompts to confirm.

### 6. Execute — `ExecutePlanAsync`
Two passes:

- **Creations**: for each, `renderer.Render(templateName, entity)` → if file exists, skip. Otherwise create dirs and write. In `--dry-run`, prints the path but doesn't write.
- **Mutators**: for each, read source → `mutator.Mutate(source, entity)` → if `Changed`, write back. If it can't find its anchor, it surfaces an error and continues with the rest (counted in `totals.Failed`).

### 7. Summary — `RenderResult`
Counts of created / mutated / skipped / failed in a panel, plus a "next steps" footer (build, migration, run).

---

## Core types (cheat sheet)

| Type | Role |
| --- | --- |
| `EntitySpec` | Immutable record. Everything the generator needs in one place: name, id type, properties, flags (Blazor/Nav/Auth/EventBus), `FilterMode`, `PageSize`, icon/accent overrides. Derived helpers: `Lower`, `Camel`, `Plural`, `IsServerFiltering`, `FilterableProperties`, `ListProperties`, `SortableProperties`. |
| `PropertySpec` | One field. `Name`, `Type` (normalized), `Required`, `Filterable`, `ShowInList`, `Sortable`. Knows its type bucket via `IsString` / `IsBool` / `IsNumeric` / `IsDateTime` / `IsGuid` and its Razor input component (`InputText`, `InputNumber`, etc.). `Parse(raw)` handles the `--prop "Name:type:flag:flag"` CLI shape. |
| `PathResolver` | Single owner of "where does file X go for entity Y?". `Combine(...)` is the only place that joins solution-relative path segments. Add new file destinations here. |
| `GenerationPlan` | Returned by `Build(entity, paths)`. Holds `Creations` + `Mutators`. Conditionals (server-mode extras, Blazor on/off, NavMenu on/off) live here. |
| `TemplateRenderer` | Reads a `.scriban` file once per render call, builds the token map for the entity, runs sequential `StringBuilder.Replace("{{KEY}}", value)`. |
| `IFileMutator` / `MutationResult` | Tiny contract for shared-file patches. Mutate is **pure** (string in, string out) — actual write happens in `GenerateCommand`. |

---

## How templates work (it's NOT Scriban)

Files have a `.scriban` extension by convention but the renderer is dead simple — no AST, no expressions, no loops:

```csharp
foreach (var (key, value) in tokens)
    sb.Replace("{{" + key + "}}", value);
```

That's the whole engine. **Tokens are plain `{{KEY}}` strings**, no `{{ for x in ... }}` or `{{ if foo }}`. Everything that varies per entity is built ahead of time into a string by a `BuildXxx(entity)` method in `TemplateRenderer`, then dropped in as a token.

### The two-phase trick

Most generated files have:
1. **A template skeleton** — the bits that never change shape (namespace, class header, base class).
2. **Body-builder tokens** — `{{PROPS_ENTITY}}`, `{{PERSISTENCE_BODY}}`, `{{DA_BODY}}`, `{{SERVICE_BODY}}`, etc. — multi-line strings built by C# methods.

So `Application.Persistence.scriban` is just:
```
{{PERSISTENCE_USINGS}}using AspireApp.Application.Persistence.Base;
using AspireApp.Domain.Entities;

namespace AspireApp.Application.Persistence;

public interface I{{ENTITY}}DA : IBaseDA<{{ENTITY}}, {{ID_TYPE}}>
{
{{PERSISTENCE_BODY}}
}
```

…and `BuildPersistenceBody(entity)` either returns `""` (client mode) or a `GetPagedAsync(...)` method signature (server mode). Same template, two outputs.

### When to add a new template file vs a new token

- **New file**: only if the file is structurally different and conditionally generated. We have two Index razor templates (`Client.Index.razor` vs `Client.IndexServer.razor`) because the markup diverges too much for a token to express cleanly.
- **New token**: anything that's a chunk of text inside an otherwise-stable file. Strongly preferred — keeps the template count low.

### Token map cheat sheet

Defined in `TemplateRenderer.BuildTokenMap`. Naming convention:

- `ENTITY`, `ID_TYPE`, etc. — scalar identity tokens.
- `PROPS_*` — per-property lists (entity props, model props, table head, filter fields, etc.).
- `*_USINGS` — extra `using` directives for that file (server-mode usually adds `using AspireApp.Domain.Paging;`).
- `*_BODY` — the main body content of the file (empty `;\n` for client mode, full implementation for server mode).
- `AUTHORIZE_*`, `EVENT_BUS_*` — conditional fragments toggled by entity flags.

If a token doesn't appear in the map, `{{IT}}` is left **literally in the output**. Always wire a token in `BuildTokenMap` before using it in a template.

---

## How mutators work

`IFileMutator` exists because some files are **shared across entities** — DI containers, the DbContext, NavMenu — and a fresh write would clobber existing entities. So instead of templating them, we read them, patch them, write them.

Three implementations cover everything:

### `DiRegistrationMutator`
Generic: takes a list of `usingLines`, a `registrationLine` (e.g. `services.AddScoped<IOrderService, OrderService>();`), and a `markerForLastRegistration` (a substring like `"services.AddScoped<I"`). Idempotent — if the registration line already exists, no-op.

Where it's wired: `GenerationPlan.Build` constructs **one per DI file** (DataAccessDI, ApplicationDI, MappersDI, ClientProgram).

### `DbContextMutator`
- Inserts `public DbSet<X> Xs => Set<X>();` after the last existing DbSet.
- If the entity has string properties, also inserts a `modelBuilder.Entity<X>(entity => { ... });` block before the closing brace of `OnModelCreating`. Uses a brace-depth counter to find the matching close brace.

### `NavMenuMutator`
Looks for `href="entityname"` to detect existing entries (idempotent), otherwise inserts a `<div class="nav-item">…<NavLink>…` block right before `</nav>`.

All three are **anchor-based**: they fail loud if their anchor is missing rather than silently dropping the change.

---

## How the layered files fit together (one entity, server mode)

```
Domain.Entity      → AspireApp.Domain.Entities/{Entity}.cs
Application.Model  → AspireApp.Application.Models/App/{Entity}.cs
Application.Filter → AspireApp.Application.Models/App/{Entity}Filter.cs   (server only)
Application.Contract / Service        ← business logic, builds predicates from Filter
Application.Persistence (I{Entity}DA) ← port, takes PagedQuery + IEnumerable<Expression<Func<...>>>
Application.Mapper                    ← {Entity}Mapper, ToModel / ToEntity / ToModelList
DataAccess ({Entity}DA)               ← adapter, EF Core LINQ implementation
Api.Controller                        ← REST surface, POST /api/{entity}/query for server mode
Client.ApiClient                      ← typed HTTP wrapper, GetPagedAsync(filter, ct) extra in server mode
Client.Index.razor + .razor.cs        ← Blazor page (client or server variant)
Client.Edit.razor  + .razor.cs        ← Blazor page (single shared template)
```

For client mode, drop `Application.Filter` and swap the Index variant.

---

## Recipes

### I want to add a new property type
1. `PropertySpec.NormalizeType` — map the user-facing name to the canonical C# type.
2. `PropertySpec` getters — add to whichever of `IsString` / `IsBool` / `IsNumeric` / `IsDateTime` / `IsGuid` makes sense (or add a new bucket).
3. `PropertySpec.RazorInputComponent` — Razor `InputXxx` to use in `Client.Edit.razor`.
4. `TemplateRenderer.BuildFilterFields` / `BuildFilterDtoProps` / `AppendServicePredicate` — emit the smart filter shape (range, dropdown, etc.) for the new bucket.

### I want to add a new generated file
1. `PathResolver` — add a method returning its target path.
2. `Templates/` — drop the new `.scriban` file.
3. `GenerationPlan.Build` — add a `new FileCreation(...)` (wrap in `if (entity.IsServerFiltering)` etc. if conditional).
4. Run the generator with `--dry-run` to confirm it appears in the plan tree.

### I want to add a new token
1. `TemplateRenderer.BuildTokenMap` — add `["MY_TOKEN"] = BuildMyToken(entity)`.
2. Implement `BuildMyToken(EntitySpec entity)` — return a `string`. Use a `StringBuilder` if multi-line.
3. Add `{{MY_TOKEN}}` to whichever template needs it. Rebuild.

### I want to add a new shared-file patch
1. Implement `IFileMutator` (existing ones are 40–115 LOC; copy the closest fit).
2. Anchor on **a stable substring** in the target file. Never use line numbers.
3. Add an idempotency check (early-return with `Changed: false` if the patch is already there).
4. Wire it in `GenerationPlan.Build`'s `mutators` list.

### I want to change the CLI surface
- All flags live in `GenerateCommand.Settings` as `[CommandOption]` properties. Resolve them inside `BuildEntitySpec` (with an interactive fallback if it's a meaningful choice).

### I want to see what would be generated without writing
- `--dry-run`. Hits every code path except the actual `File.WriteAllTextAsync` and mutator writes.

---

## Conventions

- **Pure logic in `Generator/`, console UX in `Commands/`.** Don't print to `AnsiConsole` from `TemplateRenderer` or `EntitySpec`. The renderer/spec must be unit-testable headless.
- **`PathResolver` owns paths.** No string-concat path math anywhere else.
- **Mutators are idempotent.** Re-running the generator for the same entity must be a no-op on shared files.
- **Templates are dumb.** All logic in C#; templates are skeletons. If you find yourself wanting `{{ if }}` in a template, build the variant in a body-builder method instead and emit it as a token.
- **No partial state on failure.** A mutator that can't find its anchor fails its single file and is reported in `totals.Failed`; the rest of the plan still runs.
