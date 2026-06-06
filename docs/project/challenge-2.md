# Fase 2 — Análise de Requisitos e Progresso

## Contexto

A Fase 2 do Tech Challenge (14SOAT) pede a evolução da aplicação desenvolvida na Fase 1,
incorporando qualidade de código, infraestrutura escalável, orquestração com Kubernetes,
Infraestrutura como Código (Terraform) e pipeline de CI/CD completa.

Repositório: https://github.com/rnataoliveira/mechanics-software

---

## O que já temos (Fase 1 + melhorias já implementadas)

### Aplicação

| Item | Status | Observação |
|---|---|---|
| Domínio rico (DDD) | ✅ Feito | Agregados, entidades, value objects, exceções de domínio |
| Camada de aplicação (Use Cases) | ✅ Feito | Vertical Slice Architecture sem MediatR |
| Camada de infraestrutura | ✅ Feito | EF Core 8 + Npgsql + PostgreSQL 16 |
| API REST (ASP.NET Core 8) | ✅ Feito | JWT, Swagger, middleware de exceções |
| Autenticação JWT | ✅ Feito | Login, BCrypt para senhas |
| CRUD de Clientes, Veículos, Peças, Serviços | ✅ Feito | Todos os endpoints implementados |
| Abertura de OS | ✅ Feito | `POST /api/service-orders` retorna ID único |
| Consulta de status da OS | ✅ Feito | `GET /api/service-orders/{id}/status` |
| Fluxo completo da OS | ✅ Feito | Recebida → Diagnóstico → Aguardando Aprovação → Execução → Finalizada → Entregue |
| Aprovação/rejeição de orçamento | ⚠️ Parcial | Dois endpoints separados (`/approve` e `/reject`) — Fase 2 pede um único endpoint externo |
| Listagem de OS | ⚠️ Parcial | Existe, mas sem ordenação por status e sem exclusão de Finalizada/Entregue |
| Testes unitários | ✅ Feito | xUnit + Moq + FluentAssertions, cobertura ≥ 80% |
| Testes de integração | ✅ Feito | Testcontainers + PostgreSQL real |
| Análise estática (SonarCloud) | ✅ Feito | Integrado ao CI |

### Infraestrutura já existente

| Item | Status | Observação |
|---|---|---|
| Dockerfile | ✅ Feito | Multi-stage build (SDK → runtime ASP.NET 8) |
| docker-compose (local) | ✅ Feito | API + PostgreSQL 16, healthcheck configurado |
| GitHub Actions — build + testes | ✅ Feito | Roda em todo PR e push na main |
| GitHub Actions — SonarCloud | ✅ Feito | Análise de qualidade e cobertura |
| GitHub Actions — GitHub Pages | ✅ Feito | Relatório de cobertura publicado |

---

## O que falta para a Fase 2

### 1. Correções de API

#### 1.1 Listagem de Ordens de Serviço — ordenação e filtro

**Requisito:** ordenar por prioridade de status e excluir logicamente OS finalizadas/entregues.

- Ordenação exigida: `Em Execução > Aguardando Aprovação > Diagnóstico > Recebida`
- Dentro do mesmo status: mais antigas primeiro
- OS com status `Finalizada` ou `Entregue` **não devem aparecer** na listagem

**O que mudar:** `ListServiceOrdersUseCase.cs` — adicionar ordenação por status com peso e filtro de exclusão.

---

#### 1.2 Aprovação de orçamento — endpoint único externo

**Requisito:** endpoint único para receber notificação externa de aprovação ou recusa.

**Proposta:**
```
POST /api/service-orders/{id}/budget-decision
Body: { "decision": "approve" | "reject" }
```

Substitui os dois endpoints atuais (`/approve` e `/reject`) por um único ponto de entrada externo.

---

#### 1.3 Atualização de status via e-mail (NOVO)

**Requisito:** "Atualização de status da OS via alguma ferramenta como e-mail."

**Interpretação adotada (outbound):** enviar e-mail ao cliente toda vez que o status da OS mudar.

- Quando uma OS muda de status, a aplicação envia e-mail notificando o cliente
- Ferramenta: **SendGrid** (via HTTP API, sem dependência de servidor SMTP) ou SMTP simples
- Gatilho: ao fim de cada use case que muda status da OS

**O que criar:**
- Interface `IEmailNotifier` na camada Application
- Implementação `SendGridEmailNotifier` (ou `SmtpEmailNotifier`) na Infrastructure
- Chamada ao notificador nos use cases de mudança de status
- Configuração da chave de API via variável de ambiente / Secret do K8s

