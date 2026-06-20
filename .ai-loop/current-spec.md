# Spec: Unit tests para UserService

## Objetivo
Cubrir con tests unitarios `UserService` (la única clase sin cobertura en el área de usuario).
`UserDATests` ya existe y cubre la capa de datos; no tocarla.

---

## Archivo a crear

```
AspireApp.Tests/Application/Users/UserServiceTests.cs
```

Namespace: `AspireApp.Tests.Application.Users`

---

## Dependencias / stack de testing

- **Framework**: xunit.v3 (global using `Xunit`)
- **Aserciones**: FluentAssertions (global using `FluentAssertions`)
- **Mocks**: NSubstitute (global using `NSubstitute`)
- **SUT**: `AspireApp.Application.Implementations.Users.UserService` (clase `internal sealed`)
  - Accesible porque `AspireApp.Tests` ya referencia `AspireApp.Application.Implementations.csproj`

---

## Firmas de dependencias a mockear

```csharp
// AspireApp.Application.Persistence.IUserDA
Task<User?>  GetByIdAsync(Guid id, CancellationToken ct);
Task<User?>  GetByIdWithRolesAsync(Guid id, CancellationToken ct);
Task<IReadOnlyList<User>> ListWithRolesAsync(int skip, int take, string? search, CancellationToken ct);
Task<int>    CountAsync(string? search, CancellationToken ct);
void         Update(User entity);
void         Delete(User entity);
Task         SaveChangesAsync(CancellationToken ct);

// AspireApp.Application.Persistence.IRoleDA
Task<IReadOnlyList<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct);

// AspireApp.Application.Persistence.IRefreshTokenDA
Task RevokeAllForUserAsync(Guid userId, string reason, string? revokedByIp, CancellationToken ct);

// AspireApp.Application.Contracts.Auth.IPasswordHasher
bool Verify(string plain, byte[] hash, byte[] salt, int iterations);
(byte[] hash, byte[] salt, int iterations) Hash(string plain);

// System.TimeProvider (clase abstracta)
DateTimeOffset GetUtcNow();
```

---

## Helper builder

```csharp
private static (UserService sut,
                IUserDA userDA,
                IRoleDA roleDA,
                IRefreshTokenDA refreshTokenDA,
                IPasswordHasher hasher,
                TimeProvider time) Build()
{
    var userDA          = Substitute.For<IUserDA>();
    var roleDA          = Substitute.For<IRoleDA>();
    var refreshTokenDA  = Substitute.For<IRefreshTokenDA>();
    var hasher          = Substitute.For<IPasswordHasher>();
    var time            = Substitute.For<TimeProvider>();
    time.GetUtcNow().Returns(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    var sut = new UserService(userDA, roleDA, refreshTokenDA, hasher, time);
    return (sut, userDA, roleDA, refreshTokenDA, hasher, time);
}
```

Helper de entidad con roles:

```csharp
private static User MakeUser(params string[] roleNames)
{
    var user = new User
    {
        Id      = Guid.NewGuid(),
        Email   = "u@example.com",
        Name    = "Ana",
        Surname = "López",
        IsActive = true,
        PasswordHash = [1, 2, 3],
        PasswordSalt = [4, 5, 6],
        PasswordIterations = 10_000,
        CreatedUtc = DateTimeOffset.UtcNow,
        UserRoles  = []
    };
    foreach (var n in roleNames)
        user.UserRoles.Add(new UserRole { UserId = user.Id, Role = new Role { Id = Guid.NewGuid(), Name = n } });
    return user;
}
```

---

## Casos de test requeridos

### GetByIdAsync

| Test | Setup | Expect |
|---|---|---|
| `GetById_WhenUserExists_ReturnsDto` | `userDA.GetByIdWithRolesAsync` devuelve `MakeUser("Admin")` | `result.Success == true`, `result.Value.Email == "u@example.com"`, roles contiene "Admin" |
| `GetById_WhenUserNotFound_ReturnsNotFound` | devuelve `null` | `result.IsFailure == true`, `HttpStatusCode == 404` |

---

### ListAsync

| Test | Setup | Expect |
|---|---|---|
| `List_NormalizesPageAndPageSize` | page=0, pageSize=500, devuelve 2 users, total=2 | skip pasado a `ListWithRolesAsync` == 0 (page clampado a 1), take == 200 (pageSize clampado) |
| `List_ReturnsCorrectPageMetadata` | page=2, pageSize=10, total=25 | `result.Page==2`, `result.PageSize==10`, `result.Total==25`, `result.Items.Count==1` (un user seedeado) |

---

### UpdateAsync

