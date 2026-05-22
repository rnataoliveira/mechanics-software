# Roadmap Fase 2 — Plano de Implementação

Issues ordenadas por dependência de execução. Cada tarefa deve ser iniciada somente após suas predecessoras estarem mergeadas.

**Repositório:** https://github.com/rnataoliveira/mechanics-software
**Análise de gaps:** [docs/project/fase2-analise.md](fase2-analise.md)

---

## Visão geral das milestones

| Milestone | Foco | Status |
|---|---|---|
| M1 — Correções de API | ListOS + aprovação + notificação por e-mail | A fazer |
| M2 — Kubernetes | Manifestos YAML em `/k8s` | A fazer |
| M3 — Terraform | Infraestrutura como Código em `/infra` | A fazer |
| M4 — CI/CD | Extensão do pipeline com Docker push + deploy K8s | A fazer |
| M5 — Documentação e Entrega | README, vídeo, submissão | A fazer |

---

## M1 — Correções de API

> Ponto de partida obrigatório. Corrige comportamentos que o rubric da Fase 2 avalia diretamente.
> As tarefas F2-03 a F2-07 dependem de F2-03, mas F2-01 e F2-02 são independentes entre si.

### F2-01 — Fix: listagem de OS com ordenação por status e exclusão lógica

**Arquivo:** `src/MechanicsSoftware.Application/Features/ServiceOrders/ListServiceOrdersUseCase.cs`

O que mudar:
- Excluir da listagem OS com status `Completed` e `Delivered`
- Ordenar por prioridade de status: `InExecution > AwaitingApproval > InDiagnosis > Received`
- Dentro do mesmo status: mais antigas primeiro (`CreatedAt ASC`)

```
Ordenação com peso:
InExecution      → peso 1
AwaitingApproval → peso 2
InDiagnosis      → peso 3
Received         → peso 4
```

**Testes:** atualizar testes de integração existentes de listagem.

---

### F2-02 — Refatorar: endpoint único de aprovação de orçamento

**Requisito:** um único endpoint externo para aprovar ou recusar o orçamento.

Criar `POST /api/service-orders/{id}/budget-decision`:
```json
{ "decision": "approve" | "reject" }
```

- Rota `[AllowAnonymous]` (recebe notificação externa do cliente)
- Internamente chama `ApproveServiceOrderUseCase` ou `RejectServiceOrderUseCase` conforme `decision`
- Os use cases existentes **não mudam** — apenas o controller/endpoint muda

**Testes:** adicionar teste de integração para o novo endpoint.

---

### F2-03 — Criar: interface `IEmailNotifier` na Application layer

**Arquivo novo:** `src/MechanicsSoftware.Application/Common/IEmailNotifier.cs`

```csharp
public interface IEmailNotifier
{
    Task SendStatusChangedAsync(
        string toEmail, string customerName,
        Guid serviceOrderId, string newStatus,
        CancellationToken cancellationToken = default);
}
```

Sem implementação aqui — apenas o contrato. Segue a inversão de dependência já usada no projeto (`IAppDbContext`, `IJwtProvider`, `IPasswordHasher`).

---

### F2-04 — Criar: implementação `SmtpEmailNotifier` na Infrastructure

**Arquivo novo:** `src/MechanicsSoftware.Infrastructure/Notifications/SmtpEmailNotifier.cs`

- Usa `System.Net.Mail.SmtpClient` (sem dependência externa) ou SendGrid SDK
- Configuração via variáveis de ambiente: `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `SMTP_FROM`
- Registrar no `DependencyInjection.cs` da Infrastructure

> Decisão de grupo: SMTP simples é suficiente para o entregável. SendGrid é opcional se quiserem um relay gratuito.

---

### F2-05 — Injetar `IEmailNotifier` nos use cases de mudança de status

Use cases que devem chamar `SendStatusChangedAsync` ao final de execução bem-sucedida:

| Use case | Novo status enviado |
|---|---|
| `StartDiagnosisUseCase` | IN_DIAGNOSIS |
| `SendBudgetUseCase` | AWAITING_APPROVAL |
| `ApproveServiceOrderUseCase` | IN_EXECUTION (após aprovação inicia execução) |
| `RejectServiceOrderUseCase` | CANCELLED |
| `StartExecutionUseCase` | IN_EXECUTION |
| `CompleteServiceOrderUseCase` | COMPLETED |
| `DeliverServiceOrderUseCase` | DELIVERED |

O e-mail do cliente vem de `serviceOrder.Customer.Email` (já existe na entidade).

**Depende de:** F2-03, F2-04

---

### F2-06 — Testes unitários para e-mail e novo endpoint

- Mockar `IEmailNotifier` nos testes unitários dos use cases alterados (F2-05)
- Verificar que `SendStatusChangedAsync` é chamado com os parâmetros corretos
- Testar cenário de falha de e-mail não bloqueia a mudança de status (try/catch no use case ou fire-and-forget)

**Depende de:** F2-03, F2-05

---

### F2-07 — Testes de integração para as correções de API

- Listagem retorna OS na ordem correta por prioridade de status
- Listagem não retorna OS com status Completed/Delivered
- `POST /budget-decision` com `"approve"` aprova o orçamento
- `POST /budget-decision` com `"reject"` cancela a OS

**Depende de:** F2-01, F2-02

---

## M2 — Kubernetes (`/k8s`)

> Pode ser desenvolvido em paralelo com M1 por outro membro do time.
> Validar localmente com `minikube` ou `kind` antes de commitar.

### F2-08 — Namespace e ConfigMap

**Arquivos novos:** `k8s/namespace.yaml`, `k8s/configmap.yaml`

ConfigMap contém variáveis não-sensíveis:
- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`

---

### F2-09 — Secrets

**Arquivo novo:** `k8s/secret.yaml`

Secrets (base64):
- `ConnectionStrings__DefaultConnection`
- `JWT_SECRET`
- `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`

> O arquivo commitado no repo deve conter valores placeholder (`<base64-encoded-value>`).
> Valores reais ficam nos GitHub Secrets e são aplicados pelo CI/CD.

---

### F2-10 — Deployment e Service da API

**Arquivos novos:** `k8s/deployment-api.yaml`, `k8s/service-api.yaml`

Deployment:
- Image: `ghcr.io/rnataoliveira/mechanics-software:latest`
- Réplicas: 2 (mínimo para HPA)
- Resources: `requests: cpu 250m, memory 256Mi` / `limits: cpu 500m, memory 512Mi`
- readinessProbe + livenessProbe em `GET /health`
- envFrom: configMapRef + secretRef

Service: `type: LoadBalancer` na porta 8080

---

### F2-11 — Deployment, Service e PVC do banco de dados

**Arquivos novos:** `k8s/deployment-db.yaml`, `k8s/service-db.yaml`, `k8s/pvc.yaml`

- PostgreSQL 16-alpine
- PVC de 1Gi para `/var/lib/postgresql/data`
- Service: `type: ClusterIP` (interno ao cluster)

> Alternativa: usar banco fora do cluster (RDS via Terraform) e apontar a connection string via Secret.

---

### F2-12 — Horizontal Pod Autoscaler (HPA)

**Arquivo novo:** `k8s/hpa.yaml`

```
minReplicas: 2
maxReplicas: 10
targetCPUUtilizationPercentage: 70
```

**Depende de:** F2-10 (o HPA referencia o Deployment da API)

---

### F2-13 — Validação local do cluster

- Subir cluster local com `kind` ou `minikube`
- `kubectl apply -f k8s/` sem erros
- API responde via `kubectl port-forward`
- HPA listado com `kubectl get hpa`

**Depende de:** F2-08 a F2-12

---

## M3 — Terraform (`/infra`)

> Pode rodar em paralelo com M2. Foco em provisionar o ambiente que o K8s vai usar.
> Adotamos a abordagem **local com Kind** para evitar custo de cloud durante o desenvolvimento.
> O mesmo código Terraform pode apontar para uma cloud real mudando somente o provider.

### F2-14 — Estrutura base Terraform

**Arquivos novos:**
```
infra/
  providers.tf
  variables.tf
  main.tf
  outputs.tf
  modules/
    kubernetes/
      main.tf
      variables.tf
      outputs.tf
    database/
      main.tf
      variables.tf
      outputs.tf
  README.md
```

**`infra/providers.tf`** — versões fixadas para reproducibilidade:
```hcl
terraform {
  required_version = ">= 1.7"

  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.30"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.13"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }
}
```

**`infra/variables.tf`** — variáveis globais:
```hcl
variable "cluster_name"    { default = "mechanics-software" }
variable "db_name"         { default = "mechanics_software" }
variable "db_user"         { default = "postgres" }
variable "db_password"     { sensitive = true }
variable "jwt_secret"      { sensitive = true }
variable "smtp_host"       { default = "" }
variable "smtp_user"       { default = "" }
variable "smtp_pass"       { sensitive = true; default = "" }
```

**`infra/main.tf`** — orquestra os módulos:
```hcl
module "kubernetes" {
  source       = "./modules/kubernetes"
  cluster_name = var.cluster_name
}

module "database" {
  source      = "./modules/database"
  db_name     = var.db_name
  db_user     = var.db_user
  db_password = var.db_password
  depends_on  = [module.kubernetes]
}
```

**`infra/outputs.tf`:**
```hcl
output "kubeconfig_path" { value = module.kubernetes.kubeconfig_path }
output "db_host"         { value = module.database.db_host }
```

---

### F2-15 — Módulo: cluster Kubernetes (Kind local)

**Arquivos:** `infra/modules/kubernetes/main.tf`, `variables.tf`, `outputs.tf`

**`modules/kubernetes/main.tf`:**
```hcl
resource "null_resource" "kind_cluster" {
  provisioner "local-exec" {
    command = <<-EOT
      kind create cluster \
        --name ${var.cluster_name} \
        --config ${path.module}/kind-config.yaml \
        --kubeconfig ${path.module}/kubeconfig.yaml \
        || echo "Cluster already exists"
    EOT
  }

  provisioner "local-exec" {
    when    = destroy
    command = "kind delete cluster --name ${var.cluster_name}"
  }
}
```

**`modules/kubernetes/kind-config.yaml`** (arquivo estático no módulo):
```yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

**`modules/kubernetes/outputs.tf`:**
```hcl
output "kubeconfig_path" {
  value = "${path.module}/kubeconfig.yaml"
}
```

> Para usar em cloud: substituir `null_resource` pelo provider GKE/EKS/AKS.
> O restante do projeto não muda — apenas este módulo.

**Depende de:** F2-14

---

### F2-16 — Módulo: banco de dados PostgreSQL

**Arquivos:** `infra/modules/database/main.tf`, `variables.tf`, `outputs.tf`

Na abordagem local, o banco roda dentro do cluster K8s (já declarado nos manifestos F2-11).
O módulo Terraform aplica os manifestos do banco via provider `kubernetes`:

**`modules/database/main.tf`:**
```hcl
resource "kubernetes_namespace" "mechanics" {
  metadata { name = "mechanics-software" }
}

resource "kubernetes_persistent_volume_claim" "db" {
  metadata {
    name      = "postgres-pvc"
    namespace = kubernetes_namespace.mechanics.metadata[0].name
  }
  spec {
    access_modes = ["ReadWriteOnce"]
    resources { requests = { storage = "1Gi" } }
  }
}

resource "kubernetes_secret" "app" {
  metadata {
    name      = "mechanics-secrets"
    namespace = kubernetes_namespace.mechanics.metadata[0].name
  }
  data = {
    "db-password" = var.db_password
    "jwt-secret"  = var.jwt_secret
  }
}
```

**`modules/database/outputs.tf`:**
```hcl
output "db_host" { value = "postgres-service.mechanics-software.svc.cluster.local" }
output "namespace" { value = "mechanics-software" }
```

**Depende de:** F2-14, F2-15 (namespace criado pelo módulo de K8s)

---

### F2-17 — Documentação do Terraform

**Arquivo:** `infra/README.md`

Conteúdo obrigatório:
- Lista de recursos criados por módulo (tabela: recurso, tipo, descrição)
- Pré-requisitos: `terraform >= 1.7`, `kind`, `kubectl`, `docker`
- Passo a passo:
  ```bash
  cd infra
  terraform init
  terraform plan -var="db_password=postgres" -var="jwt_secret=dev-secret"
  terraform apply -auto-approve -var="db_password=postgres" -var="jwt_secret=dev-secret"
  ```
- Como destruir: `terraform destroy`
- Como obter o kubeconfig: `export KUBECONFIG=$(terraform output -raw kubeconfig_path)`

**Depende de:** F2-15, F2-16

---

## M4 — CI/CD (extensão do pipeline)

> Requer F2-10 (imagem Docker com nome correto do registry) e F2-08–F2-12 (manifestos K8s) validados.
> Criar um workflow separado `deploy.yml` — não misturar com o `coverage.yml` existente.
> O `deploy.yml` só roda após o `coverage.yml` passar (usando `workflow_run`).

### F2-18 — Workflow: build e push da imagem Docker

**Arquivo novo:** `.github/workflows/deploy.yml`

```yaml
name: Deploy

on:
  workflow_run:
    workflows: ["Coverage Report"]
    types: [completed]
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    name: Build & Push Docker image
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    permissions:
      contents: read
      packages: write

    steps:
      - uses: actions/checkout@v4

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=sha,prefix=sha-
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
```

**GitHub Secrets necessários para este job:** nenhum além do `GITHUB_TOKEN` automático.

---

### F2-19 — Workflow: deploy dos manifestos K8s

Adicionar job `deploy` no mesmo `deploy.yml`, rodando após `build-and-push`:

```yaml
  deploy:
    name: Deploy to Kubernetes
    runs-on: ubuntu-latest
    needs: build-and-push
    permissions:
      contents: read

    steps:
      - uses: actions/checkout@v4

      - name: Configure kubectl
        run: |
          mkdir -p ~/.kube
          echo "${{ secrets.KUBECONFIG }}" | base64 -d > ~/.kube/config

      - name: Set image tag in deployment
        run: |
          IMAGE="${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}"
          sed -i "s|ghcr.io/rnataoliveira/mechanics-software:latest|$IMAGE|g" \
            k8s/deployment-api.yaml

      - name: Apply K8s manifests
        run: kubectl apply -f k8s/

      - name: Wait for rollout
        run: |
          kubectl rollout status deployment/mechanics-software-api \
            -n mechanics-software --timeout=120s
```

**GitHub Secrets necessários:**
| Secret | Descrição |
|---|---|
| `KUBECONFIG` | kubeconfig do cluster em base64 (`base64 ~/.kube/config`) |

**Depende de:** F2-18, F2-13 (cluster validado localmente)

---

### F2-20 — Workflow: migration do banco no cluster

Adicionar job `migrate` no `deploy.yml`, **antes** do job `deploy`:

```yaml
  migrate:
    name: Apply database migrations
    runs-on: ubuntu-latest
    needs: build-and-push
    permissions:
      contents: read

    steps:
      - uses: actions/checkout@v4

      - name: Configure kubectl
        run: |
          mkdir -p ~/.kube
          echo "${{ secrets.KUBECONFIG }}" | base64 -d > ~/.kube/config

      - name: Run EF Core migrations
        run: |
          IMAGE="${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}"
          kubectl run ef-migrate \
            --image=$IMAGE \
            --restart=Never \
            --rm \
            --attach \
            -n mechanics-software \
            --env="ConnectionStrings__DefaultConnection=${{ secrets.DB_CONNECTION_STRING }}" \
            -- dotnet MechanicsSoftware.API.dll --migrate-only || true
```

> Alternativa mais simples: usar Init Container no `deployment-api.yaml` com o mesmo comando.
> O Init Container roda antes do container principal a cada deploy, garantindo que as migrations
> estejam aplicadas antes da API subir.

**GitHub Secrets adicionais:**
| Secret | Descrição |
|---|---|
| `DB_CONNECTION_STRING` | Connection string completa do PostgreSQL no cluster |

**Atualizar job `deploy` para depender de `migrate`:**
```yaml
  deploy:
    needs: [build-and-push, migrate]
