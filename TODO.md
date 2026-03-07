# Project Checklist — Tech Challenge Phase 1

Progress tracker for the FIAP POS Tech 15SOAT challenge.

---

## Setup

- [x] Git repository created
- [x] Branch protection (1 PR approval required on `main`)
- [x] `CLAUDE.md` — project conventions defined
- [ ] Install .NET 8 SDK on all machines
- [ ] Scaffold .NET solution (`MechanicsSoftware.sln` + 4 projects + 2 test projects)

---

## DDD Documentation (`docs/`)

- [x] Ubiquitous Language
- [x] Event Storming — Service Order flow (text + Mermaid)
- [x] Event Storming — Inventory flow (text + Mermaid)
- [x] Aggregates, Entities and Value Objects + ER diagram
- [x] Bounded Contexts + context map diagram
- [x] Architecture overview + request flow diagram
- [ ] Visual diagrams — aggregate map (classDiagram)

---

## Architecture Decisions (`docs/decisions/`)

- [x] ADR-001 — Tech stack (C# / ASP.NET Core 8)
- [x] ADR-002 — Vertical Slice Architecture + DDD domain (no MediatR, no repository pattern)
- [x] ADR-003 — PostgreSQL

---

## Domain (`MechanicsSoftware.Domain`)

- [ ] Shared: `Entity`, `ValueObject`, `DomainException`, `Money`
- [ ] Value Objects: `TaxId` (CPF/CNPJ), `LicensePlate`, `Email`
- [ ] `Customer` entity
- [ ] `Vehicle` entity
- [ ] `Service` entity
- [ ] `Part` entity + `StockMovement` entity
- [ ] `ServiceOrder` aggregate root + state machine (`ServiceOrderStatus`)
- [ ] `ServiceItem` and `PartItem` entities
- [ ] `Budget` entity

---

## Application (`MechanicsSoftware.Application`)

### Common
- [ ] `IAppDbContext` interface
- [ ] `NotFoundException`

### Features — Auth
- [ ] `LoginUseCase`

### Features — Customers
- [ ] `CreateCustomerUseCase`
- [ ] `UpdateCustomerUseCase`
- [ ] `DeleteCustomerUseCase`
- [ ] `GetCustomerUseCase`
- [ ] `ListCustomersUseCase`

### Features — Vehicles
- [ ] `CreateVehicleUseCase`
- [ ] `UpdateVehicleUseCase`
- [ ] `DeleteVehicleUseCase`
- [ ] `GetVehicleUseCase`
- [ ] `ListVehiclesUseCase`

### Features — Services
- [ ] `CreateServiceUseCase`
- [ ] `UpdateServiceUseCase`
- [ ] `DeleteServiceUseCase`
- [ ] `GetServiceUseCase`
- [ ] `ListServicesUseCase`

### Features — Inventory
- [ ] `CreatePartUseCase`
- [ ] `UpdatePartUseCase`
- [ ] `DeletePartUseCase`
- [ ] `GetPartUseCase`
- [ ] `ListPartsUseCase`
- [ ] `UpdateStockUseCase`

### Features — Service Orders
- [ ] `CreateServiceOrderUseCase`
- [ ] `AddServiceItemUseCase`
- [ ] `AddPartItemUseCase`
- [ ] `GenerateBudgetUseCase`
- [ ] `SendBudgetUseCase`
- [ ] `StartDiagnosisUseCase`
- [ ] `ApproveServiceOrderUseCase`
- [ ] `RejectServiceOrderUseCase`
- [ ] `StartExecutionUseCase`
- [ ] `CompleteServiceOrderUseCase`
- [ ] `DeliverServiceOrderUseCase`
- [ ] `GetServiceOrderStatusUseCase` ← public (no JWT)
- [ ] `GetServiceOrderUseCase`
- [ ] `ListServiceOrdersUseCase`
- [ ] `GetAverageExecutionTimeUseCase`

---

## Infrastructure (`MechanicsSoftware.Infrastructure`)

- [ ] `AppDbContext` implementing `IAppDbContext`
- [ ] EF Core Fluent API configurations (snake_case table/column names)
- [ ] Initial migration
- [ ] `JwtProvider`
- [ ] `PasswordHasher` (BCrypt)

---

## API (`MechanicsSoftware.API`)

- [ ] `AuthController`
- [ ] `CustomersController`
- [ ] `VehiclesController`
- [ ] `ServicesController`
- [ ] `PartsController`
- [ ] `ServiceOrdersController`
- [ ] Swagger configured and documented
- [ ] JWT middleware + `[Authorize]` on protected routes
- [ ] Global exception handling middleware
- [ ] `appsettings.json` with environment variable support
- [ ] DI registration for all use cases

---

## Tests

### Unit — Domain
- [ ] `TaxIdTests` — CPF and CNPJ validation
- [ ] `LicensePlateTests` — Mercosul and legacy formats
- [ ] `MoneyTests` — arithmetic operations
- [ ] `ServiceOrderStatusTests` — valid and invalid transitions
- [ ] `ServiceOrderTests` — business rules (add items, generate budget, state machine)
- [ ] `BudgetTests` — total calculation
- [ ] `PartTests` — stock rules (no negative stock)

### Unit — Application
- [ ] `CreateServiceOrderUseCaseTests`
- [ ] `ApproveServiceOrderUseCaseTests`
- [ ] `RejectServiceOrderUseCaseTests`
- [ ] `GenerateBudgetUseCaseTests`
- [ ] `UpdateStockUseCaseTests`

### Integration
- [ ] Customers CRUD endpoints
- [ ] Service order full flow (create → approve → complete → deliver)
- [ ] Inventory flow (stock reservation and deduction)

### Coverage
- [ ] 80%+ on Domain and Application layers verified

---

## DevOps

- [ ] `Dockerfile`
- [ ] `docker-compose.yml` (API + PostgreSQL)
- [ ] `README.md` — setup, how to run, env vars, endpoints

---

## Security

- [ ] Vulnerability scan (Snyk or `dotnet list package --vulnerable`)
- [ ] Vulnerability analysis report written

---

## Final Deliverables

- [ ] Add `soat-architecture` as repository collaborator
- [ ] DDD documentation link confirmed
- [ ] Repository link confirmed
- [ ] Vulnerability report complete
- [ ] Delivery PDF:
  - [ ] Group name
  - [ ] Participants and Discord usernames
  - [ ] Documentation link
  - [ ] Repository link
  - [ ] Vulnerability report
- [ ] Demo video recorded (max 15 minutes)
