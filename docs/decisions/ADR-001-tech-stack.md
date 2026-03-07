# ADR-001: Tech Stack

**Status:** Accepted
**Date:** 2026-03-06

## Context

The Tech Challenge Phase 1 requires a backend MVP for a mechanic shop management system. It must be delivered with Dockerfile, docker-compose, Swagger, and automated tests with 80% coverage on critical domains.

## Decision

Chosen stack:

- **Language:** TypeScript
- **Framework:** NestJS
- **Database:** PostgreSQL
- **ORM:** Prisma
- **Authentication:** JWT via @nestjs/jwt
- **Documentation:** Swagger via @nestjs/swagger
- **Testing:** Jest + Supertest
- **Containerization:** Docker + docker-compose

## Rationale

### NestJS
- Native modular architecture aligned with DDD and bounded contexts
- Built-in dependency injection, enabling easy inversion of control
- Decorators for Swagger and DTO validation with minimal boilerplate
- Mature ecosystem for JWT Guards, Pipes, and validation

### PostgreSQL
- Relational database suited to the domain (strongly related entities)
- Referential integrity ensures consistency between OS, customer, and vehicle
- Full ACID transactions — essential for stock operations and status transitions
- Widely used in production management systems

### Prisma
- Declarative schema with versioned migrations
- End-to-end type safety between database and application
- Lower learning curve compared to TypeORM for this domain

## Alternatives Considered

| Alternative | Reason Not Chosen |
|---|---|
| Java Spring Boot | Higher verbosity for MVP; longer setup time |
| Python FastAPI | Smaller ecosystem for modular DDD patterns |
| TypeORM | More verbose and more edge cases than Prisma |
| MySQL | PostgreSQL has better support for advanced types and JSON |

## Consequences

- All team members must have Node.js 20+ installed
- Local environment runs via docker-compose (PostgreSQL + API)
- Prisma migrations must be versioned alongside the code
