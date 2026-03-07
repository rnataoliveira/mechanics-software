# Architecture Overview

## Style

**Modular Monolith with DDD** — organized by bounded context, with internal layering per module.

## Folder Structure

```
src/
  modules/
    auth/
      application/
        use-cases/
          login.use-case.ts
      domain/
        entities/user.entity.ts
        value-objects/password.vo.ts
      infrastructure/
        repositories/user.repository.ts
      presentation/
        controllers/auth.controller.ts
        dto/login.dto.ts

    customers/
      application/
        use-cases/
          create-customer.use-case.ts
          update-customer.use-case.ts
          find-customer-by-document.use-case.ts
          list-customers.use-case.ts
          delete-customer.use-case.ts
      domain/
        entities/customer.entity.ts
        value-objects/tax-id.vo.ts
        value-objects/email.vo.ts
        repositories/customer.repository.interface.ts
      infrastructure/
        repositories/prisma-customer.repository.ts
        mappers/customer.mapper.ts
      presentation/
        controllers/customers.controller.ts
        dto/create-customer.dto.ts
        dto/update-customer.dto.ts

    vehicles/             # same structure
    service-orders/       # same structure + state machine
    inventory/            # same structure + stock movements

  shared/
    domain/
      base-entity.ts
      domain-exception.ts
      value-object.ts
    infrastructure/
      database/prisma.service.ts
      security/jwt.strategy.ts
    utils/
      validators/

  main.ts
  app.module.ts

prisma/
  schema.prisma
  migrations/

test/
  unit/
  integration/
  e2e/
```

## Request Flow

```
HTTP Request
     |
     v
Controller (Presentation)
  - Validates DTO (class-validator)
  - Checks JWT Guard (if protected route)
     |
     v
Use Case (Application)
  - Orchestrates the operation
  - Calls repositories and domain services
     |
     v
Entity / Domain Service
  - Executes business rules
  - Throws DomainException if invariant is violated
     |
     v
Repository Interface (Domain)
     |
     v
Prisma Repository (Infrastructure)
  - Persists to PostgreSQL
     |
     v
Mapper (Infrastructure)
  - Converts between Prisma model and Domain entity
     |
     v
Response DTO (Presentation)
```

## Dependency Rule

```
Presentation → Application → Domain ← Infrastructure
```

- **Domain** has zero external dependencies (no framework, no ORM)
- **Infrastructure** implements interfaces defined in Domain
- **Application** orchestrates use cases using Domain + repositories
- **Presentation** exposes HTTP and converts DTOs

## API Endpoints

### Public (no JWT)
```
POST /auth/login
GET  /service-orders/:id/status
```

### Protected (JWT required)
```
POST   /customers
GET    /customers
GET    /customers/:id
PUT    /customers/:id
DELETE /customers/:id

POST   /vehicles
GET    /vehicles
GET    /vehicles/:id
PUT    /vehicles/:id
DELETE /vehicles/:id

POST   /services
GET    /services
GET    /services/:id
PUT    /services/:id
DELETE /services/:id

POST   /parts
GET    /parts
GET    /parts/:id
PUT    /parts/:id
DELETE /parts/:id
PATCH  /parts/:id/stock

POST   /service-orders
GET    /service-orders
GET    /service-orders/:id
POST   /service-orders/:id/services
POST   /service-orders/:id/parts
POST   /service-orders/:id/budget
POST   /service-orders/:id/approve
POST   /service-orders/:id/reject
POST   /service-orders/:id/start-diagnosis
POST   /service-orders/:id/start-execution
POST   /service-orders/:id/complete
POST   /service-orders/:id/deliver
GET    /service-orders/metrics/average-execution-time
```

## Infrastructure (docker-compose)

```
services:
  api:  NestJS (port 3000)
  db:   PostgreSQL 16 (port 5432)

Swagger: available at /api/docs
```

## Security

- JWT with configurable expiration via ENV
- Passwords hashed with bcrypt (salt rounds: 12)
- CPF/CNPJ and license plate validation in Value Objects (not in DTOs)
- DTOs validated with class-validator
- Prisma prevents SQL injection by design
- Security headers via Helmet
- Rate limiting via @nestjs/throttler
