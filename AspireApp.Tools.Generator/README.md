# AspireApp.Tools.Generator

CLI para scaffolding de entidades en la solución AspireApp. Genera todos los archivos de las capas (Domain, Application, Infrastructure, API, Client) y modifica los archivos compartidos (DI, AppDbContext, NavMenu, Program.cs) en una sola corrida.

## Uso

### Modo interactivo

Sin argumentos te pregunta paso a paso:

```pwsh
dotnet run --project AspireApp.Tools.Generator
```

### Modo no-interactivo

```pwsh
dotnet run --project AspireApp.Tools.Generator -- generate Order `
  --id long `
  --prop "Total:decimal:required" `
  --prop "Notes:string" `
  --prop "PlacedAt:DateTime:required" `
  --yes
```

### Opciones

| Opción                | Descripción                                                                |
| --------------------- | -------------------------------------------------------------------------- |
| `ENTITY_NAME`         | Nombre PascalCase singular de la entidad (e.g. `Order`).                   |
| `--id <ID_TYPE>`      | Tipo del Id: `long`, `int` o `Guid`. Default `long`.                       |
| `-p, --prop <PROP>`   | Propiedad en formato `Name:type` o `Name:type:required`. Repetible.        |
| `--no-ui`             | No generar las pantallas Blazor (`Index` + `Edit`). En modo interactivo se pregunta. |
| `--no-nav`            | No agregar el `NavLink` a `NavMenu.razor`.                                 |
| `--no-auth`           | No decorar el controller con `[Authorize]`.                                |
| `--dry-run`           | Mostrar el plan sin tocar nada en disco.                                   |
| `-y, --yes`           | Ejecutar sin pedir confirmación.                                           |
| `--root <PATH>`       | Path explícito a la raíz de la solución (auto-detectado por defecto).      |

### Tipos de propiedad soportados

`string`, `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `bool`, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`.

## Qué genera

Para una entidad `Order` con Id `long`:

**Archivos nuevos (hasta 13):**

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

### Pantallas Blazor

- **Index** (`/order`):
  - Header con título + botón **Nuevo**.
  - Card con filtros (un input por cada propiedad `string` con búsqueda *contains* case-insensitive).
  - Botones **Buscar** y **Limpiar** + contador `filtrados / total`.
  - Tabla `table-hover` con columna `#` (Id), todas las propiedades y columna **Acciones** con **Editar** (lápiz) y **Borrar** (tacho con confirmación JS).
  - Spinner durante la carga, alert info cuando no hay resultados.
- **Edit / Detail** (`/order/new` o `/order/edit/{id}`):
  - Header con botón **Volver** + título dinámico (`(nuevo)` o `#id`).
  - Botón **Borrar** alineado a la derecha (solo en modo edit).
  - `EditForm` con `DataAnnotationsValidator` + `ValidationSummary` y un `ValidationMessage` por campo.
  - Inputs en grid Bootstrap (col-12 para strings, col-6 para el resto).
  - Botón **Guardar** con spinner mientras `saving=true` + botón **Cancelar**.
  - Si `Id == default`, modo crear (POST). Sino carga con `GetAsync` y guarda con `UpdateAsync` (PUT).

**Archivos modificados (idempotente, no duplica):**

- `AspireApp.DataAccess.Implementations/AppDbContext.cs` — agrega `DbSet<Order>` y `modelBuilder.Entity<Order>` con configuración de strings.
- `AspireApp.DataAccess.Implementations/DependencyInjection.cs` — registra `IOrderDA → OrderDA`.
- `AspireApp.Application.Implementations/DependencyInjection.cs` — `using`s y registra `IOrderService → OrderService`.
- `AspireApp.Application.Mappers/DependencyInjection.cs` — registra `OrderMapper`.
- `AspireApp.Client/Program.cs` — registra `OrderApiClient`.
- `AspireApp.Client/Components/Layout/NavMenu.razor` — agrega un `NavLink` apuntando a `/order`.

## Idempotencia

Si la entidad ya existe, los archivos existentes se saltan y los archivos compartidos no se duplican. Podés correrlo varias veces sin riesgo.

## Templates

Los templates están en [`Templates/`](./Templates/) como `*.scriban` (texto plano con tokens `{{TOKEN}}`). Los tokens disponibles:

| Token                        | Ejemplo (`Order`)               |
| ---------------------------- | ------------------------------- |
| `{{ENTITY}}`                 | `Order`                         |
| `{{entity}}`                 | `order`                         |
| `{{ENTITY_CAMEL}}`           | `order`                         |
| `{{ENTITY_PLURAL}}`          | `Orders`                        |
| `{{entity_plural}}`          | `orders`                        |
| `{{ID_TYPE}}`                | `long`                          |
| `{{PROPS_ENTITY}}`           | Propiedades para la entidad     |
| `{{PROPS_MODEL}}`            | Propiedades + DataAnnotations   |
| `{{PROPS_MAPPER_TO_MODEL}}`  | Líneas del mapper hacia model   |
| `{{PROPS_MAPPER_TO_ENTITY}}` | Líneas del mapper hacia entity  |
| `{{PROPS_FORM_FIELDS}}`      | `<InputText>`, `<InputNumber>`… |
| `{{PROPS_TABLE_HEAD}}`       | `<th>` con los nombres          |
| `{{PROPS_TABLE_BODY}}`       | `<td>@item.X</td>`              |
| `{{AUTHORIZE_ATTR}}`         | `[Authorize]\n` o vacío         |
| `{{AUTHORIZE_USING}}`        | El using correspondiente        |

Los archivos compartidos los manejan los mutadores en [`Generator/Mutators/`](./Generator/Mutators/), que detectan duplicados antes de insertar líneas.
