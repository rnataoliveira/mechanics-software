# ADR-001: Tech Stack

**Status:** Accepted
**Date:** 2026-03-06

## Context

The Tech Challenge Phase 1 requires a backend MVP for a mechanic shop management system. It must be delivered with Dockerfile, docker-compose, Swagger, and automated tests with 80% coverage on critical domains. The team's primary stack is C#/.NET.

## Decision

Chosen stack:

- **Language:** C# 12
- **Framework:** ASP.NET Core 8 (LTS)
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core 8 + Npgsql
- **Authentication:** JWT via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Documentation:** Swagger via `Swashbuckle.AspNetCore`
- **Testing:** xUnit + Moq + FluentAssertions
- **Containerization:** Docker + docker-compose

## Project Structure

```
src/
  MechanicsSoftware.API/             # Presentation: controllers, DTOs, Swagger, middleware
  MechanicsSoftware.Application/     # Application: use cases, interfaces, DTOs
  MechanicsSoftware.Domain/          # Domain: entities, value objects, repository interfaces
  MechanicsSoftware.Infrastructure/  # Infrastructure: EF Core, repositories, JWT

tests/
  MechanicsSoftware.UnitTests/
  MechanicsSoftware.IntegrationTests/
```

## Rationale

### ASP.NET Core 8
- Primary stack of the team — faster delivery, easier code reviews
- Native support for Swagger, JWT, dependency injection, middleware
- .NET 8 is LTS — stable for production
- Excellent DDD support with clean layering

### PostgreSQL
- Relational database suited to the domain (strongly related entities)
- Full ACID transactions — essential for stock operations and status transitions
- Native support via Npgsql and EF Core

### Entity Framework Core
- Code-first migrations keep schema versioned alongside code
- Strong typing and LINQ integration
- Npgsql provider is mature and actively maintained

## Alternatives Considered

| Alternative | Reason Not Chosen |
|---|---|
| NestJS / TypeScript | Team unfamiliar; slower delivery for the group |
| Java Spring Boot | Not considered by the team |
| Dapper (instead of EF Core) | More boilerplate for CRUD-heavy MVP; EF Core is sufficient |
| MySQL | PostgreSQL has better advanced type support and EF Core integration |

## Consequences

- All team members must have .NET 8 SDK installed
- Local environment runs via docker-compose (PostgreSQL + API)
- EF Core migrations must be versioned and applied before running the app
