# AspireApp

AspireApp es una aplicación basada en .NET 9, diseñada con una arquitectura modular y escalable, siguiendo principios de **Arquitectura Limpia**. Este documento proporciona una visión general de la estructura del proyecto, sus componentes clave y patrones utilizados.

---

## 📂 Estructura del Proyecto

El proyecto está organizado en varias capas y componentes:

### 🏗️ **Api**
- **`AspireApp.Api`**: Define los controladores y la configuración de la API.
- **`AspireApp.Api.Models`**: Modelos utilizados por la API.

### 🖥️ **Client**
- **`AspireApp.Client`**: Aplicación cliente.
- **`AspireApp.Client.ApiClients`**: Clientes para consumir la API.

### ⚙️ **Core**
- **`AspireApp.Core.Mappers`**: Mapeo de entidades y DTOs.
- **`AspireApp.Core.ROP`**: Implementación de Programación Orientada a Resultados (ROP).

### 📦 **Domain**
- **`AspireApp.Application.Contracts`**: Contratos de la capa de aplicación.
- **`AspireApp.Application.Implementations`**: Implementaciones de la lógica de negocio.
- **`AspireApp.DataAccess.Contracts`**: Contratos de acceso a datos.
- **`AspireApp.DataAccess.Implementations`**: Implementaciones de acceso a datos.
- **`AspireApp.Entities`**: Definición de entidades del dominio.

### 🏗️ **Infrastructure**
- **`AspireApp.AppHost`**: Configuración de la aplicación.
- **`AspireApp.ServiceDefaults`**: Configuración de servicios comunes.

### 🛠️ **Tests**
- **`AspireApp.Tests.Client`**: Pruebas unitarias del cliente.

---

## 🔑 Componentes Clave y Patrones

### 🛡️ **Autenticación y Autorización**
Se utiliza autenticación **JWT** para proteger los endpoints:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
```

### 💾 **Caching**
Se implementa un mecanismo de caché con **Redis**:
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

### 🗄️ **Entity Framework Core**
Para acceso a datos se usa **EF Core**:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

### ✅ **Pruebas Unitarias**
Se utilizan **MSTest** y **Moq** para garantizar la calidad del código.

### 📜 **Programación Orientada a Resultados (ROP)**
Se adopta **ROP** para mejorar el manejo de errores y resultados.

### 📨 **Mensajería con RabbitMQ**
Se integra **RabbitMQ** para la comunicación entre módulos:
```csharp
builder.Services.AddSingleton<RabbitMqService>();
```

---

## 🚀 Cómo Ejecutar el Proyecto

1️⃣ Clonar el repositorio:
```sh
git clone https://github.com/tu-repo/aspireapp.git
```
2️⃣ Abrir la solución en **Visual Studio 2022**.
3️⃣ Restaurar paquetes NuGet:
```sh
dotnet restore
```
4️⃣ Compilar y ejecutar:
```sh
dotnet run --project AspireApp.Api
```

---

## 🏁 Conclusión
AspireApp es un proyecto bien estructurado, con un enfoque en **modularidad, seguridad y escalabilidad**. Gracias a su organización clara y uso de patrones modernos, permite un desarrollo eficiente y mantenible. 🎯