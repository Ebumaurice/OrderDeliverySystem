# Fusion Intelligence Backend Engineering Assessment

## Order & Delivery Management System

A REST API for managing orders and assigning them to delivery agents, built with .NET 9

---

## Table of Contents
- [Tech Stack](#tech-stack)
- [Setup Instructions](#setup-instructions)
- [API Overview](#api-overview)
- [Architectural Decisions](#architectural-decisions)
- [Trade-offs Made](#trade-offs-made)
- [Testing](#testing)
- [Potential Improvements](#potential-improvements)

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 9 |
| ORM | Entity Framework Core |
| Database | SQL Server (production-ready) |
| Auth | API Key |
| Logging | Serilog |
| Caching | Redis |
| Container | Docker (bonus if implemented) |
| Testing | xUnit + Moq + Microsoft.AspNetCore.TestHost |

---

## Setup Instructions

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Redis](https://redis.io/download/)

### Clone & Run

```bash
# Clone repository
git clone https://github.com/Ebumaurice/OrderDeliverySystem.git
cd OrderDeliverySystem.API

# Restore dependencies
dotnet restore

# Apply migrations & create database
dotnet ef migrations add ConfigureRelationshipsAndSeedAgents -p src/OrderDeliverySystem.Infrastructure -s src/OrderDeliverySystem.API
dotnet ef database update -p src/OrderDeliverySystem.Infrastructure -s src/OrderDeliverySystem.API


# Run the application
cd OrderDeliverySystem.API
dotnet run


### Trade-offs
Architectural Trade-off: Repository Pattern
In strict clean architecture, the Application layer communicates with persistence through repository interfaces, keeping it decoupled from EF Core. For this assessment I chose to inject OrderDeliveryContext directly into services to reduce boilerplate without sacrificing clarity. In a production system with multiple data sources or larger teams, introducing the repository pattern would improve testability and maintainability. Unit tests use EF Core's in-memory provider to compensate for the lack of repository interfaces.

### Another one
Architectural Trade-off: Manual Mapping over AutoMapper
DTOs are mapped to and from domain entities manually using private static helper methods within each service class. AutoMapper was intentionally avoided for the following reasons:

Explicitness — every mapped field is visible and traceable. There is no magic convention matching happening at runtime.
Debuggability — mapping errors surface as compile-time errors rather than silent runtime mismatches.
No extra dependency — AutoMapper adds configuration overhead (profiles, CreateMap<> declarations) that isn't justified for a project with two entities.

The trade-off is verbosity — as the number of entities grows, manual mapping becomes repetitive. At that scale, AutoMapper or a lightweight alternative like Mapster would be worth introducing.

####
Architectural Trade-off: Centralised Dependency Injection
All service registrations are consolidated in Program.cs rather than split across per-layer DependencyInjection extension classes. In a larger solution, per-layer registration improves encapsulation and makes each project self-contained. For this assessment, centralising registrations reduces file count and keeps the wiring easy to follow in one place.