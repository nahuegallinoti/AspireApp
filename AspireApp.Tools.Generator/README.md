# AspireApp.Tools.Generator

CLI para scaffolding de entidades en la solución AspireApp. Genera todos los archivos de las capas (Domain, Application, Infrastructure, API, Client) y modifica los archivos compartidos (DI, AppDbContext, NavMenu, Program.cs) en una sola corrida. Las pantallas Blazor generadas usan **Bootstrap 5 + Bootstrap Icons** con un layout limpio y consistente: header sencillo con icono de la entidad, card de filtros, tabla, empty/loading states y formulario de edición sin decoración excesiva.

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

| Opción                | Descripción                                                                                            |
| --------------------- | ------------------------------------------------------------------------------------------------------ |
| `ENTITY_NAME`         | Nombre PascalCase singular de la entidad (e.g. `Order`).                                               |
| `--id <ID_TYPE>`      | Tipo del Id: `long`, `int` o `Guid`. Default `long`.                                                   |
| `-p, --prop <PROP>`   | Propiedad en formato `Name:type` o `Name:type:required`. Repetible.                                    |
| `--icon <ICON>`       | Override del Bootstrap Icon (sin el prefijo `bi-`). Auto-detectado por nombre (Order → `receipt`, etc).|
| `--accent <ACCENT>`   | Override del color de acento Bootstrap: `primary`, `success`, `info`, `warning`, `danger`, `secondary`.|
| `--no-ui`             | No generar las pantallas Blazor (`Index` + `Edit`). En modo interactivo se pregunta.                   |
| `--no-nav`            | No agregar el `NavLink` a `NavMenu.razor`.                                                             |
| `--no-auth`           | No decorar el controller con `[Authorize]`.                                                            |
| `--dry-run`           | Mostrar el plan sin tocar nada en disco.                                                               |
| `-y, --yes`           | Ejecutar sin pedir confirmación.                                                                       |
| `--root <PATH>`       | Path explícito a la raíz de la solución (auto-detectado por defecto).                                  |

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

Las pantallas se generan con un look limpio y consistente, sin decoración innecesaria:

- **Index** (`/order`):
  - **Header simple**: icono de la entidad con el color de acento + título + botones **Refrescar** y **Nuevo**.
  - **Errores cerrables** con `<button class="btn-close">`.
  - **Card de filtros**: un input por cada propiedad `string`, botones **Buscar** y **Limpiar**, contador `filtrados de total` alineado a la derecha.
  - **Tabla** simple (`table-hover align-middle`) con cabecera clara y sin iconos por columna.
  - **Renderizado por tipo en la tabla**: `bool` → texto "Sí/No", `DateTime` → fecha `yyyy-MM-dd`, numéricos → fuente monospace alineada a la derecha, `Email` → `mailto:`, `Phone`/`Mobile` → `tel:`. Id en fuente monospace gris.
  - **Botonera por fila**: lápiz azul (Editar) + tacho rojo (Borrar).
  - **Empty state**: icono `bi-inbox` grande, mensaje, botones **Limpiar filtros** y **Crear primero**.
  - **Loading state**: spinner centrado con texto descriptivo.

- **Edit / Detail** (`/order/new` o `/order/edit/{id}`):
  - **Header simple**: flecha de volver + icono de la entidad + título dinámico + badge `#id` (sólo en edición) + botón **Borrar** alineado a la derecha (sólo en edición).
  - **Card** con el formulario (sin header/footer separados).
  - **EditForm** con `DataAnnotationsValidator` + `ValidationSummary` (alert warning) y un `ValidationMessage` por campo.
  - **Inputs simples** (`InputText`, `InputNumber`, `InputDate`) — sin `input-group` ni iconos de prefijo, label limpio con asterisco para los requeridos.
  - **Booleans como switch** (`form-check form-switch`).
  - **Botón Guardar** con spinner mientras `saving=true` y texto "Guardando..." + botón Cancelar, ambos alineados a la derecha.

### Iconos contextuales

El generador detecta automáticamente un Bootstrap Icon adecuado para tu entidad:

- **Entidades**: `Order` → `receipt`, `User` → `person-circle`, `Product` → `box-seam`, `Invoice` → `file-earmark-text`, `Event` → `calendar-event`, `Country` → `globe2`, `City` → `buildings`, `Project` → `kanban`, `Ticket` → `ticket-detailed`, etc. Si no hay match → `collection`.

Podés forzar uno específico con `--icon <name>` (sin el prefijo `bi-`). El icono aparece al lado del título (Index y Edit) y como `NavLink` en el menú lateral.

### Acento de color

Por defecto todas las entidades comparten el acento `primary` para que la app se vea cohesiva: la identidad visual la da el icono, no el color. Podés overridear por entidad con `--accent <primary|success|info|warning|danger|secondary>` si una pantalla específica lo amerita. `danger` se usa siempre y de forma fija para acciones destructivas (botón Borrar).

