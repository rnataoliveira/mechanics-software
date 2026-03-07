# Visao Geral da Arquitetura

## Estilo arquitetural

**Monolito Modular em Camadas** com DDD leve.

## Estrutura de pastas

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
        value-objects/cpf-cnpj.vo.ts
        repositories/customer.repository.interface.ts
      infrastructure/
        repositories/prisma-customer.repository.ts
        mappers/customer.mapper.ts
      presentation/
        controllers/customers.controller.ts
        dto/create-customer.dto.ts

    vehicles/             # mesma estrutura
    service-orders/       # mesma estrutura + state machine
    inventory/            # mesma estrutura + movimentacoes

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

## Fluxo de uma requisicao

```
HTTP Request
     |
     v
Controller (Presentation)
  - Valida DTO (class-validator)
  - Verifica JWT Guard (se rota protegida)
     |
     v
Use Case (Application)
  - Orquestra operacao
  - Chama repositorio e servicos de dominio
     |
     v
Entity / Domain Service
  - Executa regras de negocio
  - Lanca DomainException se invariante violada
     |
     v
Repository Interface (Domain)
     |
     v
Prisma Repository (Infrastructure)
  - Persiste no PostgreSQL
     |
     v
Mapper (Infrastructure)
  - Converte entre Prisma model e Domain entity
     |
     v
Response DTO (Presentation)
```

## Endpoints (resumo)

### Publicos (sem JWT)
```
POST /auth/login
GET  /service-orders/:id/status
```

### Protegidos (JWT obrigatorio)
```
POST/GET/PUT/DELETE  /customers
POST/GET/PUT/DELETE  /vehicles
POST/GET/PUT/DELETE  /services
POST/GET/PUT/DELETE  /parts
PATCH                /parts/:id/stock
POST/GET             /service-orders
POST                 /service-orders/:id/services
POST                 /service-orders/:id/parts
POST                 /service-orders/:id/budget
POST                 /service-orders/:id/approve
POST                 /service-orders/:id/reject
POST                 /service-orders/:id/start-diagnosis
POST                 /service-orders/:id/start-execution
POST                 /service-orders/:id/finalize
POST                 /service-orders/:id/deliver
GET                  /service-orders/:id/metrics
```

## Infraestrutura (docker-compose)

```
services:
  api:    NestJS (porta 3000)
  db:     PostgreSQL 16 (porta 5432)
  swagger: disponivel em /api/docs
```

## Seguranca

- JWT com expiracao configuravel via ENV
- Senhas com bcrypt (salt rounds 12)
- Validacao de CPF/CNPJ e placa em Value Objects (nao em DTO)
- DTOs validados com class-validator
- Prisma previne SQL injection por design
- Headers de seguranca com Helmet
- Rate limiting com @nestjs/throttler
