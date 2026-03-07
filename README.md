# Mechanics Software

Backend system for a mechanic shop — built as the Phase 1 Tech Challenge for FIAP POS Tech (15SOAT).

## Overview

A RESTful API that manages the full lifecycle of service orders, customers, vehicles, parts, and inventory for a medium-sized auto repair shop.

**Architecture:** Vertical Slice Architecture + DDD Domain
**Stack:** C# 12 · ASP.NET Core 8 · PostgreSQL 16 · Entity Framework Core 8
**Docs:** [`/docs`](./docs)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop) + Docker Compose

### Run with Docker (recommended)

```bash
docker compose up --build
```

API available at: `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

### Run locally

```bash
# 1. Start the database
docker compose up db -d

# 2. Apply migrations
dotnet ef database update --project src/MechanicsSoftware.Infrastructure --startup-project src/MechanicsSoftware.API

# 3. Run the API
dotnet run --project src/MechanicsSoftware.API
```

### Environment variables

| Variable | Description | Default |
|---|---|---|
| `DATABASE_URL` | PostgreSQL connection string | see `docker-compose.yml` |
| `JWT_SECRET` | Secret key for JWT signing | — |
| `JWT_EXPIRATION_MINUTES` | Token expiration time | `60` |
| `BCRYPT_SALT_ROUNDS` | BCrypt salt rounds | `12` |

---

## API

### Public endpoints (no authentication)

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate and receive JWT |
| `GET` | `/api/service-orders/{id}/status` | Customer status query |

### Protected endpoints (JWT required)

| Resource | Endpoints |
|---|---|
| Customers | `GET/POST /api/customers` · `GET/PUT/DELETE /api/customers/{id}` |
| Vehicles | `GET/POST /api/vehicles` · `GET/PUT/DELETE /api/vehicles/{id}` |
| Services | `GET/POST /api/services` · `GET/PUT/DELETE /api/services/{id}` |
| Parts | `GET/POST /api/parts` · `GET/PUT/DELETE /api/parts/{id}` · `PATCH /api/parts/{id}/stock` |
| Service Orders | `GET/POST /api/service-orders` · `GET /api/service-orders/{id}` |
| OS Actions | `POST /api/service-orders/{id}/{action}` |
| Metrics | `GET /api/service-orders/metrics/average-execution-time` |

**OS actions:** `services` · `parts` · `budget` · `approve` · `reject` · `start-diagnosis` · `start-execution` · `complete` · `deliver`

Full documentation available at `/swagger` when running.

---

## Service Order Lifecycle

```
RECEIVED → IN_DIAGNOSIS → AWAITING_APPROVAL → IN_EXECUTION → COMPLETED → DELIVERED
                                    ↓
                                CANCELLED
```

---

## Project Structure

```
src/
  MechanicsSoftware.Domain/        # Entities, value objects, business rules
  MechanicsSoftware.Application/   # Use cases organized by feature (VSA)
  MechanicsSoftware.Infrastructure/ # EF Core, JWT, BCrypt
  MechanicsSoftware.API/           # Controllers, middleware, Swagger

tests/
  MechanicsSoftware.UnitTests/
  MechanicsSoftware.IntegrationTests/
```

See [`docs/architecture/overview.md`](./docs/architecture/overview.md) for full details.

---

## Documentation

| Document | Path |
|---|---|
| Architecture Overview | [`docs/architecture/overview.md`](./docs/architecture/overview.md) |
| Event Storming | [`docs/domain/event-storming.md`](./docs/domain/event-storming.md) |
| Ubiquitous Language | [`docs/domain/ubiquitous-language.md`](./docs/domain/ubiquitous-language.md) |
| Aggregates & Entities | [`docs/domain/aggregates-and-entities.md`](./docs/domain/aggregates-and-entities.md) |
| Bounded Contexts | [`docs/domain/bounded-contexts.md`](./docs/domain/bounded-contexts.md) |
| ADR-001 Tech Stack | [`docs/decisions/ADR-001-tech-stack.md`](./docs/decisions/ADR-001-tech-stack.md) |
| ADR-002 Architecture | [`docs/decisions/ADR-002-architecture.md`](./docs/decisions/ADR-002-architecture.md) |
| ADR-003 Database | [`docs/decisions/ADR-003-database.md`](./docs/decisions/ADR-003-database.md) |

---

## Running Tests

```bash
# Unit tests
dotnet test tests/MechanicsSoftware.UnitTests

# Integration tests
dotnet test tests/MechanicsSoftware.IntegrationTests

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Contributing

See [`CLAUDE.md`](./CLAUDE.md) for project conventions, commit message format, and branch naming rules.
