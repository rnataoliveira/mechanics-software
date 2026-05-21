# ADR-005: Migração para Classic Clean Architecture

**Status:** Accepted  
**Date:** 2026-05-04  
**Autores:** Lucas Pamponet

---

## Contexto

O projeto foi iniciado seguindo **Vertical Slice Architecture (VSA)** — documentado em ADR-002 e ADR-004. Nesse modelo, o código é organizado por *feature* (fatia vertical), e cada fatia contém o caso de uso, os tipos de request/response e as exceções co-localizados na mesma pasta.

```
Application/
  Features/
    Customers/
      CreateCustomerUseCase.cs
      CustomerResponse.cs
    ServiceOrders/
      CreateServiceOrderUseCase.cs
      ServiceOrderDto.cs
```

Com o amadurecimento do projeto e a incorporação de novos membros ao time, surgiram demandas que a VSA atual não endereça de forma explícita:

1. **Separação leitura/escrita (CQRS):** casos de uso de consulta e mutação cresceram juntos na mesma pasta sem distinção estrutural.
2. **Pipeline behaviors:** validações transversais (logging, autorização, validação de input) precisam de um ponto de entrada padronizado — ausente na estrutura atual.
3. **Padronização de time:** o template de referência adotado pelo time segue Classic Clean Architecture com CQRS explícito (Commands/Handlers/Queries). Novos membros têm mais familiaridade com esse modelo.
4. **Organização do domínio:** atualmente os arquivos de domínio são agrupados por agregado (`Domain/Customers/`, `Domain/Inventory/`). A nova estrutura organiza por tipo técnico (`Entities/`, `ValueObjects/`, `Enums/`), tornando as fronteiras de camada mais explícitas.
5. **Infrastructure:** não há distinção entre configurações de mapeamento EF Core, repositórios e SQL puro. A nova estrutura cria subpastas dedicadas.

---

## Decisão

Adotar **Classic Clean Architecture** com CQRS explícito como estrutura de referência do projeto, reorganizando as pastas de acordo com o template abaixo:

```
src/
├── MechanicsSoftware.Application/
│   ├── Abstractions/          ← interfaces de domínio (IAppDbContext, IJwtProvider, IPasswordHasher)
│   ├── Behaviors/             ← pipeline behaviors (logging, validação, autorização)
│   ├── DTOs/                  ← tipos de resposta compartilhados entre features
│   ├── Exceptions/            ← exceções da camada de aplicação
│   ├── Mapping/               ← mapeamentos de domínio → DTO
│   ├── Messaging/             ← contratos de eventos/mensagens (se aplicável)
│   ├── Persistence/           ← contrato de contexto de persistência
│   ├── UseCases/
│   │   ├── Customers/
│   │   │   ├── Commands/      ← CreateCustomerCommand.cs, UpdateCustomerCommand.cs
│   │   │   ├── Handlers/      ← CreateCustomerHandler.cs, UpdateCustomerHandler.cs
│   │   │   └── Queries/       ← GetCustomerQuery.cs, ListCustomersQuery.cs
│   │   ├── Inventory/
│   │   │   ├── Commands/
│   │   │   ├── Handlers/
│   │   │   └── Queries/
│   │   ├── ServiceOrders/
│   │   │   ├── Commands/
│   │   │   ├── Handlers/
│   │   │   └── Queries/
│   │   ├── Services/
│   │   └── Vehicles/
│   └── DependencyInjection/
│
├── MechanicsSoftware.Domain/
│   ├── Entities/              ← Customer, Part, ServiceOrder, Vehicle, Service, User
│   ├── Enums/                 ← PersonType, StockMovementType, ServiceOrderStatus, etc.
│   ├── Events/                ← eventos de domínio (futuro)
│   ├── Exceptions/            ← exceções de domínio
│   ├── Services/              ← serviços de domínio (se necessário)
│   └── ValueObjects/          ← Email, TaxId, LicensePlate, Money, BudgetStatus
│
├── MechanicsSoftware.Infrastructure/
│   ├── Configuration/         ← appsettings, opções tipadas
│   ├── DependencyInjection/   ← registro de serviços de infraestrutura
│   ├── ExternalServices/      ← integrações externas (futuro)
│   ├── Extensions/            ← métodos de extensão de infraestrutura
│   ├── Logging/               ← structured logging helpers
│   ├── Messaging/             ← implementação de mensageria (futuro)
│   └── Persistence/
│       ├── Configurations/    ← EntityTypeConfiguration<T> por entidade
│       ├── Repositories/      ← implementações de repositório (se adotados)
│       └── SQL/               ← migrations, scripts, seeding
│
└── MechanicsSoftware.API/
    ├── Controllers/
    ├── DependencyInjection/
    ├── Endpoints/
    ├── Extensions/
    ├── Filters/
    ├── Middleware/
    ├── Properties/
    ├── Swagger/
    └── Transport/             ← request/response types da API (separados dos DTOs de Application)
```

