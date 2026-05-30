# AspireApp.Tools.Generator

> 🌐 **Español** · [English](./README.en.md)

CLI para scaffolding de entidades en la solución AspireApp. Genera todos los archivos de las capas (Domain, Application, Infrastructure, API, Client) y modifica los archivos compartidos (DI, AppDbContext, NavMenu, Program.cs) en una sola corrida. Las pantallas Blazor generadas usan **Bootstrap 5 + Bootstrap Icons** con filtros inteligentes por tipo, columnas ordenables, opcionalmente columnas ocultas en la tabla, y soporte de paginación server-side cuando se elige el modo `server`.

> 🛠 **¿Vas a tocar el código del generator?** Empezá por [`ARCHITECTURE.md`](./ARCHITECTURE.md) — tiene el pipeline, el layout de archivos, las convenciones, y recetas para casos comunes (agregar un tipo, un template, un token, un mutator).

## Uso

### Modo interactivo

Sin argumentos te pregunta paso a paso (incluye, por cada campo, si es filtrable, si se muestra en la tabla y si la columna es ordenable; al final, modo de filtrado client/server):

```pwsh
dotnet run --project AspireApp.Tools.Generator
```

### Modo no-interactivo

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

### Opciones

| Opción                | Descripción                                                                                            |
| --------------------- | ------------------------------------------------------------------------------------------------------ |
| `ENTITY_NAME`         | Nombre PascalCase singular de la entidad (e.g. `Order`).                                               |
| `--id <ID_TYPE>`      | Tipo del Id: `long`, `int` o `Guid`. Default `long`.                                                   |
| `-p, --prop <PROP>`   | Propiedad en formato `Name:type[:flag1[:flag2...]]`. Repetible. Flags abajo.                           |
| `--icon <ICON>`       | Override del Bootstrap Icon (sin el prefijo `bi-`). Auto-detectado por nombre.                         |
| `--accent <ACCENT>`   | Override del color de acento Bootstrap: `primary`, `success`, `info`, `warning`, `danger`, `secondary`.|
| `--filter-mode <M>`   | `client` (default) o `server`. Server agrega Filter DTO, endpoint POST `/query` y paginación completa. |
| `--page-size <N>`     | Tamaño de página por defecto (server-mode). Default 25.                                                |
| `--no-ui`             | No generar las pantallas Blazor (`Index` + `Edit`). En modo interactivo se pregunta.                   |
| `--no-nav`            | No agregar el `NavLink` a `NavMenu.razor`.                                                             |
| `--no-auth`           | No decorar el controller con `[Authorize]`.                                                            |
| `--event-bus`         | Inyectar `IMessageBus` y publicar un evento al crear una nueva instancia. En interactivo se pregunta.  |
| `--dry-run`           | Mostrar el plan sin tocar nada en disco.                                                               |
| `-y, --yes`           | Ejecutar sin pedir confirmación.                                                                       |
| `--root <PATH>`       | Path explícito a la raíz de la solución (auto-detectado por defecto).                                  |

### Flags de propiedad (`--prop "Name:type:flag1:flag2..."`)

| Flag                                 | Efecto                                                                                  |
| ------------------------------------ | --------------------------------------------------------------------------------------- |
| `required`                           | `[Required]` en el model + `IsRequired()` en EF + asterisco en el form.                 |
| `filter` / `filterable`              | Aparece en la card de **Filtros** del Index con un control acorde al tipo.              |
| `nofilter`                           | No aparece en filtros (default para no-strings).                                        |
| `hidden` / `hide` / `nolist`         | No aparece en la **tabla** del Index (sigue en el form de edición).                     |
| `list`                               | Aparece en la tabla (default).                                                          |
| `sort` / `sortable`                  | Columna ordenable click-to-sort en la tabla (default).                                  |
| `nosort`                             | Columna no ordenable.                                                                   |

Defaults: `required=false`, `filterable=true` sólo para strings, `showInList=true`, `sortable=true`. En modo interactivo se preguntan estas opciones por cada campo (defaults sensatos pre-seleccionados).

### Tipos de propiedad soportados

`string`, `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `bool`, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`.

## Qué genera

Para una entidad `Order` con Id `long`:

**Archivos siempre creados (hasta 13):**

