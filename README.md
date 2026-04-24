# Fusion Intelligence Backend Engineering Assessment

## Order & Delivery Management System

A REST API for managing orders and assigning them to delivery agents, built with .NET 9 and clean architecture principles.

---

## Table of Contents
- [Tech Stack](#tech-stack)
- [Setup Instructions](#setup-instructions)
- [Project Structure](#project-structure)
- [API Overview](#api-overview)
- [Authentication](#authentication)
- [Architectural Decisions](#architectural-decisions)
- [Trade-offs Made](#trade-offs-made)
- [Testing](#testing)

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 9 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server |
| Authentication | API Key |
| Logging | Serilog (Console + File) |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit + EF Core InMemory + WebApplicationFactory |

---

## Setup Instructions

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Git](https://git-scm.com/)

### Clone & Configure

```bash
git clone https://github.com/Ebumaurice/OrderDeliverySystem.git
cd OrderDeliverySystem
```

Copy the example settings file and fill in your values:

```bash
cp src/OrderDeliverySystem.Api/appsettings.example.json src/OrderDeliverySystem.Api/appsettings.json
```

Update `appsettings.json` with your connection string and a generated API key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrderDeliveryDb;Trusted_Connection=True;"
  },
  "ApiKey": "your-generated-api-key-here"
}
```

### Apply Migrations & Run

```bash
# Restore dependencies
dotnet restore

# Apply migrations and create database
dotnet ef migrations add InitialCommit -p src/OrderDeliverySystem.Infrastructure -s src/OrderDeliverySystem.API
dotnet ef database update --project src/OrderDeliverySystem.Infrastructure --startup-project src/OrderDeliverySystem.Api

# Run the application
dotnet run --project src/OrderDeliverySystem.Api
```

The API will be available at `https://localhost:7001` and Swagger UI at `https://localhost:7001/swagger`.

### Seeded Data

Three delivery agents are seeded automatically on first run:
- Yakubu Kachiro
- Fola Adeyemi
- Chinedu Okonkwo

---

## Project Structure

---

## API Overview

### Orders

| Method | Endpoint | Auth Required | Description |
|--------|----------|--------------|-------------|
| GET | `/api/orders` | No | List orders with pagination and optional status filter |
| GET | `/api/orders/{id}` | No | Get order by ID |
| POST | `/api/orders` | Yes | Create a new order |
| PATCH | `/api/orders/{id}/status` | Yes | Update order status |
| POST | `/api/orders/{id}/assign` | Yes | Assign a delivery agent to an order |

### Agents

| Method | Endpoint | Auth Required | Description |
|--------|----------|--------------|-------------|
| GET | `/api/agents` | No | List all delivery agents |
| GET | `/api/agents/{id}` | No | Get agent by ID |
| POST | `/api/agents` | Yes | Create a new delivery agent |

### Filtering & Pagination

Valid status values: `Created`, `Assigned`, `InTransit`, `Delivered`, `Cancelled`

### Order Status Transitions

Invalid transitions return `400 Bad Request` with a descriptive error message.

---

## Authentication

All write endpoints (POST/PATCH) are protected with API Key authentication.

Pass the key in every write request header:

When testing via Swagger UI, click the **Authorize** button at the top right and enter your API key. All subsequent requests from Swagger will include the header automatically.

> In production, the API key should be stored in a secrets manager such as Azure Key Vault rather than in `appsettings.json`.

---

## Architectural Decisions

### Clean Architecture Layering

The solution is split into four layers with a strict one-way dependency flow:

- **Domain** — entities and enums only, no dependencies
- **Application** — business logic, service interfaces, DTOs
- **Infrastructure** — EF Core DbContext, persistence implementation
- **Api** — controllers, middleware, authentication, program entry point

The `Application` layer defines `IOrderDeliveryContext` so it never directly references `Infrastructure`, keeping business logic decoupled from EF Core.

### API Key Authentication

API Key was chosen over JWT for simplicity. The key is stored in `appsettings.json` and validated through a custom `AuthenticationHandler`. All write endpoints are protected with `[Authorize]` while read endpoints remain open.

### Entity Status as String

`OrderStatus` is stored as a string in the database using EF Core's `HasConversion<string>()`. This makes the database human-readable and allows filtering directly from query string parameters without manual parsing.

### Delivery Agent Assignment on Order

Rather than a separate `Assignment` table, the assigned agent is tracked via a nullable `DeliveryAgentId` foreign key directly on the `Order` entity. This is sufficient for the requirements and avoids unnecessary complexity.

### Serilog Logging

Serilog is used instead of the default ASP.NET Core logger. It provides structured, timestamped logs to both the console and a rolling daily file under `logs/`. A bootstrap logger captures startup errors before full configuration loads.

---

## Trade-offs Made

### Repository Pattern Not Implemented

In strict clean architecture, the `Application` layer communicates with persistence through repository interfaces, keeping it fully decoupled from EF Core. For this assessment, `IOrderDeliveryContext` is injected directly into services to reduce boilerplate without sacrificing clarity. In a production system with multiple data sources or larger teams, introducing the repository pattern would improve testability and long-term maintainability.

### Manual Mapping over AutoMapper

DTOs are mapped to and from domain entities using private static helper methods within each service. AutoMapper was intentionally avoided because:

- Every mapped field is explicit and traceable — no convention magic at runtime
- Mapping errors surface as compile-time failures rather than silent mismatches
- AutoMapper's profile configuration overhead is not justified for two entities

At larger scale, AutoMapper or Mapster would be worth introducing to reduce verbosity.

### Centralised Dependency Injection

All service registrations are consolidated in `Program.cs` rather than split across per-layer `DependencyInjection` extension classes. In a larger solution, per-layer registration improves encapsulation and makes each project independently testable. For this assessment, a single registration file keeps the wiring easy to follow without unnecessary indirection.

---

## Testing

The test suite is in `OrderDeliverySystem.Tests` and uses xUnit with EF Core's in-memory provider.

### Run Tests

```bash
dotnet test
```

### Test Summary

### Unit Tests — `UnitTests/OrderServiceTests.cs`

Business logic tested directly against the service layer using an in-memory database:

- Valid status transitions succeed (`Created → Assigned`, `Assigned → InTransit` etc.)
- Invalid status transitions throw `InvalidOperationException`
- Assigning an agent with an existing active order is blocked
- Assigning an inactive agent is blocked
- Assigning an agent to a Delivered order is blocked
- Updating status on a non-existent order throws `KeyNotFoundException`

### Integration Tests — `IntegrationTests/OrdersControllerTests.cs`

Full HTTP pipeline tested using `WebApplicationFactory`:

- `POST /api/orders` returns `201 Created` with correct response body
- `POST /api/orders` without API key returns `401 Unauthorized`
- `GET /api/orders` returns `200 OK` with paged response
- `GET /api/orders/{id}` for existing order returns `200 OK`
- `GET /api/orders/{id}` for non-existing order returns `404 Not Found`