---

### 2. Kubernetes — `/k8s` (tudo novo)

Criar a pasta `/k8s` com os seguintes manifestos YAML:

| Arquivo | Conteúdo |
|---|---|
| `namespace.yaml` | Namespace `mechanics-software` |
| `configmap.yaml` | Variáveis não sensíveis (environment, URLs) |
| `secret.yaml` | JWT secret, connection string, chave SendGrid |
| `deployment-api.yaml` | Deployment da API (2 réplicas mínimas) |
| `deployment-db.yaml` | Deployment do PostgreSQL (ou usar managed DB via Terraform) |
| `service-api.yaml` | Service tipo LoadBalancer ou ClusterIP + Ingress |
| `service-db.yaml` | Service ClusterIP para o banco |
| `hpa.yaml` | HorizontalPodAutoscaler — escala de 2 a 10 pods por CPU ≥ 70% |
| `pvc.yaml` | PersistentVolumeClaim para dados do PostgreSQL |

---

### 3. Terraform — `/infra` (tudo novo)

Criar a pasta `/infra` com scripts para provisionar:

| Recurso | Ferramenta |
|---|---|
| Cluster Kubernetes (local com Kind ou cloud com EKS/GKE) | Terraform |
| Banco de dados PostgreSQL (RDS ou container gerenciado) | Terraform |
| Variáveis sensíveis (Secrets) | Terraform + K8s provider |

Estrutura sugerida:
```
/infra
  main.tf
  variables.tf
  outputs.tf
  providers.tf
  modules/
    kubernetes/
    database/
```

---

### 4. CI/CD — extensão do pipeline existente

O pipeline atual já faz: build → testes → SonarCloud → GitHub Pages.

**Adicionar ao `.github/workflows/`:**

| Etapa nova | Descrição |
|---|---|
| Build da imagem Docker | `docker build` + push para registry (Docker Hub ou GHCR) |
| Deploy no cluster K8s | `kubectl apply -f k8s/` após push na main |
| Deploy do banco | Aplicar migration via `dotnet ef database update` no cluster |
| Aplicar manifestos YAML | `kubectl apply -k k8s/` ou Kustomize |

Sugestão: criar um segundo workflow `deploy.yml` separado do `coverage.yml`, ativado apenas em push na `main` após os testes passarem.

---

### 5. README.md — atualização

Adicionar à raiz:

- [ ] Descrição da solução Fase 2 (objetivos, o que mudou)
- [ ] Diagrama de arquitetura (componentes, infraestrutura, fluxo de deploy)
- [ ] Instruções de execução local (`docker-compose up`)
- [ ] Instruções de deploy em Kubernetes (`kubectl apply`)
- [ ] Instruções de provisionamento Terraform (`terraform init && apply`)
- [ ] Link para collection Postman/Swagger
- [ ] Link para vídeo demonstrativo (YouTube/Vimeo, até 15 min)

---

## Resumo de pendências

| Prioridade | Tarefa | Esforço estimado |
|---|---|---|
| 🔴 Alta | Fix `ListServiceOrders` — ordenação + exclusão de finalizadas | Pequeno |
| 🔴 Alta | Refatorar endpoint de aprovação de orçamento (único) | Pequeno |
| 🔴 Alta | Implementar notificação por e-mail ao mudar status da OS | Médio |
| 🔴 Alta | Criar manifestos Kubernetes (`/k8s`) | Médio |
| 🟡 Média | Criar scripts Terraform (`/infra`) | Médio |
| 🟡 Média | Estender pipeline CI/CD (Docker push + K8s deploy) | Médio |
| 🟢 Baixa | Atualizar README.md com conteúdo da Fase 2 | Pequeno |
| 🟢 Baixa | Gravar vídeo demonstrativo (até 15 min) | — |

---

## Decisões abertas

1. **Email outbound vs. inbound?**
   Adotamos outbound (enviar e-mail quando status muda). Se o grupo preferir inbound
   (receber e-mail para acionar mudança de status), o esforço é consideravelmente maior
   (requer mailbox + webhook ou polling de caixa de entrada).

2. **Kubernetes local (Kind/Minikube) ou cloud (EKS/GKE/AKS)?**
   Para o vídeo basta local. O Terraform pode provisionar um cluster Kind localmente ou
   apontar para uma cloud gratuita (GKE Autopilot free tier, por exemplo).

3. **Registry de imagem Docker?**
   GitHub Container Registry (GHCR) é gratuito para repositórios públicos/privados do
   GitHub e já está integrado ao GitHub Actions.
