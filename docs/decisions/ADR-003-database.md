# ADR-003: Database — PostgreSQL

**Status:** Accepted
**Date:** 2026-03-06

## Context

The challenge allows free database choice but requires justification. The system domain involves entities with strong relationships (Customer → Vehicle → OS → Items → Parts), transactional control (stock), and movement history.

## Decision

Use **PostgreSQL 16** as the primary database.

## Technical Justification

| Criterion | PostgreSQL |
|---|---|
| Relationships | Native FK, JOIN, and referential integrity support |
| Transactions | Full ACID — essential for stock deductions and status transitions |
| Constraints | CHECK constraints for document and status validation |
| Concurrency | MVCC avoids unnecessary locks on concurrent reads |
| Ecosystem | Excellent support via Entity Framework Core + Npgsql; mature provider actively maintained |
| JSON | Native JSONB for semi-structured data if needed |

## Data Model (summary)

```
customers          (id, person_type, document, name, email, phone)
vehicles           (id, license_plate, make, model, year, customer_id)
services           (id, name, description, base_price, estimated_minutes)
parts              (id, code, name, unit_price, stock_quantity)
service_orders     (id, customer_id, vehicle_id, status, total, created_at)
os_service_items   (id, os_id, service_id, quantity, unit_price)
os_part_items      (id, os_id, part_id, quantity, unit_price)
budgets            (id, os_id, services_total, parts_total, total, status)
stock_movements    (id, part_id, type, quantity, reference, created_at)
users              (id, email, password_hash, role)
```

## Alternatives Considered

| Database | Reason Not Chosen |
|---|---|
| MySQL | PostgreSQL has better advanced type support and richer syntax |
| MongoDB | Relational domain with FK requirements; ACID is a hard requirement |
| SQLite | Not suitable for production with concurrency; useful only for tests |
