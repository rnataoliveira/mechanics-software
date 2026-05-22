# Backlog — Fase 2

**Projeto:** Mechanics Software — FIAP POS Tech 15SOAT  
**Período:** 2026-05-01 → 2026-06-30  
**Time:** Allan (RM373714) · Daniel (RM370852) · Diogo (RM371224) · Lucas (RM371615) · Joelma Renata (RM371593)

---

## Visão geral

| Milestone | Foco | PBIs | Pode iniciar |
|---|---|---|---|
| M0 — Migração de Arquitetura | Refatoração estrutural conforme ADR-005 | F2-00a a F2-00e | Imediatamente |
| M1 — Correções de API | ListOS + endpoint de aprovação + notificação por e-mail | F2-01 a F2-07 | Após M0 completo |
| M2 — Kubernetes | Manifestos YAML em `/k8s` | F2-08 a F2-13 | Imediatamente (paralelo ao M0) |
| M3 — Terraform | Infraestrutura como Código em `/infra` | F2-14 a F2-17 | Imediatamente (paralelo ao M0) |
| M4 — CI/CD | Extensão do pipeline com Docker push + deploy K8s | F2-18 a F2-20 | Após M2 validado |
| M5 — Documentação e Entrega | README, vídeo, submissão | F2-21 a F2-23 | Após M1 e M2 concluídos |

**Legenda de esforço:** P = Pequeno (< 2h) · M = Médio (2–4h) · G = Grande (> 4h)

---

## M0 — Migração de Arquitetura (ADR-005)

> Refatoração puramente estrutural — nenhuma lógica de negócio é alterada.
> Os PRs devem ser executados em sequência. Cada PR deve passar em todos os testes antes de ser mergeado.
> Referência: [ADR-005](../decisions/ADR-005-clean-architecture-migration.md)

---

### F2-00a — Migrar `Domain/` para organização por tipo técnico

**Responsável sugerido:** Lucas  
**Esforço:** M  
**Depende de:** —

**Descrição**  
Reorganizar a camada de domínio de uma estrutura por agregado para uma estrutura por tipo técnico, conforme definido no ADR-005.

Estrutura atual:
```
Domain/
  Customers/
  Inventory/
  ServiceOrders/
  Vehicles/
  ...
```

Estrutura alvo:
```
Domain/
  Entities/       ← Customer, Part, ServiceOrder, Vehicle, Service, User
  Enums/          ← PersonType, StockMovementType, ServiceOrderStatus, BudgetStatus, etc.
  Events/         ← (pasta criada, vazia por ora — reservada para domain events)
  Exceptions/     ← exceções de domínio existentes
  Services/       ← serviços de domínio existentes (se houver)
  ValueObjects/   ← Email, TaxId, LicensePlate, Money, BudgetStatus
```

**Critérios de aceite**
- [ ] Todos os arquivos movidos para a pasta correta conforme a tabela do ADR-005
- [ ] Namespaces atualizados em todos os arquivos da camada Domain
- [ ] Referências ao Domain nos projetos Application, Infrastructure, API e testes compilam sem erros
- [ ] Todos os testes passam sem modificação de lógica
- [ ] PR contém apenas movimentação de arquivos e atualização de `using` — zero mudança de lógica

---

### F2-00b — Migrar `Application/` para Clean Architecture com CQRS explícito

**Responsável sugerido:** Joelma  
**Esforço:** G  
**Depende de:** F2-00a

**Descrição**  
Reorganizar a camada de aplicação de Vertical Slice Architecture (Features) para a estrutura com separação explícita de Commands, Handlers e Queries por feature.

Estrutura atual:
```
Application/
  Common/
    IAppDbContext.cs
    IPasswordHasher.cs
    Auth/IJwtProvider.cs
    Exceptions/
  Features/
    Customers/
      CreateCustomerUseCase.cs
      CustomerResponse.cs
    ServiceOrders/
      ...
```

Estrutura alvo:
```
Application/
  Abstractions/          ← IAppDbContext, IJwtProvider, IPasswordHasher
  Behaviors/             ← (vazio por ora — reservado para pipeline behaviors)
  DTOs/                  ← tipos de resposta compartilhados
  Exceptions/            ← exceções da camada de aplicação
  Mapping/               ← mapeamentos domínio → DTO (se aplicável)
  Persistence/           ← contrato do contexto de persistência (ou manter em Abstractions)
  UseCases/
    Customers/
      Commands/          ← CreateCustomerCommand.cs, UpdateCustomerCommand.cs
      Handlers/          ← CreateCustomerHandler.cs (ex-UseCase)
      Queries/           ← GetCustomerQuery.cs, ListCustomersQuery.cs
    Inventory/
    ServiceOrders/
    Services/
    Vehicles/
  DependencyInjection/
```

