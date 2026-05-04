# Mechanics Software

[![CI](https://github.com/rnataoliveira/mechanics-software/actions/workflows/coverage.yml/badge.svg)](https://github.com/rnataoliveira/mechanics-software/actions/workflows/coverage.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=rnataoliveira_mechanics-software&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rnataoliveira_mechanics-software)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=rnataoliveira_mechanics-software&metric=coverage)](https://sonarcloud.io/summary/new_code?id=rnataoliveira_mechanics-software)

Backend system for a mechanic shop — built as the Phase 1 Tech Challenge for FIAP POS Tech (15SOAT) by group **TorqueOS**.

**Architecture:** Vertical Slice Architecture + DDD Domain  
**Stack:** C# 12 · ASP.NET Core 8 · PostgreSQL 16 · Entity Framework Core 9  
**Docs:** [`/docs`](./docs)

---

## Demo

[YouTube](https://youtu.be/vqERT_zrLpo) · [Google Drive (fallback)](https://drive.google.com/drive/folders/1oj1IXAMRfgA8g7ux_z_ZEq06ieYLf0PZ?usp=sharing)

---

## Quick Start

```bash
docker compose up --build
```

| | |
|---|---|
| API | `http://localhost:8080` |
| Swagger UI | `http://localhost:8080/swagger` |
| Admin email | `admin@mechanics.local` |
| Admin password | `Admin@123` |

On first startup the application automatically applies all pending migrations and seeds the database (see below). No extra steps needed.

---

## Seed Data

Every startup resets domain data and seeds the following records so the API is ready to use immediately:

**Services**

| Name | Price | Duration |
|---|---|---|
| Troca de Óleo | R$ 90,00 | 60 min |
| Alinhamento e Balanceamento | R$ 150,00 | 90 min |
| Revisão dos Freios | R$ 250,00 | 120 min |

**Parts**

| Code | Name | Price | Stock |
|---|---|---|---|
| OL-5W30 | Óleo Motor 5W30 | R$ 45,00 | 100 |
| FILT-AR-001 | Filtro de Ar | R$ 35,00 | 80 |
| PAST-FREIO | Pastilha de Freio Dianteira | R$ 80,00 | 60 |

**Customers & Vehicles**

| Customer | Vehicle |
|---|---|
| Carlos Silva | Toyota Corolla 2022 — ABC1234 |
| Ana Souza | Honda Civic 2021 — DEF5678 |
| Roberto Mendes | Renault Sandero 2020 — GHI9012 |

**Admin user** — survives restarts (not wiped). Override credentials with `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` env vars.

---

## Authentication

Call `POST /api/auth/login`:

```json
{
  "email": "admin@mechanics.local",
  "password": "Admin@123"
}
```

Copy the `token` from the response. In Swagger, click **Authorize** (🔒) and paste it.

---

## Environment Variables

| Variable | Required | Default |
|---|---|---|
| `JWT_SECRET` | Yes (production) | pre-configured for local dev |
| `DATABASE_URL` | No | see `appsettings.Development.json` |
| `JWT_EXPIRATION_MINUTES` | No | `60` |
| `BCRYPT_SALT_ROUNDS` | No | `12` |
| `SEED_ADMIN_EMAIL` | No | `admin@mechanics.local` |
| `SEED_ADMIN_PASSWORD` | No | `Admin@123` |

---

## Run Locally (without Docker)

**1. Start the database**

```bash
docker compose up db -d
```

PostgreSQL available at `localhost:5435`.

**2. Run the API**

```bash
dotnet run --project src/MechanicsSoftware.API/MechanicsSoftware.API.csproj
```

Swagger UI: `http://localhost:5066/swagger`

---

## API

### Public endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate and receive JWT |
| `GET` | `/api/service-orders/{id}/status` | Check service order status (for customers) |

### Protected endpoints (JWT required)

| Resource | Endpoints |
|---|---|
| Customers | `GET/POST /api/customers` · `GET/PUT/DELETE /api/customers/{id}` |
| Vehicles | `GET/POST /api/vehicles` · `GET/PUT/DELETE /api/vehicles/{id}` |
| Parts | `GET/POST /api/parts` · `GET/PUT/DELETE /api/parts/{id}` · `PATCH /api/parts/{id}/stock` |
| Services | `GET/POST /api/services` · `GET/PUT/DELETE /api/services/{id}` |
| Service Orders | `GET/POST /api/service-orders` · full lifecycle via action endpoints |

Full schemas available at `/swagger`.

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
  MechanicsSoftware.Domain/          # Entities, value objects, business rules
  MechanicsSoftware.Application/     # Use cases organized by feature (VSA)
  MechanicsSoftware.Infrastructure/  # EF Core, JWT, BCrypt
  MechanicsSoftware.API/             # Controllers, middleware, Swagger

tests/
  MechanicsSoftware.UnitTests/
  MechanicsSoftware.IntegrationTests/
```

See [`docs/architecture/overview.md`](./docs/architecture/overview.md) for full details.

---

## Running Tests

```bash
# Unit tests
dotnet test tests/MechanicsSoftware.UnitTests

# With coverage report
dotnet test tests/MechanicsSoftware.UnitTests \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  --results-directory ./coverage-results
```

Interactive coverage report: [rnataoliveira.github.io/mechanics-software](https://rnataoliveira.github.io/mechanics-software/)

---

## Database Migrations

The project uses a local `dotnet-ef` tool pinned in `.config/dotnet-tools.json`. Run `dotnet tool restore` once, then use `dotnet dotnet-ef`.

```bash
# Apply existing migrations
dotnet dotnet-ef database update \
  --project src/MechanicsSoftware.Infrastructure/MechanicsSoftware.Infrastructure.csproj \
  --startup-project src/MechanicsSoftware.API/MechanicsSoftware.API.csproj

# Add a new migration
dotnet dotnet-ef migrations add <MigrationName> \
  --project src/MechanicsSoftware.Infrastructure/MechanicsSoftware.Infrastructure.csproj \
  --startup-project src/MechanicsSoftware.API/MechanicsSoftware.API.csproj \
  --output-dir Persistence/Migrations
```

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

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md) for project conventions, commit message format, and branch naming rules.
