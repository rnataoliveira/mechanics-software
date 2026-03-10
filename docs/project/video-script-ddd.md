# Video Script — DDD & Architecture Explanation
> Two versions: PT-BR and EN

---

## VERSÃO PT-BR

---

### [ABERTURA]

Olá! Neste vídeo vamos explicar como aplicamos Domain-Driven Design — o DDD — no nosso projeto de software para oficina mecânica, desenvolvido como Tech Challenge da Pós-Tech FIAP.

Vamos mostrar nossa separação de bounded contexts, os agregados e entidades, o fluxo do domínio e como nossa arquitetura traduz tudo isso em código.

---

### [PARTE 1 — O PROBLEMA DE NEGÓCIO]

O sistema resolve um problema real: **gerenciar o ciclo completo de atendimento de uma oficina mecânica.**

Quando um cliente chega à oficina com seu veículo, uma série de etapas precisa acontecer:

1. O atendente identifica o cliente e o veículo
2. Abre uma Ordem de Serviço
3. O mecânico faz o diagnóstico e registra os serviços e peças necessários
4. Um orçamento é gerado e enviado ao cliente
5. O cliente aprova ou rejeita
6. Se aprovado, os serviços são executados
7. O veículo é entregue

Todo esse fluxo tem regras de negócio complexas — e é exatamente para modelar isso que usamos DDD.

---

### [PARTE 2 — BOUNDED CONTEXTS]

O DDD nos ensina a dividir um sistema complexo em **Contextos Delimitados** — regiões com linguagem e responsabilidades próprias.

Identificamos **6 bounded contexts** no nosso sistema:

**Customers (Clientes)**
Gerencia o cadastro de clientes, sejam pessoas físicas com CPF ou jurídicas com CNPJ. Toda validação de documento fica aqui.

**Vehicles (Veículos)**
Gerencia os veículos cadastrados. Um veículo sempre pertence a um cliente. Validamos o formato da placa — tanto o padrão Mercosul quanto o legado.

**Services Catalogue (Catálogo de Serviços)**
Mantém o catálogo de serviços que a oficina oferece, com nome, descrição, preço-base e tempo estimado. Quando um serviço é adicionado a uma OS, o preço é "fotografado" — mudanças futuras no catálogo não afetam OSs existentes.

**Service Orders (Ordens de Serviço)**
O coração do sistema. Gerencia todo o ciclo de vida da OS: criação, composição de itens, orçamento e aprovação. Tem uma máquina de estados bem definida.

**Inventory (Estoque)**
Controla o estoque de peças. Cada movimentação é registrada. O sistema suporta reservas temporárias de estoque enquanto uma OS aguarda aprovação.

**Auth (Autenticação)**
Emite e valida tokens JWT. Protege todas as rotas administrativas. A única rota pública além do login é a consulta de status da OS — que o cliente pode acessar sem login.

---

### [PARTE 3 — AGGREGATES E ENTIDADES]

Dentro de cada bounded context, usamos os padrões do DDD: **Aggregate Roots, Entities e Value Objects**.

**ServiceOrder** é nosso aggregate root mais complexo. Ela encapsula:
- `ServiceItem` — um serviço adicionado à OS
- `PartItem` — uma peça adicionada à OS
- `Budget` — o orçamento calculado, filho da OS, sem ciclo de vida independente

As regras de negócio ficam dentro do próprio aggregate:
- Não é possível adicionar itens a uma OS fora do status RECEIVED ou IN_DIAGNOSIS
- Não é possível iniciar a execução sem orçamento aprovado
- As transições de status seguem uma máquina de estados rígida

**Customer** é outro aggregate root — com os value objects `TaxId` (CPF/CNPJ com validação de dígito verificador) e `Email`.

**Vehicle** carrega o value object `LicensePlate`, com validação dos dois formatos aceitos no Brasil.

**Part** (no contexto de Inventory) controla o estoque. Cada alteração de quantidade gera um `StockMovement`. O estoque nunca pode ficar negativo.

**Value Objects importantes:**
- `Money` — armazena valores monetários em centavos (inteiros) para evitar erros de ponto flutuante
- `ServiceOrderStatus` — encapsula a máquina de estados com as transições válidas
- `BudgetStatus` — PENDING → APPROVED | REJECTED, transições terminais

---

### [PARTE 4 — EVENT STORMING]

Usamos Event Storming para descobrir o domínio antes de escrever qualquer linha de código.