Convenção de nomenclatura:
- `*UseCase.cs` → `*Handler.cs` (em `Handlers/`)
- Inputs de mutação → `*Command.cs` (em `Commands/`)
- Inputs de consulta → `*Query.cs` (em `Queries/`)
- DTOs de resposta → `*Response.cs` ou `*Dto.cs` (em `DTOs/` ou dentro de `Queries/`)

**Critérios de aceite**
- [ ] Todos os `*UseCase.cs` renomeados para `*Handler.cs` e movidos para `Handlers/`
- [ ] Tipos de input extraídos como `*Command.cs` ou `*Query.cs`
- [ ] Interfaces `IAppDbContext`, `IJwtProvider`, `IPasswordHasher` movidas para `Abstractions/`
- [ ] Exceções movidas de `Common/Exceptions/` para `Exceptions/`
- [ ] Projeto API e Infrastructure compilam sem erros após atualização de referências
- [ ] Todos os testes passam sem modificação de lógica
- [ ] PR contém apenas movimentação e renomeação — zero mudança de lógica

---

### F2-00c — Migrar `Infrastructure/` para nova estrutura de pastas

**Responsável sugerido:** Daniel  
**Esforço:** M  
**Depende de:** F2-00b

**Descrição**  
Reorganizar a camada de infraestrutura criando subpastas dedicadas conforme ADR-005.

Mudanças principais:
- `Infrastructure/Persistence/Seeding/` → `Infrastructure/Persistence/SQL/`
- `API/Logging/` → `Infrastructure/Logging/`
- Criar pastas vazias reservadas: `ExternalServices/`, `Messaging/`
- `Infrastructure/Security/` — manter ou mover para `ExternalServices/` (decisão do time)

**Critérios de aceite**
- [ ] Arquivos de seeding movidos para `Persistence/SQL/`
- [ ] Logging movido de API para Infrastructure
- [ ] Namespaces atualizados em todos os arquivos afetados
- [ ] Projeto API compila sem erros após atualização de referências
- [ ] Todos os testes passam

---

### F2-00d — Migrar `API/` — criar `Transport/` e atualizar referências

**Responsável sugerido:** Diogo  
**Esforço:** M  
**Depende de:** F2-00b, F2-00c

**Descrição**  
Criar a pasta `API/Transport/` para separar os tipos de request/response da API dos DTOs da camada Application, conforme ADR-005.

Mudanças principais:
- Tipos de request recebidos nos controllers (ex.: `CreateCustomerRequest`) → `API/Transport/`
- Controllers atualizam referências para `Transport/`
- Remover qualquer tipo de request que ainda viva em `Application/`

**Critérios de aceite**
- [ ] Pasta `API/Transport/` criada com todos os tipos de request/response da API
- [ ] Controllers usam tipos de `Transport/`, não de `Application/DTOs/`
- [ ] Projeto compila sem erros
- [ ] Swagger continua gerando documentação correta
- [ ] Todos os testes passam

---

### F2-00e — Atualizar projetos de testes para a nova estrutura

**Responsável sugerido:** Allan  
**Esforço:** M  
**Depende de:** F2-00a, F2-00b, F2-00c, F2-00d

**Descrição**  
Atualizar todos os `using` e namespaces nos projetos de testes unitários e de integração para refletir a nova estrutura de pastas estabelecida em F2-00a a F2-00d.

**Critérios de aceite**
- [ ] Projeto de testes unitários compila sem erros
- [ ] Projeto de testes de integração compila sem erros
- [ ] Todos os testes unitários passam (sem alteração de lógica)
- [ ] Todos os testes de integração passam (sem alteração de lógica)
- [ ] SonarCloud não reporta novos code smells decorrentes da migração

---

## M1 — Correções de API

> Inicia após M0 completo. F2-01 e F2-02 são independentes entre si e podem ser desenvolvidos em paralelo.

---

### F2-01 — Fix: listagem de OS com ordenação por prioridade e exclusão de finalizadas

**Responsável sugerido:** Joelma  
**Esforço:** P  
**Depende de:** F2-00e

