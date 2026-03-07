# Project Checklist — Tech Challenge Phase 1

Progress tracker for the FIAP POS Tech 15SOAT challenge.

---

## Setup

- [x] Git repository created
- [x] Branch protection (1 PR approval required on `main`)
- [x] `CLAUDE.md` — project conventions defined
- [ ] Install .NET 8 SDK on all machines
- [ ] Scaffold .NET solution (`MechanicsSoftware.sln`)

---

## DDD Documentation (`docs/`)

- [x] Ubiquitous Language
- [x] Event Storming — Service Order flow
- [x] Event Storming — Inventory flow
- [x] Aggregates, Entities and Value Objects
- [x] Bounded Contexts
- [ ] Visual diagrams (aggregate map, entity relationships)
- [ ] Miro board or equivalent with visual Event Storming

---

## Architecture Decisions (`docs/decisions/`)

- [x] ADR-001 — Tech stack (C# / ASP.NET Core 8)
- [x] ADR-002 — Modular monolith architecture
- [x] ADR-003 — PostgreSQL

---

## Domain Layer (`MechanicsSoftware.Domain`)

- [ ] Base classes: `Entity`, `ValueObject`, `DomainException`
- [ ] Value Objects: `TaxId` (CPF/CNPJ), `LicensePlate`, `Money`, `Email`
- [ ] `Customer` entity
- [ ] `Vehicle` entity
- [ ] `Service` entity
- [ ] `Part` entity + `StockMovement`
- [ ] `ServiceOrder` aggregate root + state machine
- [ ] `ServiceItem` and `PartItem` entities
- [ ] `Budget` entity
- [ ] Repository interfaces

---

## Application Layer (`MechanicsSoftware.Application`)

- [ ] Auth: `LoginUseCase`
- [ ] Customers: `CreateCustomer`, `UpdateCustomer`, `DeleteCustomer`, `GetCustomer`, `ListCustomers`
- [ ] Vehicles: `CreateVehicle`, `UpdateVehicle`, `DeleteVehicle`, `GetVehicle`, `ListVehicles`
- [ ] Services: `CreateService`, `UpdateService`, `DeleteService`, `GetService`, `ListServices`
- [ ] Parts: `CreatePart`, `UpdatePart`, `DeletePart`, `GetPart`, `ListParts`, `UpdateStock`
- [ ] Service Orders: `CreateServiceOrder`, `AddService`, `AddPart`, `GenerateBudget`, `SendBudget`
- [ ] Service Orders: `Approve`, `Reject`, `StartDiagnosis`, `StartExecution`, `Complete`, `Deliver`
- [ ] Service Orders: `GetStatus` (public), `GetServiceOrder`, `ListServiceOrders`
- [ ] Metrics: `GetAverageExecutionTime`

---

## Infrastructure Layer (`MechanicsSoftware.Infrastructure`)

- [ ] `AppDbContext` with EF Core configuration
- [ ] EF Core entity configurations (Fluent API, snake_case)
- [ ] Migrations (initial schema)
- [ ] Repository implementations
- [ ] JWT provider
- [ ] Password hasher (BCrypt)

---

## API Layer (`MechanicsSoftware.API`)

- [ ] `AuthController` — `POST /api/auth/login`
- [ ] `CustomersController` — full CRUD
- [ ] `VehiclesController` — full CRUD
- [ ] `ServicesController` — full CRUD
- [ ] `PartsController` — full CRUD + stock update
- [ ] `ServiceOrdersController` — full flow + public status endpoint
- [ ] Swagger configured and documented
- [ ] JWT authentication middleware
- [ ] Global exception handling middleware
- [ ] `appsettings.json` / environment variable configuration

---

## Tests

- [ ] Unit tests — `TaxId` value object (CPF/CNPJ validation)
- [ ] Unit tests — `LicensePlate` value object
- [ ] Unit tests — `Money` value object
- [ ] Unit tests — `ServiceOrder` state machine
- [ ] Unit tests — budget generation
- [ ] Unit tests — stock reservation and deduction
- [ ] Integration tests — customer CRUD endpoints
- [ ] Integration tests — service order full flow
- [ ] Integration tests — inventory flow
- [ ] 80%+ coverage on Domain and Application layers verified

---

## Infrastructure / DevOps

- [ ] `Dockerfile`
- [ ] `docker-compose.yml` (API + PostgreSQL)
- [ ] `README.md` — setup instructions, how to run, endpoints overview

---

## Security

- [ ] Vulnerability scan (Snyk or `dotnet` security tooling)
- [ ] Vulnerability analysis report written

---

## Final Deliverables

- [ ] Add `soat-architecture` as repository collaborator
- [ ] DDD documentation link (Miro or docs/)
- [ ] Repository link shared
- [ ] Vulnerability report included
- [ ] Delivery PDF:
  - [ ] Group name
  - [ ] Participants and Discord usernames
  - [ ] Documentation link
  - [ ] Repository link
  - [ ] Vulnerability report
- [ ] Demo video recorded (max 15 minutes)