- `AspireApp.Domain.Entities/Order.cs`
- `AspireApp.Application.Models/App/Order.cs`
- `AspireApp.Application.Contracts/Order/IOrderService.cs`
- `AspireApp.Application.Implementations/Order/OrderService.cs`
- `AspireApp.Application.Mappers/OrderMapper.cs`
- `AspireApp.Application.Persistence/IOrderDA.cs`
- `AspireApp.DataAccess.Implementations/OrderDA.cs`
- `AspireApp.Api/Controllers/OrderController.cs`
- `AspireApp.Client.ApiClients/OrderApiClient.cs`
- `AspireApp.Client/Components/Pages/OrderIndex.razor` *(si UI)*
- `AspireApp.Client/Components/Pages/OrderIndex.razor.cs` *(si UI)*
- `AspireApp.Client/Components/Pages/OrderEdit.razor` *(si UI)*
- `AspireApp.Client/Components/Pages/OrderEdit.razor.cs` *(si UI)*

**Archivos extra en modo `server`:**

- `AspireApp.Application.Models/App/OrderFilter.cs` — DTO de filtros + paging + sort (hereda `PagedQuery`).
- El `IOrderDA`, `OrderDA`, `IOrderService`, `OrderService`, `OrderController` y `OrderApiClient` ganan un método `GetPagedAsync({Entity}Filter, ct)` con LINQ-to-EF (Skip/Take, Count, OrderBy/Descending por columna).
- El endpoint expuesto es `POST /api/{entity}/query` con el filtro en el body.

### Pantallas Blazor

**Index** (`/order`):

- **Header simple**: icono + título + botones **Refrescar** y **Nuevo**.
- **Card de filtros** con controles inteligentes por tipo de dato:
  - `string` → input de texto "contiene".
  - `bool` → dropdown **Todos / Sí / No**.
  - numéricos → dos inputs **mín – máx**.
  - fechas → dos inputs **desde – hasta**.
  - sólo para los campos marcados como **filtrables**.
- **Tabla** simple con cabecera **ordenable** (botón con flecha que cambia de dirección al click).
- **Renderizado por tipo en la tabla**: `bool` → "Sí/No", `DateTime` → `yyyy-MM-dd`, numéricos → monospace alineado a la derecha, `Email` → `mailto:`, `Phone`/`Mobile` → `tel:`. Id en monospace gris.
- Las columnas con flag `hidden`/`nolist` no aparecen en la tabla pero siguen disponibles para edición.
- **Botonera por fila**: lápiz (Editar) + tacho (Borrar).
- **Empty state**: icono `bi-inbox` grande + botones **Limpiar filtros** y **Crear primero**.
- **Loading state**: spinner centrado.
- **Modo `client`** (default): carga todo con `GetAllAsync`, filtra y ordena en el navegador, muestra `N filtrados de Total`.
- **Modo `server`**: cada cambio de filtro/orden/página dispara `POST /api/{entity}/query`. Paginación visible debajo de la tabla (primera/anterior/indicador/siguiente/última + selector de tamaño de página `10/25/50/100`) y label `X–Y de N`.

**Edit / Detail** (`/order/new` o `/order/edit/{id}`):

- **Header simple**: flecha de volver + icono + título dinámico + badge `#id` (sólo en edición) + botón **Borrar** (sólo en edición).
- **Card** con el formulario.
- **EditForm** con `DataAnnotationsValidator` + `ValidationSummary` + un `ValidationMessage` por campo.
- **Inputs simples** (`InputText`, `InputNumber`, `InputDate`). Booleans como switch.
- **Botón Guardar** con spinner + botón Cancelar.

### Iconos contextuales y acento

El generador detecta un Bootstrap Icon adecuado para tu entidad (por keyword: `Order → receipt`, `User → person-circle`, `Product → box-seam`, etc.). Se puede forzar con `--icon`. El acento por defecto es `primary` para mantener cohesión visual; podés overridear con `--accent`. `danger` se reserva para acciones destructivas.

### Archivos modificados (idempotente, no duplica)

- `AspireApp.DataAccess.Implementations/AppDbContext.cs` — agrega `DbSet<Order>` y configuración EF para strings (MaxLength, IsRequired).
- `AspireApp.DataAccess.Implementations/DependencyInjection.cs` — `IOrderDA → OrderDA`.
- `AspireApp.Application.Implementations/DependencyInjection.cs` — `using`s + `IOrderService → OrderService`.
- `AspireApp.Application.Mappers/DependencyInjection.cs` — registra `OrderMapper`.
- `AspireApp.Client/Program.cs` — registra `OrderApiClient`.
- `AspireApp.Client/Components/Layout/NavMenu.razor` — agrega `NavLink` apuntando a `/order` con el icono contextual.

## Experiencia CLI

La terminal muestra (todo en español, con iconografía consistente):