**Descrição**  
O endpoint `GET /api/service-orders` atualmente retorna todas as OS sem ordenação definida. O requisito da Fase 2 exige ordenação por prioridade de status e exclusão de OS finalizadas ou entregues.

Arquivo a modificar: `Application/UseCases/ServiceOrders/Handlers/ListServiceOrdersHandler.cs` (novo caminho pós-migração).

Ordenação exigida (menor número = maior prioridade):

| Status | Peso |
|---|---|
| `InExecution` | 1 |
| `AwaitingApproval` | 2 |
| `InDiagnosis` | 3 |
| `Received` | 4 |

Dentro do mesmo status: mais antigas primeiro (`CreatedAt ASC`).  
OS com status `Completed` ou `Delivered` **não devem aparecer** na listagem.

**Critérios de aceite**
- [ ] `GET /api/service-orders` retorna apenas OS com status ativo (não Completed, não Delivered)
- [ ] OS são ordenadas por prioridade de status conforme a tabela acima
- [ ] OS com mesmo status são ordenadas por `CreatedAt ASC`
- [ ] Testes de integração existentes atualizados para validar a nova ordenação
- [ ] Testes de integração existentes atualizados para validar a exclusão de finalizadas

---

### F2-02 — Refatorar: endpoint único de decisão de orçamento

**Responsável sugerido:** Joelma  
**Esforço:** P  
**Depende de:** F2-00e

**Descrição**  
Atualmente existem dois endpoints separados (`/approve` e `/reject`) para aprovação e rejeição de orçamento. O requisito da Fase 2 pede um único ponto de entrada externo para receber a notificação do cliente.

Criar:
```
POST /api/service-orders/{id}/budget-decision
Body: { "decision": "approve" | "reject" }
```

- Rota `[AllowAnonymous]` — representa notificação vinda de sistema externo (ex.: portal do cliente)
- Internamente chama os handlers `ApproveServiceOrderHandler` ou `RejectServiceOrderHandler` conforme o valor de `decision`
- Os handlers existentes **não mudam** — apenas o endpoint/controller muda
- Validar que `decision` aceita apenas `"approve"` ou `"reject"` (retornar `400` para valor inválido)

**Critérios de aceite**
- [ ] `POST /budget-decision` com `"approve"` aprova o orçamento e retorna `200`
- [ ] `POST /budget-decision` com `"reject"` rejeita o orçamento e retorna `200`
- [ ] `POST /budget-decision` com valor inválido retorna `400`
- [ ] Endpoints antigos (`/approve`, `/reject`) removidos ou marcados como `[Obsolete]`
- [ ] Swagger documenta o novo endpoint corretamente
- [ ] Teste de integração cobre os três cenários acima

---

### F2-03 — Criar: interface `IEmailNotifier` na camada Application

**Responsável sugerido:** Lucas  
**Esforço:** P  
**Depende de:** F2-00e

**Descrição**  
Definir o contrato de notificação por e-mail na camada Application, seguindo o mesmo padrão de inversão de dependência já usado no projeto (`IAppDbContext`, `IJwtProvider`, `IPasswordHasher`).

Arquivo novo: `Application/Abstractions/IEmailNotifier.cs`

```csharp
public interface IEmailNotifier
{
    Task SendStatusChangedAsync(
        string toEmail,
        string customerName,
        Guid serviceOrderId,
        string newStatus,
        CancellationToken cancellationToken = default);
}
```

**Critérios de aceite**
- [ ] Interface criada em `Application/Abstractions/`
- [ ] Nenhuma implementação nesta camada — apenas o contrato
- [ ] Projeto compila sem erros

---

### F2-04 — Criar: implementação `SmtpEmailNotifier` na Infrastructure

**Responsável sugerido:** Lucas  
**Esforço:** M  
**Depende de:** F2-03

**Descrição**  
Implementar `IEmailNotifier` usando `System.Net.Mail.SmtpClient` (sem dependência de pacote externo).

Arquivo novo: `Infrastructure/Notifications/SmtpEmailNotifier.cs`

Configuração via variáveis de ambiente:

| Variável | Descrição |
|---|---|
| `SMTP_HOST` | Servidor SMTP (ex.: `smtp.gmail.com`) |
| `SMTP_PORT` | Porta (ex.: `587`) |
| `SMTP_USER` | Usuário / endereço de origem |
| `SMTP_PASS` | Senha ou app password |
| `SMTP_FROM` | Endereço de exibição do remetente |

