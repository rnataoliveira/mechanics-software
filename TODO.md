# Project Checklist — Tech Challenge Phase 1

Progress tracker for the FIAP POS Tech 15SOAT challenge.

---

## Setup

- [x] Git repository created
- [x] Branch protection (1 PR approval required on `main`)
- [x] `CONTRIBUTING.md` — project conventions defined
- [x] Install .NET 8 SDK on all machines
- [x] Scaffold .NET solution (`MechanicsSoftware.sln` + 4 projects + 2 test projects)

---

## DDD Documentation (`docs/`)

- [x] Ubiquitous Language
- [x] Event Storming — Service Order flow (text + Mermaid)
- [x] Event Storming — Inventory flow (text + Mermaid)
- [x] Aggregates, Entities and Value Objects + ER diagram
- [x] Bounded Contexts + context map diagram
- [x] Architecture overview + request flow diagram
- [x] Domain Storytelling — all flows (9 stories with sequence diagrams)
- [ ] Visual diagrams — aggregate map (classDiagram)

---

## Architecture Decisions (`docs/decisions/`)

- [x] ADR-001 — Tech stack (C# / ASP.NET Core 8)
- [x] ADR-002 — Vertical Slice Architecture + DDD domain (no MediatR, no repository pattern)
- [x] ADR-003 — PostgreSQL

---

## Domain (`MechanicsSoftware.Domain`)

- [x] Shared: `Entity`, `ValueObject`, `DomainException`, `Money`
- [x] Value Objects: `TaxId` (CPF/CNPJ), `LicensePlate`, `Email`
- [x] `Customer` entity
- [x] `Vehicle` entity
- [x] `Service` entity
- [x] `Part` entity + `StockMovement` entity
- [x] `ServiceOrder` aggregate root + state machine (`ServiceOrderStatus`)
- [x] `ServiceItem` and `PartItem` entities
- [x] `Budget` entity

---

## Application (`MechanicsSoftware.Application`)

### Common
- [x] `IAppDbContext` interface
- [x] `NotFoundException`

### Features — Auth
- [x] `LoginUseCase`

### Features — Customers
- [x] `CreateCustomerUseCase`
- [x] `UpdateCustomerUseCase`
- [x] `DeleteCustomerUseCase`
- [x] `GetCustomerUseCase`
- [x] `ListCustomersUseCase`

### Features — Vehicles
- [x] `CreateVehicleUseCase`
- [x] `UpdateVehicleUseCase`
- [x] `DeleteVehicleUseCase`
- [x] `GetVehicleUseCase`
- [x] `ListVehiclesUseCase`

### Features — Services
- [x] `CreateServiceUseCase`
- [x] `UpdateServiceUseCase`
- [x] `DeleteServiceUseCase`
- [x] `GetServiceUseCase`
- [x] `ListServicesUseCase`

### Features — Inventory
- [x] `CreatePartUseCase`
- [x] `UpdatePartUseCase`
- [x] `DeletePartUseCase`
- [x] `GetPartUseCase`
- [x] `ListPartsUseCase`
- [x] `UpdateStockUseCase`

### Features — Service Orders
- [x] `CreateServiceOrderUseCase`
- [x] `AddServiceItemUseCase`
- [x] `AddPartItemUseCase`
- [x] `GenerateBudgetUseCase`
- [x] `SendBudgetUseCase`
- [x] `StartDiagnosisUseCase`
- [x] `ApproveServiceOrderUseCase`
- [x] `RejectServiceOrderUseCase`
- [x] `StartExecutionUseCase`
- [x] `CompleteServiceOrderUseCase`
- [x] `DeliverServiceOrderUseCase`
- [x] `GetServiceOrderStatusUseCase` ← public (no JWT)
- [x] `GetServiceOrderUseCase`
- [x] `ListServiceOrdersUseCase`
- [x] `GetAverageExecutionTimeUseCase`

---

## Infrastructure (`MechanicsSoftware.Infrastructure`)

- [x] `AppDbContext` implementing `IAppDbContext`
- [x] EF Core Fluent API configurations (snake_case table/column names)
- [x] Initial migration + subsequent migrations
- [x] `JwtProvider`
- [x] `PasswordHasher` (BCrypt)
- [x] `DatabaseSeeder`

---

## API (`MechanicsSoftware.API`)

- [x] `AuthController`
- [x] `CustomersController`
- [x] `VehiclesController`
- [x] `ServicesController`
- [x] `PartsController`
- [x] `ServiceOrdersController`
- [x] Swagger configured and documented
- [x] JWT middleware + `[Authorize]` on protected routes
- [x] Global exception handling middleware
- [x] `appsettings.json` with environment variable support
- [x] DI registration for all use cases

---

## Tests

### Unit — Domain
- [x] `TaxIdTests` — CPF and CNPJ validation
- [x] `LicensePlateTests` — Mercosul and legacy formats
- [x] `MoneyTests` — arithmetic operations
- [x] `ServiceOrderStatusTests` — valid and invalid transitions
- [x] `ServiceOrderTests` — business rules (add items, generate budget, state machine)
- [x] `BudgetStatusTests` — status transitions
- [x] `PartTests` — stock rules (no negative stock)

### Unit — Application
- [x] `CreateServiceOrderUseCaseTests`
- [x] `ApproveServiceOrderUseCaseTests`
- [x] `RejectServiceOrderUseCaseTests`
- [x] `GenerateBudgetUseCaseTests`
- [x] `UpdateStockUseCaseTests`
- [x] All remaining use case tests (customers, vehicles, services, inventory, auth)

### Integration
- [ ] Customers CRUD endpoints
- [ ] Service order full flow (create → approve → complete → deliver)
- [ ] Inventory flow (stock reservation and deduction)

### Coverage
- [x] 96.9% on Domain and Application layers (exceeds 80% minimum)
- [x] Coverage report published to GitHub Pages

---

## DevOps

- [x] `Dockerfile`
- [x] `docker-compose.yml` (API + PostgreSQL)
- [x] `README.md` — setup, how to run, env vars, endpoints

---

## Security

- [x] Vulnerability scan (`dotnet list package --vulnerable --include-transitive`)
- [x] Vulnerability analysis report written (`DELIVERABLES.md`)
- [x] Security report published to GitHub Pages (`/security/`)

---

## Final Deliverables

- [ ] Add `soat-architecture` as repository collaborator (GitHub Settings → Collaborators)
- [x] DDD documentation in `docs/` (repository)
- [x] Repository link confirmed
- [x] Vulnerability report complete (`DELIVERABLES.md`)
- [ ] Delivery PDF / `DELIVERABLES.md` finalization:
  - [ ] Group name (fill in `DELIVERABLES.md` line 9)
  - [x] Participants and Discord usernames
  - [ ] DDD documentation link (fill in `DELIVERABLES.md` — Miro board URL or docs/ link)
  - [x] Repository link
  - [x] Vulnerability report
- [ ] Demo video recorded (max 15 minutes) — link pending in `DELIVERABLES.md`