1. **Banner Figlet "AspireApp"** + tagline con la cadena de capas (`Domain › Application › Infra › Api › Client`).
2. **Context bar** con `▸ Root`, `▸ Modo` (APPLY o DRY-RUN) y, si aplica, `▸ Prompts desactivados`.
3. **Panel "Entity preview"**: icono detectado + nombre + plural, `# Id type`, `● Acento`, `▣ Blazor UI`, `▸ NavMenu`, `★ Authorize`, `✉ Event bus`, `⛃ Filtrado` (client / server + pageSize). Luego una **tabla de propiedades** (`# / Nombre / Tipo / Req / Filtra / Lista / Ordena`).
4. **Tree del plan** (`◈ Plan de generación`) con los archivos a crear agrupados por capa.
5. **Status spinner** durante la creación con tag de capa coloreada y prefijos `✚` (creado) / `○` (omitido) / `✎` (mutado) / `✗` (error).
6. **Panel `▣ Resumen`** con totales.
7. **Sección `❯❯ Próximos pasos`** con comandos sugeridos.

## Idempotencia

Si la entidad ya existe, los archivos existentes se saltan y los archivos compartidos no se duplican. Podés correrlo varias veces sin riesgo.

## Templates

Los templates están en [`Templates/`](./Templates/) como `*.scriban` (texto plano con tokens `{{TOKEN}}`). Los tokens disponibles:

| Token                          | Ejemplo (`Order`) / descripción                                          |
| ------------------------------ | ------------------------------------------------------------------------ |
| `{{ENTITY}}`                   | `Order`                                                                  |
| `{{entity}}`                   | `order`                                                                  |
| `{{ENTITY_CAMEL}}`             | `order`                                                                  |
| `{{ENTITY_PLURAL}}`            | `Orders`                                                                 |
| `{{entity_plural}}`            | `orders`                                                                 |
| `{{ID_TYPE}}`                  | `long`                                                                   |
| `{{ID_ROUTE_CONSTRAINT}}`      | `:long`, `:int`, `:guid` o vacío                                         |
| `{{ENTITY_ICON}}`              | `receipt` (Bootstrap Icons, sin `bi-`)                                   |
| `{{ACCENT}}`                   | `primary`, `success`, …                                                  |
| `{{ACCENT_SUBTLE}}`            | `primary-subtle`, …                                                      |
| `{{PAGE_SIZE}}`                | `25`                                                                     |
| `{{PROPS_ENTITY}}`             | Propiedades para la entidad                                              |
| `{{PROPS_MODEL}}`              | Propiedades + DataAnnotations                                            |
| `{{PROPS_DBCONFIG}}`           | Líneas `entity.Property(...).HasMaxLength(...).IsRequired();`            |
| `{{PROPS_MAPPER_TO_MODEL}}`    | Líneas del mapper hacia model                                            |
| `{{PROPS_MAPPER_TO_ENTITY}}`   | Líneas del mapper hacia entity                                           |
| `{{PROPS_FORM_FIELDS}}`        | `<InputText>`, `<InputNumber>`… del Edit                                 |
| `{{PROPS_TABLE_HEAD}}`         | `<th>` con botón sort y flecha para propiedades ordenables               |
| `{{PROPS_TABLE_BODY}}`         | `<td>` por tipo, sólo `ShowInList`                                       |
| `{{PROPS_FILTER_FIELDS}}`      | Inputs inteligentes por tipo, bind apropiado por modo                    |
| `{{PROPS_FILTER_STATE}}`       | Campos `filter_x*` (client-mode)                                         |
| `{{PROPS_FILTER_LOGIC}}`       | LINQ del `ApplyFilter` (client-mode)                                     |
| `{{PROPS_RESET_LOGIC}}`        | Reset de los `filter_x*` (client-mode)                                   |
| `{{PROPS_SORT_CASES}}`         | Switch cases para `ApplySort` (client-mode)                              |
| `{{PROPS_FILTER_DTO}}`         | Propiedades del `{Entity}Filter` (server-mode)                           |
| `{{AUTHORIZE_ATTR}}`           | `[Authorize]\n` o vacío                                                  |
| `{{AUTHORIZE_USING}}`          | Using correspondiente                                                    |
| `{{EVENT_BUS_USING}}`          | `using AspireApp.Application.Contracts.EventBus;` o vacío                |
| `{{EVENT_BUS_CTOR_PARAM}}`     | `, IMessageBus messageBus` o vacío                                       |
| `{{EVENT_BUS_BASE_ARG}}`       | `, messageBus` o vacío                                                   |
| `{{DISPLAY_NAME_EXPR}}`        | Expresión que devuelve un "nombre visible" para los headers              |
| `{{PERSISTENCE_USINGS}}`       | Usings extra del `I{Entity}DA` (server-mode)                             |
| `{{PERSISTENCE_BODY}}`         | Cuerpo extra del interface (server-mode)                                 |
| `{{DA_USINGS}}`                | Usings extra del `{Entity}DA` (server-mode)                              |
| `{{DA_BODY}}`                  | `;\n` (client) o cuerpo con `GetPagedAsync` (server)                     |
| `{{CONTRACT_USINGS}}`          | Usings extra del `I{Entity}Service`                                      |
| `{{CONTRACT_BODY}}`            | Cuerpo extra del interface                                               |
| `{{SERVICE_USINGS}}`           | Usings extra del `{Entity}Service`                                       |
| `{{SERVICE_BODY}}`             | `;\n` (client) o cuerpo (server)                                         |
| `{{CONTROLLER_EXTRA_USINGS}}`  | Usings extra del controller                                              |
| `{{CONTROLLER_BODY}}`          | `;\n` (client) o cuerpo con action POST `/query` (server)                |
| `{{API_CLIENT_EXTRA_USINGS}}`  | `using AspireApp.Domain.Paging;` cuando server-mode                      |
| `{{API_CLIENT_EXTRA_METHODS}}` | `GetPagedAsync(...)` extra del `ApiClient` (server-mode)                 |