- Registrar `SmtpEmailNotifier` como implementação de `IEmailNotifier` no `DependencyInjection.cs` da Infrastructure
- Template de e-mail mínimo: assunto `"Atualização da sua Ordem de Serviço #{id}"`, corpo com status novo e nome do cliente

**Critérios de aceite**
- [ ] `SmtpEmailNotifier` implementa `IEmailNotifier`
- [ ] Configuração lida de `IConfiguration` (variáveis de ambiente)
- [ ] Registrado no container de DI da Infrastructure
- [ ] Compila sem erros
- [ ] Variáveis documentadas no `.env.example` (ou equivalente)

---

### F2-05 — Injetar `IEmailNotifier` nos handlers de mudança de status

**Responsável sugerido:** Joelma  
**Esforço:** M  
**Depende de:** F2-03, F2-04

**Descrição**  
Chamar `SendStatusChangedAsync` ao fim de cada handler que muda o status de uma OS, após a persistência ser bem-sucedida.

Handlers a modificar:

| Handler | Status enviado no e-mail |
|---|---|
| `StartDiagnosisHandler` | `IN_DIAGNOSIS` |
| `SendBudgetHandler` | `AWAITING_APPROVAL` |
| `ApproveServiceOrderHandler` | `IN_EXECUTION` |
| `RejectServiceOrderHandler` | `CANCELLED` |
| `StartExecutionHandler` | `IN_EXECUTION` |
| `CompleteServiceOrderHandler` | `COMPLETED` |
| `DeliverServiceOrderHandler` | `DELIVERED` |

O e-mail do cliente vem de `serviceOrder.Customer.Email`.

**Importante:** falha no envio de e-mail **não deve impedir** a transição de status. Envolver a chamada em `try/catch` e logar o erro sem relançar a exceção.

**Critérios de aceite**
- [ ] Todos os 7 handlers injetam `IEmailNotifier` e chamam `SendStatusChangedAsync`
- [ ] Falha no envio de e-mail é logada mas não propaga exceção para o caller
- [ ] A transição de status é persistida no banco independentemente do resultado do e-mail
- [ ] Projeto compila sem erros

---

### F2-06 — Testes unitários para notificação por e-mail

**Responsável sugerido:** Joelma  
**Esforço:** P  
**Depende de:** F2-03, F2-05

**Descrição**  
Adicionar testes unitários para validar o comportamento de notificação por e-mail nos handlers modificados em F2-05.

**Cenários obrigatórios:**
- `IEmailNotifier` é chamado com os parâmetros corretos (e-mail, nome, id, status) após mudança de status bem-sucedida
- Falha em `SendStatusChangedAsync` (exceção lançada) não impede a persistência da transição de status
- Handler não chama `IEmailNotifier` se a transição de status falhar

**Critérios de aceite**
- [ ] Testes unitários cobrindo os 3 cenários acima para pelo menos 3 dos 7 handlers modificados
- [ ] `IEmailNotifier` mockado com Moq — sem chamada real a servidor SMTP
- [ ] Todos os testes passam
- [ ] Cobertura geral do projeto mantida ≥ 80%

---

### F2-07 — Testes de integração para correções de API

**Responsável sugerido:** Joelma  
**Esforço:** P  
**Depende de:** F2-01, F2-02

**Descrição**  
Adicionar ou atualizar testes de integração (Testcontainers + PostgreSQL real) para os dois endpoints corrigidos.

**Cenários obrigatórios:**

*Listagem de OS:*
- Retorna OS ordenadas por prioridade de status (`InExecution` antes de `AwaitingApproval`, etc.)
- OS com status `Completed` ou `Delivered` não aparecem na lista

*Budget decision:*
- `POST /budget-decision` com `"approve"` → OS vai para `InExecution`
- `POST /budget-decision` com `"reject"` → OS vai para `Cancelled`
- `POST /budget-decision` com valor inválido → `400 Bad Request`

**Critérios de aceite**
- [ ] Todos os cenários cobertos com testes de integração rodando contra PostgreSQL real (Testcontainers)
- [ ] Todos os testes passam
- [ ] Pipeline de CI passa com os novos testes

---

## M2 — Kubernetes (`/k8s`)

> Pode ser desenvolvido em paralelo com M0 e M1 — não depende do código C#.
> Validar localmente com `kind` ou `minikube` antes de abrir PR.

---

### F2-08 — Namespace e ConfigMap

**Responsável sugerido:** Allan  
**Esforço:** P  
**Depende de:** —

**Descrição**  
Criar os manifests base do cluster.

