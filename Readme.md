# AspireApp

AspireApp is a modular solution built on .NET 9 and Blazor, following **Clean Architecture** principles and focused on scalability, maintainability, and modern best practices. 

This document describes the project structure, key technologies, and patterns used.

---

## 📂 Project Structure and Technologies

### 🏗️ **Api**
- **AspireApp.Api**
  - ASP.NET Core API
  - JWT Authentication
  - Dependency Injection
  - Redis Cache
  - Hybrid Cache
  - RabbitMQ
  - Entity Framework Core InMemory
  - Logging
  - ProblemDetails (HTTP error handling)
  - ServiceDefaults (common configuration)
  - .NET 9

- **AspireApp.Api.Models**
  - API data models

### 🖥️ **Client**
- **AspireApp.Client**
  - Blazor (Server or WASM depending on implementation)
  - HTTP API consumption (HttpClientFactory)
  - Dependency Injection
  - Railway Oriented Programming (ROP)
  - Example: Product page with API integration, error handling, and form management

- **AspireApp.Client.ApiClients**
  - HttpClientFactory
  - HTTP error handling
  - ROP

### ⚙️ **Core**
- **AspireApp.Core.Mappers**
  - Entity and DTO mapping
  
- **AspireApp.Core.ROP**
  - Railway Oriented Programming (ROP)
  - Success/failure flow management

### 📦 **Domain**
- **AspireApp.Application.Contracts**
  - Application service contracts
  
- **AspireApp.Application.Implementations**
  - Business logic implementations
  - JWT Authentication
  - RabbitMQ
  - Redis Cache
  - Hybrid Cache
  - Kafka
  - ROP
  
- **AspireApp.DataAccess.Contracts**
  - Data access contracts
  - ROP
 
- **AspireApp.DataAccess.Implementations**
  - Entity Framework Core
  - Entity Framework Core InMemory
  
- **AspireApp.Entities**
  - Domain entity models

### 🏗️ **Infrastructure**
- **AspireApp.AppHost**
  - Application configuration
  
- **AspireApp.ServiceDefaults**
  - Common service configuration

### 🛠️ **Tests**
- **AspireApp.Tests.Client**
  - MSTest
  - Moq

---

## 🔑 Key Components and Patterns

### 🛡️ **Authentication and Authorization**
JWT authentication is used to protect API endpoints: