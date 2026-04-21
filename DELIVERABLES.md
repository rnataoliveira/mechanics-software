# Deliverables — Tech Challenge Phase 1

FIAP POS Tech · 15SOAT · Fase 1

---

## Group

**Name:** Pos Tech Fiap - Mechanics Software

## Participants

| Name | RM | Discord Username |
|---|---|---|
| Diogo | RM371224 | @smilipow |
| Lucas | RM371615 | @lucaspamponet |
| Renata | RM371593 | @rnataoliveira |
| Allan | RM373714 | @zer07629 |
| Daniel | RM370852 | @danielthx23 |

---

## Links

| Deliverable | Link | Status |
|---|---|---|
| Repository | https://github.com/rnataoliveira/mechanics-software | ready |
| DDD Documentation | https://miro.com/app/board/uXjVGyCZXBU=/ | ready |
| Demo Video | <!-- YouTube or Drive link --> | pending |
| Vulnerability Report | see section below | ready |

---

## DDD Documentation

Hosted in the repository under `docs/`:

| Document | Path |
|---|---|
| Ubiquitous Language | `docs/domain/ubiquitous-language.md` |
| Event Storming — Service Order | `docs/domain/event-storming.md` |
| Event Storming — Inventory | `docs/domain/event-storming.md` |
| Aggregates and Entities | `docs/domain/aggregates-and-entities.md` |
| Bounded Contexts | `docs/domain/bounded-contexts.md` |
| Domain Storytelling | `docs/domain/domain-storytelling.md` |
| Architecture Overview | `docs/architecture/overview.md` |

---

## Vulnerability Report

Tool used: `dotnet list package --vulnerable --include-transitive` (built-in NuGet vulnerability audit)

Scan date: 2026-04-08

### Findings (before remediation)

| Package | Version | Severity | Advisory | Projects affected |
|---|---|---|---|---|
| `Microsoft.Extensions.Caching.Memory` | 8.0.0 | High | [GHSA-qj66-m88j-hmgj](https://github.com/advisories/GHSA-qj66-m88j-hmgj) | Application, Infrastructure, API, UnitTests, IntegrationTests |
| `Npgsql` | 8.0.0 | High | [GHSA-x9vc-6hfv-hg8c](https://github.com/advisories/GHSA-x9vc-6hfv-hg8c) | Infrastructure, API, UnitTests, IntegrationTests |
| `System.Text.Json` | 8.0.0 | High | [GHSA-hh2w-p6rv-4g7w](https://github.com/advisories/GHSA-hh2w-p6rv-4g7w) | Infrastructure, API |
| `System.Text.Json` | 8.0.0 | High | [GHSA-8g4q-xg66-9fp4](https://github.com/advisories/GHSA-8g4q-xg66-9fp4) | Infrastructure, API |
| `System.Net.Http` | 4.3.0 | High | [GHSA-7jgj-8wvc-jh57](https://github.com/advisories/GHSA-7jgj-8wvc-jh57) | UnitTests, IntegrationTests |
| `System.Text.RegularExpressions` | 4.3.0 | High | [GHSA-cmhx-cq75-c4mj](https://github.com/advisories/GHSA-cmhx-cq75-c4mj) | UnitTests, IntegrationTests |

All findings were **transitive** dependencies — none were directly referenced by the project.

### Remediation

| Package | Action |
|---|---|
| `Microsoft.Extensions.Caching.Memory` | Fixed — updated `Microsoft.EntityFrameworkCore` to 8.0.11, which pulls 8.0.1+ |
| `Npgsql` | Fixed — updated `Npgsql.EntityFrameworkCore.PostgreSQL` to 8.0.11, which pulls Npgsql 8.0.6 |
| `System.Text.Json` | Fixed — resolved transitively via EF Core 8.0.11 update |
| `System.Net.Http` | Fixed — pinned `System.Net.Http` 4.3.4 directly in test projects |
| `System.Text.RegularExpressions` | Fixed — pinned `System.Text.RegularExpressions` 4.3.1 directly in test projects |

### Post-remediation scan

```
O projeto fornecido `MechanicsSoftware.Domain` não tem nenhum pacote vulnerável.
O projeto fornecido `MechanicsSoftware.Application` não tem nenhum pacote vulnerável.
O projeto fornecido `MechanicsSoftware.Infrastructure` não tem nenhum pacote vulnerável.
O projeto fornecido `MechanicsSoftware.API` não tem nenhum pacote vulnerável.
O projeto fornecido `MechanicsSoftware.UnitTests` não tem nenhum pacote vulnerável.
O projeto fornecido `MechanicsSoftware.IntegrationTests` não tem nenhum pacote vulnerável.
```

**Result:** 0 vulnerabilities remaining — all HIGH findings resolved.

---

## Video

Link: <!-- YouTube / Google Drive / etc -->

Duration: <!-- X minutes -->

Contents covered:
- [ ] Architecture explanation
- [ ] DDD documentation walkthrough
- [ ] API demonstration
- [ ] Docker execution