Arquivos novos:
- `k8s/namespace.yaml` — namespace `mechanics-software`
- `k8s/configmap.yaml` — variáveis não-sensíveis

Variáveis no ConfigMap:

| Chave | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` |

**Critérios de aceite**
- [ ] `kubectl apply -f k8s/namespace.yaml` cria o namespace sem erros
- [ ] `kubectl apply -f k8s/configmap.yaml` cria o ConfigMap sem erros
- [ ] `kubectl get configmap -n mechanics-software` exibe o ConfigMap com os valores corretos

---

### F2-09 — Secrets

**Responsável sugerido:** Allan  
**Esforço:** P  
**Depende de:** F2-08

**Descrição**  
Criar manifest de Secrets com valores placeholder (base64 de strings de exemplo).

Arquivo novo: `k8s/secret.yaml`

Secrets:

| Chave | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL |
| `JWT_SECRET` | Chave de assinatura JWT |
| `SMTP_HOST` | Servidor SMTP |
| `SMTP_PORT` | Porta SMTP |
| `SMTP_USER` | Usuário SMTP |
| `SMTP_PASS` | Senha SMTP |

O arquivo commitado deve conter valores placeholder (`<base64-encoded-placeholder>`). Valores reais ficam nos GitHub Secrets e são aplicados pelo CI/CD.

**Critérios de aceite**
- [ ] `k8s/secret.yaml` commitado com placeholders — sem secrets reais no repositório
- [ ] `kubectl apply -f k8s/secret.yaml` aplica sem erros (com valores placeholder)
- [ ] README ou comentário no arquivo documenta como substituir os placeholders

---

### F2-10 — Deployment e Service da API

**Responsável sugerido:** Allan  
**Esforço:** M  
**Depende de:** F2-08, F2-09

**Descrição**  
Criar os manifestos de Deployment e Service para a API.

Arquivos novos: `k8s/deployment-api.yaml`, `k8s/service-api.yaml`

Especificações do Deployment:
- Imagem: `ghcr.io/rnataoliveira/mechanics-software:latest`
- `replicas: 2` (mínimo para HPA)
- Resources: `requests: cpu 250m, memory 256Mi` / `limits: cpu 500m, memory 512Mi`
- `readinessProbe` e `livenessProbe` em `GET /health` (path: `/health`, port: `8080`)
- `envFrom` referenciando o ConfigMap e o Secret criados em F2-08 e F2-09

Especificações do Service:
- `type: LoadBalancer`
- Porta exposta: `8080`

**Critérios de aceite**
- [ ] `kubectl apply -f k8s/deployment-api.yaml` cria o Deployment sem erros
- [ ] `kubectl apply -f k8s/service-api.yaml` cria o Service sem erros
- [ ] `kubectl get pods -n mechanics-software` mostra 2 pods `Running`
- [ ] `kubectl port-forward svc/mechanics-software-api 8080:8080 -n mechanics-software` expõe a API localmente
- [ ] `GET /health` retorna `200`

---

### F2-11 — Deployment, Service e PVC do banco de dados

**Responsável sugerido:** Allan  
**Esforço:** M  
**Depende de:** F2-08, F2-09

**Descrição**  
Criar os manifestos para rodar PostgreSQL 16 dentro do cluster.

Arquivos novos: `k8s/deployment-db.yaml`, `k8s/service-db.yaml`, `k8s/pvc.yaml`

Especificações:
- Imagem: `postgres:16-alpine`
- PVC de `1Gi` montado em `/var/lib/postgresql/data`
- Service `ClusterIP` — banco acessível apenas internamente ao cluster
- Credenciais lidas do Secret criado em F2-09

**Critérios de aceite**
- [ ] Pod do PostgreSQL em estado `Running`
- [ ] PVC em estado `Bound`
- [ ] API consegue conectar ao banco via connection string que usa o service name do K8s
- [ ] Dados persistem após `kubectl rollout restart deployment/mechanics-software-db`

---

### F2-12 — Horizontal Pod Autoscaler (HPA)

**Responsável sugerido:** Allan  
**Esforço:** P  
**Depende de:** F2-10

**Descrição**  
Configurar o HPA para escalar a API automaticamente conforme uso de CPU.

Arquivo novo: `k8s/hpa.yaml`

```yaml
minReplicas: 2
maxReplicas: 10
targetCPUUtilizationPercentage: 70
```

**Critérios de aceite**
- [ ] `kubectl get hpa -n mechanics-software` exibe o HPA com `MINPODS: 2`, `MAXPODS: 10`, `TARGET: 70%`
- [ ] HPA referencia o Deployment correto (`mechanics-software-api`)

---

### F2-13 — Validação local do cluster completo

**Responsável sugerido:** Allan  
**Esforço:** M  
**Depende de:** F2-08 a F2-12

**Descrição**  
Subir o cluster localmente com `kind` ou `minikube` e validar que todos os componentes funcionam de ponta a ponta.

**Critérios de aceite**
- [ ] `kubectl apply -f k8s/` aplicado sem erros
- [ ] Todos os pods em `Running`
- [ ] API responde via `kubectl port-forward`
- [ ] `GET /api/service-orders` retorna `200` (ou `401` para rota autenticada)
- [ ] HPA listado com `kubectl get hpa -n mechanics-software`
- [ ] Screenshot ou log do cluster saudável commitado em `docs/` (para uso no vídeo)

---

## M3 — Terraform (`/infra`)

> Pode ser desenvolvido em paralelo com M0 e M2.

---

### F2-14 — Estrutura base do Terraform

**Responsável sugerido:** Daniel  
**Esforço:** P  
**Depende de:** —

**Descrição**  
Criar o esqueleto da configuração Terraform com providers fixados para garantir reproducibilidade.

Arquivos novos:
```
infra/
  providers.tf    ← versões fixadas (kubernetes ~> 2.30, helm ~> 2.13, null ~> 3.2)
  variables.tf    ← cluster_name, db_name, db_user, db_password, jwt_secret, smtp_*
  main.tf         ← chama os módulos kubernetes e database
  outputs.tf      ← kubeconfig_path, db_host
  modules/
    kubernetes/
    database/
