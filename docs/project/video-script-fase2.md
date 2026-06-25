# Fase 2 — Demo Video Recording Guide

**Target length:** 15–20 minutes  
**Recommended tools:** OBS Studio or Loom (screen + webcam)

---

## Pre-recording checklist

Do all of this **before** hitting record.

- [ ] `docker compose up --build` running and healthy at `http://localhost:8080/swagger`
- [ ] Postman open with `MechanicsSoftware.postman_collection.json` imported
  - `baseUrl` variable set to `http://localhost:8080`
  - Run **Auth / Login** once to pre-populate the token
- [ ] A test email inbox ready to show notifications live
  - Recommended: [Mailtrap.io](https://mailtrap.io) free inbox — set `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `SMTP_FROM` in your local `.env`
- [ ] Browser tabs pre-opened:
  - `http://localhost:8080/swagger`
  - GitHub Actions → recent successful `Coverage Report` run
  - GitHub Actions → recent successful `Deploy` run
  - `https://rnataoliveira.github.io/mechanics-software/` (coverage report)
  - `https://rnataoliveira.github.io/mechanics-software/security/` (security report)
- [ ] VS Code (or Rider) open on the repo root with font size ≥ 14 px
- [ ] Terminal font size ≥ 14 px
- [ ] Resolution: 1920×1080 minimum

---

## Step-by-step script

---

### Step 1 — Introduction (1–2 min)

**Show:** webcam + IDE or slide background

**Cover:**
- Group name: TorqueOS · FIAP POS Tech · 15SOAT
- What the system does: manages service orders, customers, vehicles, parts, and inventory for an auto repair shop
- What Fase 2 added: Clean Architecture migration, email notifications, Kubernetes on AWS EKS, Terraform IaC, full CI/CD pipeline, Postman collection, improved security report

---

### Step 2 — Architecture refactor — Clean Architecture (2–3 min)

**Show:** VS Code file tree expanded on `src/`

Walk through each layer:

```
src/
  MechanicsSoftware.Domain/          # innermost — entities, value objects, no dependencies
  MechanicsSoftware.Application/     # use cases, IEmailNotifier, IAppDbContext abstractions
  MechanicsSoftware.Infrastructure/  # EF Core, SmtpEmailNotifier, JWT — implements Application contracts
  MechanicsSoftware.API/             # controllers, Swagger, DI composition root
```

**Files to open and highlight:**

1. `src/MechanicsSoftware.Application/Abstractions/IEmailNotifier.cs`  
   — abstraction defined in Application (inner layer)

2. `src/MechanicsSoftware.Infrastructure/Notifications/SmtpEmailNotifier.cs`  
   — implementation in Infrastructure (outer layer) — Dependency Inversion Principle

3. `src/MechanicsSoftware.Infrastructure/DependencyInjection.cs`  
   — where `SmtpEmailNotifier` is registered against `IEmailNotifier`

4. `docs/decisions/ADR-005-clean-architecture-migration.md`  
   — the architectural decision record for the migration

**Key point to say:**  
*"The Application layer owns the contract. Infrastructure implements it. The Domain never touches email. This is Clean Architecture's Dependency Inversion Principle in practice — outer layers depend on inner ones, never the reverse."*

---

### Step 3 — Running the app with Docker (1 min)

**Show:** terminal

```bash
docker compose up --build
```

- Wait for `Application started` in the logs
- Switch to `http://localhost:8080/swagger`
- Show all endpoint groups: Auth, Customers, Vehicles, Parts, Services, ServiceOrders, Health

**Mention briefly:**  
The Dockerfile now uses `aspnet:8.0-jammy-chiseled` — runs as non-root user (UID 1654), image went from 262 MB to 129 MB, no shell or package manager inside the container.

---

### Step 4 — API walkthrough via Postman — full service order lifecycle (4–5 min)

Run each request in sequence. Show the response body and HTTP status after each call.

| # | Folder | Request | Expected status |
|---|--------|---------|-----------------|
| 1 | Auth | Login | 200 — token auto-stored |
| 2 | Customers | Create Customer | 201 |
| 3 | Vehicles | Create Vehicle | 201 |
| 4 | Parts | Create Part | 201 |
| 5 | Services | Create Service | 201 |
| 6 | Service Orders | Create Service Order | 201 |
| 7 | Service Orders | Start Diagnosis | 200 — status → `IN_DIAGNOSIS` |
| 8 | Service Orders | Add Service Item | 200 |
| 9 | Service Orders | Add Part Item | 200 |
| 10 | Service Orders | Generate Budget | 200 |
| 11 | Service Orders | Send Budget to Customer | 200 — status → `AWAITING_APPROVAL` |
| 12 | Service Orders | Budget Decision (approve) | 200 — status → `IN_EXECUTION` |
| 13 | Service Orders | Start Execution | 200 |
| 14 | Service Orders | Complete Service Order | 200 — status → `COMPLETED` |
| 15 | Service Orders | Deliver Service Order | 200 — status → `DELIVERED` |
| 16 | Service Orders | Get Status (public) | 200 — **no auth token** — customer-facing endpoint |
| 17 | Service Orders | Metrics — Average Execution Time | 200 — F2-01 endpoint |

**Tips:**
- Keep the Postman response panel visible at all times so the status code is readable
- After step 11 (Send Budget), pause and switch to the email inbox before moving to step 12

---

### Step 5 — Email notifications (1–2 min)

**Show:** email inbox (Mailtrap or Gmail)

After step 11 (Send Budget), switch to your inbox and show the emails that arrived:
- One for `AWAITING_APPROVAL` (send budget)
- One for `IN_EXECUTION` (approve)

Click one open to show the HTML template: customer name, service order ID, new status.

**Then show the code briefly:**

1. `src/MechanicsSoftware.Application/UseCases/ServiceOrders/Handlers/SendBudgetHandler.cs`  
   — show the `await _emailNotifier.SendStatusChangedAsync(...)` call (2–3 lines)

2. Mention all 6 handlers send notifications: StartDiagnosis, SendBudget, Approve, Reject, Complete, Deliver

**Key point to say:**  
*"Email sending is fully decoupled from business logic. The handler calls the abstraction — it doesn't know whether the implementation uses SMTP, SendGrid, or a test double."*

---

### Step 6 — Tests — coverage + security report (2 min)

**Show:** terminal, then browser

Run unit tests live:

```bash
dotnet test tests/MechanicsSoftware.UnitTests
```

Show tests passing. Mention the count (65+ unit tests across Domain, Application, and Infrastructure).

Switch to browser — **Coverage Report:** `https://rnataoliveira.github.io/mechanics-software/`
- Show line coverage %, branch %, per-assembly breakdown
- Mention CI enforces 80% line coverage on every PR

Switch to **Security Report:** `https://rnataoliveira.github.io/mechanics-software/security/`
- Show the Summary cards (Critical / High / Moderate / Low)
- Show the Action Plan section with prioritized remediation steps
- Show the Findings table with advisory links

**Then show Testcontainers integration:**

Open `tests/MechanicsSoftware.IntegrationTests/Fixtures/WebApplicationFactoryFixture.cs`

Show the `PostgreSqlContainer` setup.

**Say:**  
*"Integration tests spin up a real PostgreSQL container via Testcontainers — no external database needed, no environment setup, tests are fully self-contained and reproducible in any CI runner."*

---

### Step 7 — Kubernetes on AWS EKS (2 min)

**Show:** VS Code — `k8s/` folder, then terminal or AWS console

Walk through the manifests:

| File | Purpose |
|------|---------|
| `namespace.yaml` | Dedicated namespace for isolation |
| `configmap.yaml` | Non-secret configuration |
| `secret.yaml` | JWT secret, DB credentials, SMTP credentials |
| `deployment-api.yaml` | API pods + **initContainer** that runs EF migrations before startup |
| `deployment-db.yaml` | PostgreSQL pod |
| `service-api.yaml` | LoadBalancer — exposes the API externally |
| `pvc.yaml` | Persistent volume claim for PostgreSQL data |
| `hpa.yaml` | Horizontal Pod Autoscaler |

**Highlight in `deployment-api.yaml`:** the `initContainer` that runs `dotnet ef database update` — migrations run automatically before the API container starts, no manual intervention needed.

If the cluster is still running, show:
```bash
kubectl get pods -n mechanics-software
kubectl get svc -n mechanics-software
```

If already torn down, show the screenshot from a previous run or point to the CI deploy log.

---

### Step 8 — Terraform IaC (1–2 min)

**Show:** VS Code — `infra/` folder

```
infra/
  main.tf        # EKS cluster module — VPC + node group
  variables.tf   # cluster_name, region, node instance type
  outputs.tf     # cluster endpoint, kubeconfig
  providers.tf   # AWS provider
  modules/kubernetes/
```

Open `main.tf` and show the `module "eks"` block — highlight `cluster_name`, `vpc_id`, `subnet_ids`.

**Key point to say:**  
*"The entire AWS infrastructure — VPC, subnets, EKS cluster, node group — is declared in code. Anyone on the team can reproduce the environment with `terraform apply` and tear it down with `terraform destroy`. No manual console clicks, no snowflake infrastructure."*

---

### Step 9 — CI/CD Pipeline (2 min)

**Show:** GitHub → Actions tab

Open the **Coverage Report** workflow (`.github/workflows/coverage.yml`) and click a recent successful run. Walk through the steps:

1. Checkout
2. Restore → Build
3. Unit tests with coverage collection
4. SonarCloud analysis
5. HTML coverage report (ReportGenerator)
6. Security vulnerability scan
7. Structured security HTML report
8. Enforce 80% line coverage threshold
9. Upload artifact + Deploy to GitHub Pages

Then open the **Deploy** workflow (`.github/workflows/deploy.yml`) and click a recent successful run. Walk through:

1. Build Docker image
2. Push to GitHub Container Registry (GHCR)
3. Configure AWS credentials
4. EF Core migrations via initContainer
5. `kubectl apply -f k8s/` — rolling deploy to EKS

**Key point to say:**  
*"Every merge to main is fully automated — test, build, push, migrate, deploy. Zero manual steps from code review approval to production."*

---

### Step 10 — Closing (30 sec)

- Repository: `github.com/rnataoliveira/mechanics-software`
- Coverage + security report: `rnataoliveira.github.io/mechanics-software/`
- Postman collection: `MechanicsSoftware.postman_collection.json` at repo root
- Swagger: available at `/swagger` when running

---

## Timing reference

| Section | Time |
|---------|------|
| Introduction | 1–2 min |
| Architecture — Clean Architecture | 2–3 min |
| Docker + Swagger | 1 min |
| Postman — full lifecycle | 4–5 min |
| Email notifications | 1–2 min |
| Tests + coverage + security report | 2 min |
| Kubernetes | 2 min |
| Terraform | 1–2 min |
| CI/CD pipeline | 2 min |
| Closing | 30 sec |
| **Total** | **~17–19 min** |

---

## Recording tips

- Do a full dry run first to confirm timing — cold Docker starts can be slow
- For emails, [Mailtrap.io](https://mailtrap.io) shows HTML emails cleanly with no spam filtering
- VS Code and terminal: font size ≥ 14 px, high-contrast theme
- Keep the Postman response panel visible so HTTP status codes are readable throughout the lifecycle demo
- If the EKS cluster is torn down, use the `kubectl get pods` screenshot from the deploy CI run logs
- Record audio in a quiet environment — narration clarity matters more than visual effects