O fluxo principal da OS ficou assim:

```
Cliente chega → Atendente identifica cliente e veículo
→ Abre OS (status: RECEIVED)
→ Mecânico inicia diagnóstico (IN_DIAGNOSIS)
→ Registra serviços e peças
→ Sistema gera orçamento
→ Atendente envia orçamento (AWAITING_APPROVAL)
→ Cliente aprova → IN_EXECUTION → COMPLETED → DELIVERED
→ Cliente rejeita → CANCELLED (reservas de estoque liberadas)
```

Os atores identificados foram: **Cliente, Atendente, Mecânico, Administrador** e **Sistema** (para automações internas).

---

### [PARTE 5 — ARQUITETURA]

Para implementar o DDD, escolhemos **Vertical Slice Architecture** combinada com uma camada de domínio pura.

A solução tem 4 projetos principais:

**Domain** — zero dependências externas. Só C# puro. Aqui vivem as entities, value objects e as regras de negócio.

**Application** — os casos de uso, organizados por feature (slice vertical). Cada ação do sistema — como `CreateServiceOrder` ou `ApproveBudget` — é uma classe simples com um método `HandleAsync`. Nenhum framework, sem MediatR.

**Infrastructure** — implementa a interface `IAppDbContext` com EF Core e PostgreSQL. As migrations ficam aqui. Segurança: JWT e BCrypt.

**API** — controllers finos. Só preocupações HTTP. Chama os use cases diretamente.

A regra de dependência:
```
API → Application → Domain
Infrastructure → Domain  (inversão de dependência)
```

O Domain nunca conhece EF Core, ASP.NET ou qualquer framework. Isso o mantém testável e puro.

---

### [PARTE 6 — UBIQUITOUS LANGUAGE]

Uma das práticas mais importantes do DDD é a **linguagem ubíqua** — usar os mesmos termos no código, na documentação e nas conversas com o negócio.

No nosso projeto, isso significa:
- Classes chamadas `ServiceOrder`, `Budget`, `PartItem` — não `OsModel`, `ValorTotal` ou `ItemPeca`
- Tabelas no banco: `service_orders`, `budgets`, `part_items`
- Endpoints REST: `/service-orders`, `/parts`, `/budgets`
- Exceções de domínio com nomes claros: `InvalidStatusTransitionException`, `DomainException`

---

### [FECHAMENTO]

Em resumo, aplicamos DDD para:

✓ Identificar e delimitar 6 bounded contexts com responsabilidades claras
✓ Modelar aggregates com invariantes de negócio encapsuladas
✓ Criar value objects que protegem o domínio de dados inválidos
✓ Documentar o fluxo com Event Storming antes de codificar
✓ Manter o domínio puro, sem dependências de framework
✓ Usar linguagem ubíqua consistente em código, banco e APIs

O resultado é um sistema onde as regras de negócio da oficina vivem no lugar certo — no domínio — e não espalhadas pela infraestrutura ou controllers.

Obrigado!

---
---

## ENGLISH VERSION

---

### [OPENING]

Hello! In this video we'll explain how we applied Domain-Driven Design — DDD — in our auto repair shop management software, developed as a Tech Challenge for FIAP's Post-Graduate program.

We'll walk through our bounded context separation, aggregates and entities, the domain flow, and how our architecture translates all of this into code.

---

### [PART 1 — THE BUSINESS PROBLEM]

The system solves a real problem: **managing the complete service lifecycle of an auto repair shop.**

When a customer arrives at the shop with their vehicle, a series of steps must happen:

1. The attendant identifies the customer and vehicle
2. Opens a Service Order
3. The mechanic runs a diagnosis and registers the required services and parts
4. A budget is generated and sent to the customer
5. The customer approves or rejects it
6. If approved, services are executed
7. The vehicle is delivered back

This entire flow has complex business rules — and that's exactly why we modeled it with DDD.

---

### [PART 2 — BOUNDED CONTEXTS]

DDD teaches us to divide a complex system into **Bounded Contexts** — regions with their own language and responsibilities.

We identified **6 bounded contexts** in our system:

**Customers**
Manages customer registration — both individuals (CPF) and companies (CNPJ). All document validation lives here.

**Vehicles**
Manages registered vehicles. A vehicle always belongs to a customer. We validate the license plate format — both Mercosul and legacy Brazilian standards.

