# Project Conventions — mechanics-software

## Language
- Everything is in **English**: code, docs, comments, commit messages, PR titles, variable names, file names.
- Exception: business domain terms with no accurate English equivalent (e.g. "CPF", "CNPJ") are kept as-is.

## Stack
- **Language:** C# 12
- **Framework:** ASP.NET Core 8
- **ORM:** Entity Framework Core 8 + Npgsql (PostgreSQL)
- **Auth:** JWT via Microsoft.AspNetCore.Authentication.JwtBearer
- **Docs:** Swagger via Swashbuckle.AspNetCore
- **Testing:** xUnit + Moq + FluentAssertions
- **Container:** Docker + docker-compose

## Commit Messages
- Follow **Conventional Commits** (https://www.conventionalcommits.org)
- Format: `<type>(<scope>): <description>`
- Types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, `ci`
- Scopes: `auth`, `customers`, `vehicles`, `service-orders`, `inventory`, `shared`, `infra`, `docs`
- Description: imperative, lowercase, no period at the end
- No co-author lines
- Examples:
  - `feat(service-orders): add status transition state machine`
  - `fix(inventory): prevent negative stock on part confirmation`
  - `test(customers): add unit tests for TaxId value object`
  - `chore(infra): add docker-compose configuration`

## Naming Conventions

### C# Code
- Classes, interfaces, records: `PascalCase`
- Methods and properties: `PascalCase`
- Private fields: `_camelCase`
- Local variables and parameters: `camelCase`
- Constants: `PascalCase` (or `UPPER_SNAKE_CASE` for true constants)
- Interfaces: prefix with `I` (e.g. `ICustomerRepository`)

### Files
- One class per file, file name matches class name
- Test files: `CustomerTests.cs`, `ServiceOrderTests.cs`

### Database (EF Core)
- Tables: `snake_case` plural (e.g. `customers`, `service_orders`)
- Columns: `snake_case` (e.g. `created_at`, `customer_id`)
- Configured via EF Core Fluent API in `*Configuration.cs` files

### API
- Routes: `kebab-case` (e.g. `/api/service-orders/{id}/start-diagnosis`)
- JSON properties: `camelCase`

## Architecture Rules
- `Domain` project has **zero** external dependencies (no EF Core, no ASP.NET)
- Business rules and invariants live in Domain entities — never in controllers or use cases
- Value Objects validate themselves on construction; invalid state must be impossible
- Repository interfaces defined in Domain, implemented in Infrastructure
- Use cases in Application call domain entities and repository interfaces only
- Controllers in API call use cases only; no business logic in controllers
- Throw `DomainException` (or a subclass) for business rule violations

## Project Structure
```
src/
  MechanicsSoftware.Domain/
  MechanicsSoftware.Application/
  MechanicsSoftware.Infrastructure/
  MechanicsSoftware.API/
tests/
  MechanicsSoftware.UnitTests/
  MechanicsSoftware.IntegrationTests/
```
See `docs/architecture/overview.md` for full details.

## Testing
- Unit tests: pure domain and application logic, no infrastructure
- Integration tests: full HTTP request through real DB (use TestContainers or in-memory)
- Minimum 80% coverage on Domain and Application layers
- Test method naming: `MethodName_Condition_ExpectedResult`
  - e.g. `Approve_WhenStatusIsAwaitingApproval_ChangesStatusToInExecution`

## Git Workflow
- `main` is protected: requires 1 PR approval, no force pushes
- Branch naming: `<type>/<short-description>`
  - e.g. `feat/create-service-order`, `fix/stock-reservation`, `chore/docker-setup`
- Always open a PR to merge into `main`
- Squash commits when merging if the branch has noisy history
