# ADR-004: Application Layer Conventions

**Status:** Accepted
**Date:** 2026-03-30

## Context

After the initial implementation of the Application layer, team members raised questions about three design choices:

1. Why is the folder named `Features/` instead of `UseCases/`?
2. Why do use cases access `IAppDbContext` directly instead of going through `IRepository<T>` interfaces?
3. Why are C# Records used for request/response types instead of DTO classes, and why do they live in the same feature folder instead of a separate project?

This ADR documents the rationale behind each of these decisions.

---

## Decision 1 — Folder named `Features/`, not `UseCases/`

### Decision

Keep the folder named `Features/`.

### Rationale

The project follows **Vertical Slice Architecture (VSA)** — see ADR-002. In VSA, the primary organizing unit is a *feature* (a vertical slice of the system), not a technical layer. Each folder under `Features/` groups everything that belongs to that slice: the use case class, its input record, and its output record.

The name `UseCases/` would be accurate too, but `Features/` is the established convention in VSA literature and community (popularized by Jimmy Bogard). Using `Features/` makes the architectural intent explicit: this is VSA, not Classic Clean Architecture.

```
Application/
  Features/              ← vertical slices, each self-contained
    Customers/
      CreateCustomerUseCase.cs
      CreateCustomerRequest.cs  (record)
      CustomerResponse.cs       (record)
    ServiceOrders/
      ...
  Common/                ← shared interfaces and exceptions
    IAppDbContext.cs
```

In Clean Architecture, `UseCases/` would be the right name because it sits in an Application layer organized horizontally by technical concern. Here the organization is vertical, so `Features/` better communicates the approach.

---

## Decision 2 — Use cases access `IAppDbContext` directly (no `IRepository<T>`)

### Decision

Use cases receive `IAppDbContext` via constructor injection and query `DbSet<T>` directly. No per-aggregate repository interfaces (`ICustomerRepository`, `IServiceOrderRepository`, etc.) are introduced.

### Rationale

**EF Core already implements Repository + Unit of Work.**
`DbSet<T>` is a repository for `T`. `SaveChangesAsync` is the unit of work commit. Adding `IRepository<Customer>` on top of EF Core creates a double abstraction — the repository calls the context, which calls the database. The extra layer adds boilerplate and indirection with no practical benefit for this project.

**`IAppDbContext` is the testable seam.**
Unit and integration tests can substitute `IAppDbContext` with an in-memory or SQLite-backed `DbContext`. This gives the same isolation guarantee that a repository mock would provide, without the overhead of defining and maintaining per-aggregate interfaces.

**When `IRepository<T>` is justified in enterprise systems:**

| Scenario | Why IRepository helps |
|---|---|
| ORM replacement (EF → Dapper) | Repository hides the query mechanism; Application layer unchanged |
| Multi-tenancy or complex caching | Repository centralizes cross-cutting data-access logic |
| Large teams with strict slice ownership | Per-context interfaces prevent unintended cross-slice DB access |

None of these scenarios apply to this project (Phase 1 MVP, single ORM, small team). The trade-off is documented in ADR-002 under *IAppDbContext trade-off*.

---

## Decision 3 — C# Records for request/response types, co-located with the use case

### Decision

Use C# `record` types (not classes) for request and response types. Keep them in the same file or folder as the use case that owns them — not in a separate `Contracts` or `DTOs` project.

### Rationale

**Records are the idiomatic C# replacement for DTO classes.**

Before C# 9, DTOs required verbose class definitions:

```csharp
// Classic DTO class — requires manual Equals/GetHashCode for value equality
public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

C# Records provide the same semantics with much less boilerplate:

```csharp
// Record — immutable, value equality, deconstruction, ToString — one line
public sealed record CustomerResponse(Guid Id, string Name, string Email);
```

Key advantages of records for request/response types:

| Property | Class (DTO) | Record |
|---|---|---|
| Immutability | Manual (`init` setters) | Built-in (positional) |
| Value equality | Manual (`Equals` override) | Automatic |
| Conciseness | Verbose | Compact positional syntax |
| Deconstruction | Manual | Automatic |
| Serialization | Supported | Supported (same as class) |

**Co-location with the use case.**
In VSA, cohesion is by feature, not by technical type. Separating `CustomerResponse` into a `Contracts` project means a developer working on the `Customers` feature must navigate to a different project to find or change that type. Co-location keeps everything about a feature in one place.

A separate `Contracts` project is useful when:
- Multiple client assemblies (e.g., a separate Frontend BFF and a Mobile API) need to share the same types.
- You are publishing a NuGet package of public API contracts.

Neither applies here — the API is the only consumer, and it lives in the same solution.

---

## Consequences

- The `Features/` naming must be consistent across the Application project. Do not mix `UseCases/` and `Features/` folder names.
- New use cases must not introduce per-aggregate repository interfaces. If a use case needs scoped DB access, consider a context-scoped interface (`ICustomersDbContext`) as described in ADR-002.
- Records should be used for all new request/response types. Mutable reference types (`class`) should not be used for data-transfer purposes.
- If the solution ever gains multiple consuming assemblies that need shared contracts, extract a `MechanicsSoftware.Contracts` project at that point — not before.