**Archivos modificados (idempotente, no duplica):**

- `AspireApp.DataAccess.Implementations/AppDbContext.cs` — agrega `DbSet<Order>` y `modelBuilder.Entity<Order>` con configuración de strings.
- `AspireApp.DataAccess.Implementations/DependencyInjection.cs` — registra `IOrderDA → OrderDA`.
- `AspireApp.Application.Implementations/DependencyInjection.cs` — `using`s y registra `IOrderService → OrderService`.
- `AspireApp.Application.Mappers/DependencyInjection.cs` — registra `OrderMapper`.
- `AspireApp.Client/Program.cs` — registra `OrderApiClient`.
- `AspireApp.Client/Components/Layout/NavMenu.razor` — agrega un `NavLink` apuntando a `/order` **con el icono contextual** de la entidad.

## Experiencia CLI

La terminal muestra (todo en español, con iconografía consistente):

1. **Banner Figlet "AspireApp"** en violeta + tagline con la cadena de capas (`Domain › Application › Infra › Api › Client`) cada una en su color.
2. **Context bar** con `📁 Root`, `⚙ Modo` (APPLY verde o DRY-RUN amarillo) y, si aplica, `🔕 Prompts desactivados`.
3. **Panel "Entity preview"** con borde violeta: bloque con icono Bootstrap detectado + nombre + plural, `🔑 Id type`, `🎨 Acento` con bullet de color, `🖼 Blazor UI`, `🧭 NavMenu`, `🔒 Authorize`. Luego una **tabla de propiedades** (`# / Nombre / Tipo / Req`) con marca `✔` para los requeridos.
4. **Tree del plan** (`🗺 Plan de generación`) con los archivos a crear agrupados por capa, cada una con su glifo: `◆ Domain` magenta, `▼ Application` azul, `▣ Infrastructure` violeta, `▲ Api` dorado, `✦ Client` aqua, `✎ Shared` naranja.
5. **Status spinner** durante la creación con el tag de capa coloreada por línea y prefijos `✚` (creado) / `○` (omitido) / `✎` (mutado) / `✗` (error).
6. **Panel `▣ Resumen`** con borde doble verde (o rojo si hubo errores), filas iconadas `✔` estado, `✚ Creados`, `✎ Actualizados`, `○ Omitidos`, `✗ Fallidos`.
7. **Sección `🚀 Próximos pasos`** con los comandos sugeridos numerados.

## Idempotencia

Si la entidad ya existe, los archivos existentes se saltan y los archivos compartidos no se duplican. Podés correrlo varias veces sin riesgo.

## Templates

Los templates están en [`Templates/`](./Templates/) como `*.scriban` (texto plano con tokens `{{TOKEN}}`). Los tokens disponibles:

| Token                        | Ejemplo (`Order`)                              |
| ---------------------------- | ---------------------------------------------- |
| `{{ENTITY}}`                 | `Order`                                        |
| `{{entity}}`                 | `order`                                        |
| `{{ENTITY_CAMEL}}`           | `order`                                        |
| `{{ENTITY_PLURAL}}`          | `Orders`                                       |
| `{{entity_plural}}`          | `orders`                                       |
| `{{ID_TYPE}}`                | `long`                                         |
| `{{ENTITY_ICON}}`            | `receipt` (Bootstrap Icons, sin prefijo `bi-`) |
| `{{ACCENT}}`                 | `warning`, `primary`, `success`...             |
| `{{ACCENT_SUBTLE}}`          | `warning-subtle` (para `bg-*-subtle`)          |
| `{{PROPS_ENTITY}}`           | Propiedades para la entidad                    |
| `{{PROPS_MODEL}}`            | Propiedades + DataAnnotations                  |
| `{{PROPS_MAPPER_TO_MODEL}}`  | Líneas del mapper hacia model                  |
| `{{PROPS_MAPPER_TO_ENTITY}}` | Líneas del mapper hacia entity                 |
| `{{PROPS_FORM_FIELDS}}`      | `<InputText>`, `<InputNumber>`… con iconos     |
| `{{PROPS_TABLE_HEAD}}`       | `<th>` con icono + nombre                      |
| `{{PROPS_TABLE_BODY}}`       | `<td>` con badges/links/format por tipo        |
| `{{PROPS_FILTER_FIELDS}}`    | input-groups con icono `bi-search`             |
| `{{AUTHORIZE_ATTR}}`         | `[Authorize]\n` o vacío                        |
| `{{AUTHORIZE_USING}}`        | El using correspondiente                       |

Los archivos compartidos los manejan los mutadores en [`Generator/Mutators/`](./Generator/Mutators/), que detectan duplicados antes de insertar líneas.