### Mapeamento de arquivos existentes → nova estrutura

| Arquivo atual | Novo local |
|---|---|
| `Application/Common/IAppDbContext.cs` | `Application/Abstractions/` ou `Application/Persistence/` |
| `Application/Common/IPasswordHasher.cs` | `Application/Abstractions/` |
| `Application/Common/Auth/IJwtProvider.cs` | `Application/Abstractions/` |
| `Application/Common/Exceptions/*.cs` | `Application/Exceptions/` |
| `Application/Features/{Feature}/*UseCase.cs` | `Application/UseCases/{Feature}/Handlers/` |
| `Application/Features/{Feature}/*Dto.cs` | `Application/DTOs/` ou `Application/UseCases/{Feature}/Queries/` |
| `Domain/{Aggregate}/*.cs` (entidades) | `Domain/Entities/` |
| `Domain/{Aggregate}/*.cs` (value objects) | `Domain/ValueObjects/` |
| `Domain/{Aggregate}/*.cs` (enums) | `Domain/Enums/` |
| `Infrastructure/Persistence/Configurations/` | `Infrastructure/Persistence/Configurations/` *(sem mudança)* |
| `Infrastructure/Persistence/Seeding/` | `Infrastructure/Persistence/SQL/` |
| `Infrastructure/Security/` | `Infrastructure/` *(manter ou mover para ExternalServices)* |
| `API/Logging/` | `Infrastructure/Logging/` |

### Convenção CQRS adotada

Cada feature dentro de `UseCases/` segue o padrão:

```
UseCases/Customers/
  Commands/
    CreateCustomerCommand.cs     ← record com os dados de entrada
    UpdateCustomerCommand.cs
  Handlers/
    CreateCustomerHandler.cs     ← implementa a lógica, injeta dependências
    UpdateCustomerHandler.cs
  Queries/
    GetCustomerQuery.cs          ← record com os parâmetros de busca
    ListCustomersQuery.cs
```

Handlers são a unidade de lógica de aplicação — equivalentes aos `*UseCase.cs` atuais. Commands e Queries são tipos de input (records imutáveis).

---

## Alternativas Consideradas

### Manter VSA (status quo)

**Vantagens:** coesão por feature já estabelecida; sem custo de migração; times pequenos navegam bem.  
**Desvantagens:** sem ponto de extensão para pipeline behaviors; CQRS implícito dificulta otimizações de leitura; diverge do template de referência do time.

### VSA com CQRS interno por feature

Manter `Features/` mas adicionar subpastas `Commands/`, `Handlers/`, `Queries/` dentro de cada feature sem reorganizar Domain e Infrastructure.  
**Desvantagens:** resolve só o problema de CQRS; não padroniza Domain nem Infrastructure; parcialmente inconsistente com o template de referência.

---

## Consequências

### Positivas

- Estrutura alinhada com o template de referência do time, reduzindo curva de aprendizado para novos membros.
- Separação explícita de Commands e Queries facilita otimizações futuras (ex.: read models separados, CQRS com dois bancos).
- `Behaviors/` cria ponto natural para validação transversal (FluentValidation + MediatR pipeline, se adotado).
- Domain organizado por tipo técnico torna as fronteiras de camada mais visíveis em code reviews.

### Negativas / Riscos

- **Custo de migração alto:** renomear namespaces, mover arquivos e atualizar todas as referências em `using` — afeta todos os projetos da solution, incluindo testes.
- **Perda de coesão por feature:** na VSA, tudo sobre `Customers` está em uma pasta. Na nova estrutura, o `CreateCustomerHandler` está em `Handlers/` e o `CustomerDto` está em `DTOs/` — navegação exige mais saltos.
- **Risco de regressão:** a migração é puramente estrutural (sem lógica alterada), mas qualquer erro de namespace pode quebrar o build ou os testes silenciosamente.
- **Commits grandes:** a refatoração estrutural produz diffs enormes que dificultam code review. Recomenda-se migrar por camada em PRs separados.

### Plano de migração sugerido

1. **PR 1 — Domain:** reorganizar `Domain/` por tipo técnico (`Entities/`, `ValueObjects/`, `Enums/`).
2. **PR 2 — Application:** criar `UseCases/` com `Commands/Handlers/Queries/`; mover interfaces para `Abstractions/`; mover DTOs para `DTOs/`.
3. **PR 3 — Infrastructure:** criar `Persistence/Configurations/`, `Persistence/SQL/`; mover `Logging/`.
4. **PR 4 — API:** criar `Transport/`; ajustar referências.
5. **PR 5 — Tests:** reorganizar testes unitários e de integração conforme nova estrutura.

Cada PR deve passar em todos os testes antes de ser mergeado.