| Test | Setup | Expect |
|---|---|---|
| `Update_WhenValid_PersistsAndReturnsDto` | user existe, request válida `{Name="Pedro", Surname="Ruiz", IsActive=false}` | `user.Name=="Pedro"`, `user.Surname=="Ruiz"`, `user.IsActive==false`, `userDA.Update` llamado, `SaveChangesAsync` llamado, result success |
| `Update_WhenUserNotFound_ReturnsNotFound` | `GetByIdWithRolesAsync` → null | `result.IsFailure`, 404 |
| `Update_TrimsNameAndSurname` | request con `Name="  Bob  "`, `Surname=" Mar "` | `user.Name=="Bob"`, `user.Surname=="Mar"` |
| `Update_WhenNameEmpty_ReturnsValidationFailure` | `Name=""` | `result.IsFailure`, 400, `userDA.Update` NO llamado |
| `Update_WhenNameTooLong_ReturnsValidationFailure` | `Name` de 129 chars | `result.IsFailure`, 400 |

---

### UpdateProfileAsync

| Test | Setup | Expect |
|---|---|---|
| `UpdateProfile_WhenValid_DoesNotChangeIsActive` | user con `IsActive=false`, request `{Name="X", Surname="Y"}` | `user.IsActive` permanece `false`, result success |
| `UpdateProfile_WhenUserNotFound_ReturnsNotFound` | null | 404 |
| `UpdateProfile_WhenNameEmpty_ReturnsValidationFailure` | `Name=""` | 400 |

---

### DeleteAsync

| Test | Setup | Expect |
|---|---|---|
| `Delete_WhenUserExists_RevokesTokensAndDeletes` | `GetByIdAsync` devuelve user | `refreshTokenDA.RevokeAllForUserAsync(user.Id, "UserDeleted", null, ct)` llamado, `userDA.Delete` llamado, result success |
| `Delete_WhenUserNotFound_ReturnsNotFound` | null | 404, `userDA.Delete` NO llamado |

---

### AssignRolesAsync

| Test | Setup | Expect |
|---|---|---|
| `AssignRoles_WhenValid_ReplacesRolesAndRevokesTokens` | user con rol "Admin", request `["User","Manager"]`, `roleDA` devuelve ambos | `user.UserRoles.Count==2`, `refreshTokenDA.RevokeAllForUserAsync(_, "RolesChanged", _, _)` llamado, result success |
| `AssignRoles_WhenRoleMissing_ReturnsFailure` | roleDA devuelve sólo 1 de 2 solicitados | `result.IsFailure`, errors contiene "Unknown role(s)" |
| `AssignRoles_WhenUserNotFound_ReturnsNotFound` | null | 404 |
| `AssignRoles_WhenRolesListEmpty_ReturnsValidationFailure` | `Roles=[]` | 400 (MinLength(1)) |

---

### ChangePasswordAsync

| Test | Setup | Expect |
|---|---|---|
| `ChangePassword_WhenValid_UpdatesHashAndRevokesTokens` | user con password, hasher.Verify → true, hasher.Hash → (newHash, newSalt, 12_000) | `user.PasswordHash==newHash`, `refreshTokenDA` llamado con "PasswordChanged", result success |
| `ChangePassword_WhenUserNotFound_ReturnsNotFound` | null | 404 |
| `ChangePassword_WhenUserIsExternal_ReturnsConflict` | `user.HasPassword==false` (PasswordHash null) | 409 |
| `ChangePassword_WhenCurrentPasswordWrong_ReturnsUnauthorized` | hasher.Verify → false | 401 |
| `ChangePassword_WhenNewPasswordTooShort_ReturnsValidationFailure` | `NewPassword="1234567"` (7 chars) | 400, `userDA.Update` NO llamado |

---

## Convenciones a seguir (igual que AuthServiceTests)

- Un método de test = un caso de uso, nombre en formato `Método_Condición_Resultado`.
- `CancellationToken.None` en todas las llamadas.
- No uses `TimeProvider.System`; usar el mock para que los tests no dependan del reloj real.
- NSubstitute: `Received(1).Method(...)` para verificar interacciones; `DidNotReceive()` para negativos.
- No agregar namespace en los usings si ya están en `GlobalUsings.cs`.
- Imports mínimos: sólo los necesarios para compilar (`AspireApp.Application.Implementations.Users`, `AspireApp.Application.Persistence`, `AspireApp.Application.Models.Users`, `AspireApp.Domain.Entities`, `AspireApp.Application.Contracts.Auth`).

---

## Lo que NO hay que hacer

- No crear tests de integración (sin EF InMemory en este archivo).
- No tocar `UserDATests.cs` ni los tests de Auth existentes.
- No crear helpers en archivos separados; todo en el mismo archivo `UserServiceTests.cs`.
- No testear `UserMapper.ToDto` aquí (ya cubierto o puede cubrirse en `Mappers/`).