```

**Critérios de aceite**
- [ ] `terraform init` em `infra/` completa sem erros
- [ ] `terraform validate` passa sem erros
- [ ] Variáveis sensíveis (`db_password`, `jwt_secret`, `smtp_pass`) marcadas com `sensitive = true`

---

### F2-15 — Módulo Terraform: cluster Kubernetes (Kind local)

**Responsável sugerido:** Daniel  
**Esforço:** M  
**Depende de:** F2-14

**Descrição**  
Provisionar um cluster Kind local via Terraform usando `null_resource` com `local-exec`.

Arquivos novos em `infra/modules/kubernetes/`:
- `main.tf` — `null_resource` que chama `kind create cluster`
- `variables.tf` — `cluster_name`
- `outputs.tf` — `kubeconfig_path`
- `kind-config.yaml` — 1 control-plane + 2 workers

**Critérios de aceite**
- [ ] `terraform apply` cria o cluster Kind
- [ ] `terraform destroy` deleta o cluster Kind
- [ ] `output "kubeconfig_path"` aponta para o kubeconfig gerado
- [ ] Cluster acessível com `kubectl --kubeconfig=$(terraform output -raw kubeconfig_path) get nodes`

---

### F2-16 — Módulo Terraform: banco de dados PostgreSQL

**Responsável sugerido:** Daniel  
**Esforço:** M  
**Depende de:** F2-14, F2-15

**Descrição**  
Provisionar namespace, PVC e Secret do banco de dados via provider Kubernetes do Terraform.

Arquivos novos em `infra/modules/database/`:
- `main.tf` — namespace `mechanics-software`, PVC `postgres-pvc` (1Gi), Secret `mechanics-secrets`
- `variables.tf` — `db_password`, `jwt_secret`, `namespace`
- `outputs.tf` — `db_host`, `namespace`

**Critérios de aceite**
- [ ] `terraform apply` cria namespace, PVC e Secret no cluster
- [ ] `kubectl get pvc -n mechanics-software` mostra PVC `Bound`
- [ ] `kubectl get secret mechanics-secrets -n mechanics-software` existe

---

### F2-17 — Documentação do Terraform (`infra/README.md`)

**Responsável sugerido:** Daniel  
**Esforço:** P  
**Depende de:** F2-15, F2-16

**Descrição**  
Documentar o uso do Terraform para que qualquer membro do time ou o avaliador consiga provisionar o ambiente do zero.

Conteúdo obrigatório:
- Pré-requisitos (`terraform >= 1.7`, `kind`, `kubectl`, `docker`)
- Recursos criados por módulo (tabela)
- Passo a passo completo: `init → plan → apply`
- Como obter o kubeconfig após apply
- Como destruir o ambiente: `terraform destroy`

**Critérios de aceite**
- [ ] Arquivo `infra/README.md` criado
- [ ] Passo a passo testado e funcionando em uma máquina limpa (ou equivalente)
- [ ] Comandos de exemplo incluem os valores de variáveis obrigatórias

---

## M4 — CI/CD (extensão do pipeline)

> Requer F2-10 (nome da imagem Docker correto) e F2-13 (cluster K8s validado localmente).
> Criar workflow separado `deploy.yml` — não modificar o `coverage.yml` existente.
> `deploy.yml` só é acionado após `coverage.yml` passar com sucesso.

---

### F2-18 — Workflow: build e push da imagem Docker no GHCR

**Responsável sugerido:** Diogo  
**Esforço:** M  
**Depende de:** F2-10

**Descrição**  
Criar `.github/workflows/deploy.yml` com o job `build-and-push` que constrói e publica a imagem Docker no GitHub Container Registry (GHCR).

Trigger: `workflow_run` após conclusão bem-sucedida do `Coverage Report` na branch `main`.

Tags geradas:
- `sha-<commit-sha>` — para deploy imutável
- `latest` — apenas em push na `main`

GitHub Secrets necessários: nenhum além do `GITHUB_TOKEN` automático.

**Critérios de aceite**
- [ ] Workflow criado e ativo no repositório
- [ ] Push na `main` (com testes passando) dispara o job
- [ ] Imagem publicada em `ghcr.io/rnataoliveira/mechanics-software`
- [ ] Tags `sha-<sha>` e `latest` geradas corretamente
- [ ] Job aparece como verde no GitHub Actions

---

### F2-19 — Workflow: deploy dos manifestos K8s

**Responsável sugerido:** Diogo  
**Esforço:** M  
**Depende de:** F2-13, F2-18

**Descrição**  
Adicionar job `deploy` no `deploy.yml` que aplica os manifestos K8s após o build bem-sucedido da imagem.

O job substitui a tag `latest` no `deployment-api.yaml` pela tag imutável `sha-<sha>` antes de aplicar.

GitHub Secrets necessários:

| Secret | Como gerar |
|---|---|
| `KUBECONFIG` | `base64 ~/.kube/config` |

**Critérios de aceite**
- [ ] Job `deploy` roda após job `build-and-push`
- [ ] `kubectl apply -f k8s/` aplicado sem erros
- [ ] `kubectl rollout status` aguarda e confirma o rollout bem-sucedido (timeout 120s)
- [ ] Job aparece como verde no GitHub Actions

---

### F2-20 — Workflow: migration do banco de dados

**Responsável sugerido:** Diogo  
**Esforço:** M  
**Depende de:** F2-18, F2-19

**Descrição**  
Adicionar job `migrate` no `deploy.yml` que aplica as migrations do EF Core no cluster antes do deploy da API.

O job usa `kubectl run` com a imagem recém-publicada para executar as migrations de forma efêmera.

GitHub Secrets adicionais:

| Secret | Descrição |
|---|---|
| `DB_CONNECTION_STRING` | Connection string completa do PostgreSQL no cluster |

O job `deploy` deve depender de `migrate` (`needs: [build-and-push, migrate]`).

> Alternativa: usar Init Container no `deployment-api.yaml` — mais simples, executa antes de cada start do pod. Definir com o time qual abordagem adotar.

**Critérios de aceite**
- [ ] Migrations aplicadas antes do deploy da API
- [ ] Job `migrate` aparece como verde no GitHub Actions
- [ ] Redeployment sem migrations pendentes completa sem erros

---

## M5 — Documentação e Entrega

> Inicia quando M1 e M2 estiverem concluídos e M4 rodando com sucesso.

---

### F2-21 — Atualizar `README.md` com seção Fase 2

**Responsável sugerido:** Lucas  
**Esforço:** M  
**Depende de:** M1 completo, M2 validado, M4 rodando

**Descrição**  
Adicionar seção `## Fase 2` ao `README.md` da raiz com todo o conteúdo exigido pelo rubric.