Los archivos compartidos los manejan los mutadores en [`Generator/Mutators/`](./Generator/Mutators/), que detectan duplicados antes de insertar líneas.

## Capas compartidas (no generadas) que sustentan server-mode

Para que el port de persistencia **no dependa de la capa de modelos** (Clean Architecture), el envelope de paging vive en un proyecto Domain dedicado:

- `AspireApp.Domain.Paging/PagedResult.cs` — envelope inmutable `Items / Total / Page / PageSize` + helpers (`TotalPages`, `HasPrevious`, `HasNext`).
- `AspireApp.Domain.Paging/PagedQuery.cs` — base con `Page`, `PageSize`, `SortBy`, `SortDir` (`SortDirection.Asc|Desc`) y `Normalize()`.
- `AspireApp.Application.Models` referencia `AspireApp.Domain.Paging` para que el `{Entity}Filter` generado pueda heredar `PagedQuery`. La primera vez que generás una entidad en server-mode, el generator agrega esa `<ProjectReference>` a `AspireApp.Application.Models.csproj` automáticamente (idempotente — corridas siguientes la detectan y la saltean). El resto de los proyectos (`Client.ApiClients`, `Client`, `Application.Implementations`, `DataAccess.Implementations`) ven `Domain.Paging` transitivamente vía esa referencia.
- `AspireApp.Domain.ROP` queda dedicado únicamente a Railway-Oriented Programming (`Result<T>`, `Unit`).

### Flujo server-mode (de UI a DB)

```
Blazor Index ──► {Entity}ApiClient.GetPagedAsync(filter)
                  │  POST /api/{entity}/query  (body: {Entity}Filter)
                  ▼
              {Entity}Controller.GetPaged(filter)
                  │
                  ▼
              {Entity}Service.GetPagedAsync(filter)
                  │  arma List<Expression<Func<{Entity}, bool>>> a partir
                  │  de los campos del filter (string Contains, range, etc.)
                  ▼
              {Entity}DA.GetPagedAsync(query: PagedQuery,
                                       predicates: IEnumerable<Expression<Func<{Entity},bool>>>,
                                       ct)
                  │  EF Core: AsNoTracking → predicates.Aggregate(Where) →
                  │           OrderBy(query.SortBy) → Skip/Take → CountAsync
                  ▼
              PagedResult<{Entity}> ──► Service mapea a PagedResult<{Entity}Model>
```

**Por qué así (Clean Architecture):**

- `AspireApp.Application.Persistence` (los ports / interfaces I{Entity}DA) sólo depende de Domain (`Domain.ROP` + `Domain.Paging` + `Domain.Entities`). **No conoce `{Entity}Filter`** ni nada de Application.Models.
- El `{Entity}Filter` vive en `AspireApp.Application.Models.App` (es un DTO de transporte para la UI/API). El Service lo recibe y traduce a `List<Expression<Func<TEntity, bool>>>` antes de cruzar el port hacia la DA.
- La DA implementa el método con LINQ-to-EF directamente. La traducción de `query.SortBy` (un string) a `OrderBy(x => x.Foo)` se hace en un `switch` per-entity adentro de la implementación.