```

**Depende de:** F2-18, F2-19

---

## M5 — Documentação e Entrega

> Começa quando M1 e M2 estiverem concluídos e M4 rodando com sucesso.

### F2-21 — README.md atualizado

Adicionar seção `## Fase 2` ao `README.md` existente na raiz. Estrutura obrigatória:

```markdown
## Fase 2 — Escalabilidade e Infraestrutura

### Objetivos
<parágrafo descrevendo os objetivos da Fase 2>

### Arquitetura
<diagrama ASCII ou imagem linkada>

Componentes:
- API (ASP.NET Core 8) — N réplicas via HPA
- PostgreSQL 16 — PVC persistente no cluster
- GitHub Actions — CI/CD com build, testes, push e deploy
- Kubernetes (Kind local / cloud) — orquestração
- Terraform — provisionamento do cluster e banco

Fluxo de deploy:
push na main → coverage.yml (build + testes) → deploy.yml (Docker push → migrate → kubectl apply)

### Execução local
\`\`\`bash
docker compose up
# API disponível em http://localhost:8080/swagger
\`\`\`

### Deploy em Kubernetes
\`\`\`bash
# Pré-requisito: cluster rodando e kubeconfig configurado
kubectl apply -f k8s/
kubectl rollout status deployment/mechanics-software-api -n mechanics-software
kubectl port-forward svc/mechanics-software-api 8080:8080 -n mechanics-software
\`\`\`

### Provisionamento com Terraform
\`\`\`bash
cd infra
terraform init
terraform apply -var="db_password=SEU_PASSWORD" -var="jwt_secret=SEU_SECRET"
export KUBECONFIG=$(terraform output -raw kubeconfig_path)
\`\`\`

### APIs
- Swagger UI: http://localhost:8080/swagger
- [Collection Postman](<link>)

### Vídeo demonstrativo
<link YouTube/Vimeo>
```

**Diagrama de arquitetura:** pode ser ASCII art inline ou imagem em `docs/architecture/fase2-diagram.png`.

**Depende de:** M1 completo, M2 validado, M4 rodando

---

### F2-22 — Vídeo demonstrativo (até 15 minutos)

**Roteiro obrigatório conforme o rubric:**

| Segmento | O que mostrar | Tempo sugerido |
|---|---|---|
| 1 — Arquitetura | Diagrama do README, explicar componentes e fluxo de deploy | ~2 min |
| 2 — CI/CD | Abrir GitHub Actions, mostrar `coverage.yml` passando → `deploy.yml` disparando → etapas build, migrate, deploy | ~3 min |
| 3 — APIs | Consumir no Swagger/Postman: criar OS, mudar status, listar com ordenação correta, aprovar orçamento via `budget-decision` | ~4 min |
| 4 — Escalabilidade | `kubectl get hpa -w`, simular carga com `hey` ou `k6`, mostrar pods escalando, `kubectl get pods -w` | ~3 min |
| 5 — Encerramento | Mostrar cluster saudável, resumir o que foi entregue | ~1 min |

**Comandos úteis para o segmento 4 (demonstração de HPA):**
```bash
# Simular carga (instalar hey: brew install hey)
hey -n 10000 -c 50 http://localhost:8080/api/service-orders

# Acompanhar escalamento em tempo real
kubectl get hpa mechanics-software-api -n mechanics-software -w
kubectl get pods -n mechanics-software -w
```

Publicar no YouTube ou Vimeo (público ou não listado). Inserir link no README (F2-21).

---

### F2-23 — Submissão no portal do aluno

Checklist completo antes de submeter:

**Repositório:**
- [ ] Compartilhar com o usuário `soat-architecture` no GitHub (Settings → Collaborators)
- [ ] Branch `main` com todos os PRs da Fase 2 mergeados
- [ ] `coverage.yml` e `deploy.yml` passando na `main`
- [ ] Pasta `/k8s` com todos os manifestos
- [ ] Pasta `/infra` com Terraform + `infra/README.md`
- [ ] `README.md` raiz com seção Fase 2 completa