Conteúdo obrigatório:
- Objetivos da Fase 2
- Diagrama de arquitetura (ASCII ou imagem em `docs/architecture/fase2-diagram.png`)
- Componentes: API (N réplicas via HPA), PostgreSQL (PVC), GitHub Actions CI/CD, Kubernetes, Terraform
- Fluxo de deploy: `push main → coverage.yml → deploy.yml (build → migrate → kubectl apply)`
- Instruções de execução local (`docker compose up`)
- Instruções de deploy em Kubernetes (`kubectl apply -f k8s/`)
- Instruções de provisionamento Terraform (`terraform init && apply`)
- Link para Swagger / Postman collection
- Link para vídeo demonstrativo

**Critérios de aceite**
- [ ] Seção Fase 2 adicionada ao README sem quebrar conteúdo existente
- [ ] Diagrama de arquitetura presente (ASCII inline ou imagem linkada)
- [ ] Todos os comandos testados e funcionando
- [ ] Links para vídeo e Swagger preenchidos

---

### F2-22 — Vídeo demonstrativo (até 15 minutos)

**Responsável sugerido:** Time  
**Esforço:** G  
**Depende de:** F2-21, M4 funcionando

**Descrição**  
Gravar e publicar vídeo demonstrando a solução completa da Fase 2.

