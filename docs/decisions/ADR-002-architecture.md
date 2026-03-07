# ADR-002: Modular Monolith Layered Architecture

**Status:** Accepted
**Date:** 2026-03-06

## Context

The challenge requires a monolithic backend with DDD applied. Since this is an MVP, the operational complexity of microservices is unjustified. However, a "big ball of mud" monolith would not demonstrate design maturity.

## Decision

Adopt a **Modular Monolith** organized by bounded contexts, with internal layers per module:

```
src/
  modules/
    customers/
      application/    # use cases, DTOs
      domain/         # entities, value objects, repository interfaces
      infrastructure/ # Prisma repositories, mappers
      presentation/   # controllers, Swagger decorators
    vehicles/         # same structure
    service-orders/   # same structure
    inventory/        # same structure
  shared/
    domain/           # base entities, exceptions, shared value objects
    infrastructure/   # Prisma client, JWT provider
  main.ts
```

## Bounded Contexts

| Context | Responsibility |
|---|---|
| **Customers** | Customer registration and identification (CPF/CNPJ) |
| **Vehicles** | Vehicle registration and customer linking |
| **Service Orders** | Full OS lifecycle: creation, status, budget, approval |
| **Inventory** | Parts, supplies, stock control, reservation, movements |

## Layer Dependency Flow

```
Presentation → Application → Domain ← Infrastructure
```

- **Domain** depends on nothing external (no framework, no ORM)
- **Infrastructure** implements interfaces defined in Domain
- **Application** orchestrates use cases using Domain + repositories
- **Presentation** exposes HTTP and converts request/response DTOs

## Rationale

- Demonstrates DDD application even within a monolith
- Makes future migration to microservices easier (contexts are already isolated)
- NestJS natively supports modules with this separation
- Aligned with what the challenge requires: "monolith with layers"

## Alternatives Considered

| Alternative | Reason Not Chosen |
|---|---|
| Organize by type (controllers/, services/, repositories/) | Does not demonstrate DDD; hard to maintain at scale |
| Strict Clean Architecture | Excessive overhead for MVP within the given deadline |
| Microservices | Not allowed by the challenge requirements |
