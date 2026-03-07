# ADR-001: Escolha da Stack Tecnológica

**Status:** Aceito
**Data:** 2026-03-06

## Contexto

O Tech Challenge Fase 1 exige o desenvolvimento do back-end MVP de um sistema de oficina mecânica. O sistema deve ser entregue com Dockerfile, docker-compose, Swagger e testes automatizados com 80% de cobertura nos domínios críticos.

## Decisão

Stack escolhida:

- **Linguagem:** TypeScript
- **Framework:** NestJS
- **Banco de dados:** PostgreSQL
- **ORM:** Prisma
- **Autenticação:** JWT com @nestjs/jwt
- **Documentação:** Swagger (@nestjs/swagger)
- **Testes:** Jest + Supertest
- **Containerização:** Docker + docker-compose

## Justificativas

### NestJS
- Arquitetura modular nativa alinhada com DDD e bounded contexts
- Suporte built-in a injeção de dependência, facilitando inversão de controle
- Decorators para Swagger e validação de DTOs sem boilerplate excessivo
- Ecossistema maduro para JWT, Guards e Pipes de validação

### PostgreSQL
- Banco relacional compatível com a natureza do domínio (entidades fortemente relacionadas)
- Integridade referencial garante consistência entre OS, cliente e veículo
- Suporte a transações ACID — essencial para operações de estoque e mudança de status
- Amplamente usado em sistemas de gestão corporativa

### Prisma
- Schema declarativo e migrations versionadas
- Type-safety end-to-end entre banco e aplicação
- Menor curva de aprendizado comparado ao TypeORM para este tipo de domínio

## Alternativas consideradas

| Alternativa | Motivo da não escolha |
|---|---|
| Java Spring Boot | Maior verbosidade para MVP; tempo de setup mais longo |
| Python FastAPI | Menor ecossistema para DDD modular |
| TypeORM | Mais verboso e com mais edge cases que Prisma |
| MySQL | PostgreSQL tem melhor suporte a tipos avançados e JSON |

## Consequências

- Toda a equipe deve ter Node.js 20+ instalado
- O ambiente local roda via docker-compose (PostgreSQL + API)
- Prisma migrations devem ser versionadas junto ao código
