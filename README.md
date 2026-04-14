# Mechanics Software

Backend system for a mechanic shop — built as the Phase 1 Tech Challenge for FIAP POS Tech (15SOAT).

## Overview

A RESTful API that manages the full lifecycle of service orders, customers, vehicles, parts, and inventory for a medium-sized auto repair shop.

**Architecture:** Vertical Slice Architecture + DDD Domain
**Stack:** C# 12 · ASP.NET Core 8 · PostgreSQL 16 · Entity Framework Core 9
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

### Run locally (step by step)

**1. Start the database**

```bash
docker compose up db -d
```

PostgreSQL will be available at `localhost:5435`.

**2. Install the local EF Core CLI tool**

```bash
dotnet tool restore
```

**3. Apply migrations**

```bash
dotnet dotnet-ef database update \
  --project src/MechanicsSoftware.Infrastructure \
  --startup-project src/MechanicsSoftware.API
```

**4. Run the API**

```bash
dotnet run --project src/MechanicsSoftware.API
```

Swagger UI: `http://localhost:5066/swagger`

On first startup the application automatically creates a default admin user.

---

## Authentication

### Default admin credentials

| Field | Value |
|---|---|
| Email | `admin@mechanics.local` |
| Password | `Admin@123` |

Override with `SEED_ADMIN_EMAIL` and `SEED_ADMIN_PASSWORD` environment variables.

### Getting a token

Call `POST /api/auth/login` with the credentials above:

```json
{
  "email": "admin@mechanics.local",
  "password": "Admin@123"
}
```

Copy the `token` from the response.

### Using the token in Swagger

1. Open `http://localhost:5066/swagger`
2. Click the **Authorize** button (🔒)
3. Paste the token value and click **Authorize**

All protected endpoints will now work.

---

### Environment variables

| Variable | Required | Description | Default |
|---|---|---|---|
| `JWT_SECRET` | **Yes** | Secret key for JWT signing (min 32 chars) | — |
| `DATABASE_URL` | No | PostgreSQL connection string | see `appsettings.Development.json` |
| `JWT_EXPIRATION_MINUTES` | No | Token expiration time in minutes | `60` |
| `BCRYPT_SALT_ROUNDS` | No | BCrypt cost factor | `12` |
| `SEED_ADMIN_EMAIL` | No | Default admin email | `admin@mechanics.local` |
| `SEED_ADMIN_PASSWORD` | No | Default admin password | `Admin@123` |

---

## Database Migrations

The project uses a local `dotnet-ef` tool pinned in `.config/dotnet-tools.json`. Run `dotnet tool restore` once to install it, then use `dotnet dotnet-ef` instead of `dotnet ef`.

### Apply existing migrations

```bash
dotnet dotnet-ef database update \
  --project src/MechanicsSoftware.Infrastructure \
  --startup-project src/MechanicsSoftware.API
```

### Add a new migration

```bash
dotnet dotnet-ef migrations add <MigrationName> \
  --project src/MechanicsSoftware.Infrastructure \
  --startup-project src/MechanicsSoftware.API \
  --output-dir Persistence/Migrations
```

### Remove the last migration (if not yet applied)

```bash
dotnet dotnet-ef migrations remove \
  --project src/MechanicsSoftware.Infrastructure \
  --startup-project src/MechanicsSoftware.API
```

---

## API

### Public endpoints (no authentication)

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate and receive JWT |

### Protected endpoints (JWT required)

| Resource | Endpoints |
|---|---|
| Customers | `GET/POST /api/customers` · `GET/PUT/DELETE /api/customers/{id}` |
| Vehicles | `GET/POST /api/vehicles` · `GET/PUT/DELETE /api/vehicles/{id}` |
| Parts | `GET/POST /api/parts` · `GET/PUT/DELETE /api/parts/{id}` · `PATCH /api/parts/{id}/stock` |

> **Not yet implemented:** Services and Service Orders endpoints are planned for the next milestone.

Full documentation and request/response schemas available at `/swagger` when running.

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
| ADR-004 Application Layer | [`docs/decisions/ADR-004-application-layer-conventions.md`](./docs/decisions/ADR-004-application-layer-conventions.md) |

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

See [`CONTRIBUTING.md`](./CONTRIBUTING.md) for project conventions, commit message format, and branch naming rules.
