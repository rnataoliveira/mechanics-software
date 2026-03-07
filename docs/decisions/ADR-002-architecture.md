# ADR-002: Arquitetura Monolito Modular em Camadas

**Status:** Aceito
**Data:** 2026-03-06

## Contexto

O enunciado exige um back-end monolítico com aplicação de DDD. Como é um MVP, a complexidade operacional de microsserviços seria injustificada. Porém, uma arquitetura "monolito bagunçado" (big ball of mud) não demonstraria maturidade de design.

## Decisão

Adotar **Monolito Modular** organizado por bounded contexts, com camadas internas por módulo:

```
src/
  modules/
    customers/
      application/   # use cases, DTOs
      domain/        # entities, value objects, repository interfaces
      infrastructure/# Prisma repositories, mappers
      presentation/  # controllers, Swagger decorators
    vehicles/        # mesma estrutura
    service-orders/  # mesma estrutura
    inventory/       # mesma estrutura
  shared/
    domain/          # base entities, exceptions, value objects compartilhados
    infrastructure/  # Prisma client, JWT provider
  main.ts
```

## Bounded Contexts

| Contexto | Responsabilidade |
|---|---|
| **Customers** | Cadastro e identificação de clientes (CPF/CNPJ) |
| **Vehicles** | Cadastro de veículos e vínculo com cliente |
| **Service Orders** | Ciclo completo da OS: criação, status, orçamento, aprovação |
| **Inventory** | Peças, insumos, estoque, reserva e movimentação |

## Fluxo de dependência entre camadas

```
Presentation → Application → Domain ← Infrastructure
```

- **Domain** não depende de nada externo (framework, ORM, banco)
- **Infrastructure** implementa interfaces definidas no Domain
- **Application** orquestra use cases chamando domain + repositórios
- **Presentation** expõe HTTP e converte DTOs

## Justificativas

- Demonstra aplicação de DDD mesmo em monolito
- Facilita escalar para microsserviços no futuro (bounded contexts já isolados)
- NestJS suporta nativamente módulos com essa separação
- Alinhado com o que o desafio pede: "monolito em camadas"

## Alternativas consideradas

| Alternativa | Motivo da não escolha |
|---|---|
| Organização por tipo (controllers/, services/, repositories/) | Não demonstra DDD; difícil de manter em domínios complexos |
| Clean Architecture estrita | Overhead excessivo para o MVP no prazo dado |
| Microsserviços | Não permitido pelo enunciado |