Roteiro obrigatório:

| Segmento | Conteúdo | Tempo sugerido |
|---|---|---|
| 1 — Arquitetura | Diagrama do README, explicar componentes e fluxo de deploy | ~2 min |
| 2 — CI/CD | GitHub Actions: `coverage.yml` passando → `deploy.yml` disparando → build, migrate, deploy | ~3 min |
| 3 — APIs | Swagger/Postman: criar OS, mudar status, listar com ordenação, aprovar via `budget-decision` | ~4 min |
| 4 — Escalabilidade | `kubectl get hpa -w`, simular carga com `hey` ou `k6`, pods escalando ao vivo | ~3 min |
| 5 — Encerramento | Cluster saudável, resumo do que foi entregue | ~1 min |

Publicar no YouTube ou Vimeo (público ou não listado).

**Critérios de aceite**
- [ ] Vídeo publicado com link acessível
- [ ] Duração ≤ 15 minutos
- [ ] Todos os 5 segmentos presentes
- [ ] Link inserido no README (F2-21)

---

### F2-23 — Submissão no portal do aluno

**Responsável sugerido:** Joelma  
**Esforço:** P  
**Depende de:** F2-21, F2-22

**Descrição**  
Realizar a submissão formal da Fase 2 no portal FIAP e garantir que o repositório está acessível para o avaliador.

**Checklist:**
- [ ] Repositório compartilhado com o usuário `soat-architecture` no GitHub (Settings → Collaborators)
- [ ] Branch `main` com todos os PRs da Fase 2 mergeados
- [ ] Workflows `coverage.yml` e `deploy.yml` verdes na `main`
- [ ] Pasta `/k8s` com todos os manifestos
- [ ] Pasta `/infra` com Terraform + `infra/README.md`
- [ ] `README.md` raiz com seção Fase 2 completa e links preenchidos
- [ ] PDF submetido no portal com: link do repositório, diagrama de arquitetura, link do vídeo

**Critérios de aceite**
- [ ] Submissão confirmada no portal FIAP antes de 2026-06-30
- [ ] Todos os itens do checklist acima marcados

---

## Dependências entre PBIs

```
F2-00a
  └─ F2-00b
       └─ F2-00c
            └─ F2-00d
                 └─ F2-00e
                      ├─ F2-01 ──┐
                      ├─ F2-02 ──┼─ F2-07
                      └─ F2-03
                           └─ F2-04
                                └─ F2-05 ─ F2-06

F2-08 ─ F2-09
           ├─ F2-10 ─ F2-12
           └─ F2-11
F2-08 a F2-12 ─ F2-13 ─ F2-19 ─ F2-20
                                      └─ F2-19 (deploy depende de migrate)

F2-14 ─ F2-15 ─ F2-16 ─ F2-17

F2-10 ─ F2-18 ─┬─ F2-19 ─ F2-20
               └─ F2-20

M1 + M2 ─ F2-21 ─ F2-22 ─ F2-23
```

## Distribuição sugerida por membro

| Membro | PBIs | Foco |
|---|---|---|
| **Joelma** | F2-00b, F2-01, F2-02, F2-05, F2-06, F2-07, F2-23 | Application layer + correções de API |
| **Lucas** | F2-00a, F2-03, F2-04, F2-21 | Domain + e-mail + documentação |
| **Allan** | F2-00e, F2-08, F2-09, F2-10, F2-11, F2-12, F2-13 | Testes + Kubernetes |
| **Daniel** | F2-00c, F2-14, F2-15, F2-16, F2-17 | Infrastructure + Terraform |
| **Diogo** | F2-00d, F2-18, F2-19, F2-20 | API layer + CI/CD |
