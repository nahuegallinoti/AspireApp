# AspireApp

AspireApp es una aplicación Blazor basada en .NET 9, diseñada con un enfoque en arquitectura limpia, modularidad y escalabilidad. Este documento proporciona una visión general de la estructura del proyecto, los patrones utilizados y sus componentes clave.

## 📂 Estructura del Proyecto

El proyecto está dividido en varias capas y componentes, cada uno con una responsabilidad específica:

- **`AspireApp.Api`**: Contiene los controladores API y la configuración de servicios.
- **`AspireApp.Application`**: Lógica de la aplicación e implementación de servicios.
- **`AspireApp.Core`**: Utilidades, helpers y lógica común.
- **`AspireApp.DataAccess`**: Implementaciones de acceso a datos y patrones de repositorio.
- **`AspireApp.Entities`**: Modelos de entidades.
- **`AspireApp.Web`**: Aplicación Web Blazor Server.
- **`AspireApp.Web.ApiClients`**: Api Clients utilizados para hacer consultas a la api.
- **`AspireApp.Web.Tests`**: Pruebas unitarias de la aplicación.

## 🔑 Componentes Clave y Patrones

### 1️⃣ Autenticación y Autorización

Se utiliza autenticación JWT para proteger los endpoints de la API:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();
```

### 2️⃣ Caching

El proyecto implementa una estrategia de caché híbrida con Redis y almacenamiento en memoria:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

builder.Services.AddHybridCache();
```

### 3️⃣ Entity Framework Core

Se utiliza Entity Framework Core para el acceso a datos. Para pruebas, se configura una base de datos en memoria:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AspireAppDb"));
```

### 4️⃣ Pruebas Unitarias

El proyecto incluye pruebas unitarias con **MSTest** y **Moq** para simular dependencias y garantizar la calidad del código.

### 5️⃣ Patrón Base Service

Se implementa un patrón de servicio base para encapsular operaciones CRUD comunes.

### 6️⃣ Programación Orientada a Resultados (ROP)

Se adopta **Result-Oriented Programming (ROP)** para manejar los resultados de las operaciones de manera consistente y expresiva.

### 7️⃣ RabbitMQ

Se integra **RabbitMQ** como servicio de mensajería para facilitar la comunicación entre diferentes partes del sistema:

```csharp
builder.Services.AddSingleton<RabbitMqService>(); // Registro del servicio RabbitMQ
```

## 🚀 Cómo Ejecutar el Proyecto

Sigue estos pasos para ejecutar la aplicación:

1. Clona el repositorio.
2. Abre la solución en **Visual Studio 2022**.
3. Restaura los paquetes NuGet.
4. Compila la solución.
5. Ejecuta el proyecto.

## 🏁 Conclusión

AspireApp es una aplicación Blazor bien estructurada que aprovecha las características modernas de .NET y sigue las mejores prácticas. La combinación de **inyección de dependencias, autenticación JWT, caching, pruebas unitarias, ROP y RabbitMQ** garantiza un código robusto, escalable y fácil de mantener.
