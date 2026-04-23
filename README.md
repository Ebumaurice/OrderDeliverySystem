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
| Auth | JWT |
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
dotnet ef database update -p OrderDeliverySystem.Infrastructure -s OrderDeliverySystem.API

# Run the application
cd OrderDeliverySystem.API
dotnet run