**PDF para o portal:**
- [ ] Link do repositório GitHub
- [ ] Diagrama de arquitetura com os recursos escolhidos (pode ser screenshot do README)
- [ ] Link do vídeo (YouTube/Vimeo)

**Submeter no portal FIAP.**

**Depende de:** F2-21, F2-22

---

## Oportunidades de paralelismo

| Grupo | Tarefas | Quando iniciar |
|---|---|---|
| Correções de API (independentes) | F2-01, F2-02 | Imediatamente |
| Interface + implementação de e-mail | F2-03, F2-04 | Imediatamente |
| Manifestos Kubernetes | F2-08 a F2-12 | Imediatamente (independe do código) |
| Terraform | F2-14 a F2-16 | Imediatamente |
| Injeção de e-mail + testes | F2-05, F2-06, F2-07 | Após F2-03 e F2-04 |
| CI/CD deploy | F2-18, F2-19 | Após F2-10 e K8s validado |
| Documentação | F2-21 | Após M1 + M2 concluídos |
| Vídeo | F2-22 | Após F2-21 e pipeline funcionando |

---

## Sugestão de distribuição por membro

| Membro | Tarefas sugeridas |
|---|---|
| **Joelma** | F2-01, F2-02, F2-05, F2-06, F2-07 (Application/domain) |
| **Allan** | F2-08, F2-09, F2-10, F2-11, F2-12, F2-13 (Kubernetes) |
| **Daniel** | F2-14, F2-15, F2-16, F2-17 (Terraform) |
| **Diogo** | F2-18, F2-19, F2-20 (CI/CD) |
| **Lucas** | F2-03, F2-04 (e-mail) + F2-21, F2-23 (documentação) |

> A distribuição é sugestão — ajustar conforme disponibilidade do time.

---

## Resumo de tarefas

| ID | Tarefa | Milestone | Depende de | Esforço |
|---|---|---|---|---|
| F2-01 | Fix listagem de OS | M1 | — | P |
| F2-02 | Endpoint único de aprovação | M1 | — | P |
| F2-03 | Interface `IEmailNotifier` | M1 | — | P |
| F2-04 | Implementação SMTP/SendGrid | M1 | F2-03 | M |
| F2-05 | Injetar notificador nos use cases | M1 | F2-03, F2-04 | M |
| F2-06 | Testes unitários e-mail | M1 | F2-03, F2-05 | P |
| F2-07 | Testes de integração API | M1 | F2-01, F2-02 | P |
| F2-08 | Namespace + ConfigMap | M2 | — | P |
| F2-09 | Secrets K8s | M2 | — | P |
| F2-10 | Deployment + Service da API | M2 | F2-08, F2-09 | M |
| F2-11 | Deployment + Service + PVC do DB | M2 | F2-08, F2-09 | M |
| F2-12 | HPA | M2 | F2-10 | P |
| F2-13 | Validação local do cluster | M2 | F2-08 a F2-12 | M |
| F2-14 | Estrutura base Terraform | M3 | — | P |
| F2-15 | Módulo K8s cluster | M3 | F2-14 | M |
| F2-16 | Módulo banco de dados | M3 | F2-14 | M |
| F2-17 | Documentação Terraform | M3 | F2-15, F2-16 | P |
| F2-18 | Workflow Docker build + push | M4 | F2-10 | M |
| F2-19 | Workflow deploy K8s | M4 | F2-13, F2-18 | M |
| F2-20 | Workflow migration DB | M4 | F2-19 | M |
| F2-21 | README.md Fase 2 | M5 | M1, M2 | M |
| F2-22 | Vídeo demonstrativo | M5 | F2-21, M4 | G |
| F2-23 | Submissão no portal | M5 | F2-22 | P |

**Legenda de esforço:** P = Pequeno (< 2h) · M = Médio (2–4h) · G = Grande (> 4h)