**Services Catalogue**
Maintains the catalogue of services offered by the shop, including name, description, base price, and estimated duration. When a service is added to a Service Order, the price is snapshotted — future catalogue changes do not affect existing orders.

**Service Orders**
The heart of the system. Manages the complete OS lifecycle: creation, item composition, budgeting, and approval. It follows a well-defined state machine.

**Inventory**
Controls parts stock. Every stock change is recorded as a movement. The system supports temporary reservations while a Service Order awaits approval.

**Auth**
Issues and validates JWT tokens. Protects all admin routes. The only public routes are login and the Service Order status query endpoint — which customers can access without authentication.

---

### [PART 3 — AGGREGATES AND ENTITIES]

Inside each bounded context, we use DDD patterns: **Aggregate Roots, Entities, and Value Objects**.

**ServiceOrder** is our most complex aggregate root. It encapsulates:
- `ServiceItem` — a service added to the order
- `PartItem` — a part added to the order
- `Budget` — the calculated budget, a child entity with no independent lifecycle

Business rules live inside the aggregate itself:
- Items cannot be added to an order outside of RECEIVED or IN_DIAGNOSIS status
- Execution cannot start without an approved budget
- Status transitions follow a strict state machine

**Customer** is another aggregate root — with value objects `TaxId` (CPF/CNPJ with check digit validation) and `Email`.

**Vehicle** carries the `LicensePlate` value object, validating both accepted Brazilian plate formats.

**Part** (in the Inventory context) controls stock. Every quantity change generates a `StockMovement`. Stock can never go negative.

**Key Value Objects:**
- `Money` — stores monetary values in cents (integers) to avoid floating point errors
- `ServiceOrderStatus` — encapsulates the state machine with valid transitions
- `BudgetStatus` — PENDING → APPROVED | REJECTED, with terminal states

---

### [PART 4 — EVENT STORMING]

We used Event Storming to discover the domain before writing a single line of code.

The main Service Order flow:

```
Customer arrives → Attendant identifies customer and vehicle
→ Opens Service Order (status: RECEIVED)
→ Mechanic starts diagnosis (IN_DIAGNOSIS)
→ Registers services and parts
→ System generates budget
→ Attendant sends budget (AWAITING_APPROVAL)
→ Customer approves → IN_EXECUTION → COMPLETED → DELIVERED
→ Customer rejects → CANCELLED (stock reservations released)
```

The identified actors were: **Customer, Attendant, Mechanic, Administrator**, and **System** (for internal automations).

---

### [PART 5 — ARCHITECTURE]

To implement DDD, we chose **Vertical Slice Architecture** combined with a pure domain layer.

The solution has 4 main projects:

**Domain** — zero external dependencies. Pure C# only. This is where entities, value objects, and business rules live.

**Application** — use cases organized by feature (vertical slice). Each system action — like `CreateServiceOrder` or `ApproveBudget` — is a plain class with a `HandleAsync` method. No framework, no MediatR.

**Infrastructure** — implements the `IAppDbContext` interface using EF Core and PostgreSQL. Migrations live here. Security: JWT and BCrypt.

**API** — thin controllers. HTTP concerns only. Calls use cases directly.

The dependency rule:
```
API → Application → Domain
Infrastructure → Domain  (dependency inversion)
```

The Domain never knows about EF Core, ASP.NET, or any framework. This keeps it testable and pure.

---

### [PART 6 — UBIQUITOUS LANGUAGE]

One of the most important DDD practices is **ubiquitous language** — using the same terms in code, documentation, and business conversations.

In our project, this means:
- Classes named `ServiceOrder`, `Budget`, `PartItem` — not `OsModel`, `TotalValue`, or `PartRecord`
- Database tables: `service_orders`, `budgets`, `part_items`
- REST endpoints: `/service-orders`, `/parts`, `/budgets`
- Domain exceptions with clear names: `InvalidStatusTransitionException`, `DomainException`

---

### [CLOSING]

In summary, we applied DDD to:

✓ Identify and delimit 6 bounded contexts with clear responsibilities
✓ Model aggregates with encapsulated business invariants
✓ Create value objects that protect the domain from invalid data
✓ Document the flow with Event Storming before coding
✓ Keep the domain pure, with no framework dependencies
✓ Use consistent ubiquitous language across code, database, and APIs

The result is a system where the repair shop's business rules live in the right place — in the domain — not scattered across infrastructure or controllers.

Thank you!
