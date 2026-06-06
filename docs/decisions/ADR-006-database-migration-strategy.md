# ADR-006: Estratégia de Migration do Banco de Dados no Pipeline CI/CD

**Status:** Accepted  
**Date:** 2026-06-06  
**Autores:** Joelma Renata Oliveira

---

## Contexto

Com a adição do pipeline de deploy (Fase 2 — M4), surgiu a necessidade de definir como as migrations do EF Core serão aplicadas no cluster Kubernetes antes de cada atualização da API.

O projeto já possuía uma chamada a `db.Database.MigrateAsync()` no `Program.cs`, o que aplicava as migrations automaticamente na inicialização do container da API. Essa abordagem foi suficiente durante a Fase 1 (Docker Compose), mas apresenta riscos em ambiente Kubernetes com múltiplas réplicas.

Três abordagens foram avaliadas:

1. **Startup da aplicação (`MigrateAsync`)** — migrations aplicadas a cada inicialização do pod.
2. **Job no GitHub Actions** — um step no pipeline conecta no banco diretamente e aplica as migrations antes do deploy.
3. **Init Container no Deployment** — um container de inicialização roda antes do container principal dentro do próprio pod, aplicando as migrations dentro do cluster.

---

## Decisão

Adotar **Init Container** no `deployment-api.yaml` para aplicar as migrations do EF Core antes do container principal da API ser iniciado.

O Init Container usará a imagem SDK (`mcr.microsoft.com/dotnet/sdk:8.0`) e executará `dotnet ef database update`, com a connection string fornecida via Secret do Kubernetes (`mechanics-secrets`).

```yaml
initContainers:
  - name: migrate
    image: mcr.microsoft.com/dotnet/sdk:8.0
    command: ["dotnet", "ef", "database", "update"]
    env:
      - name: ConnectionStrings__DefaultConnection
        valueFrom:
          secretKeyRef:
            name: mechanics-secrets
            key: db_connection_string
```

O job `migrate` no `deploy.yml` do GitHub Actions serve como **orquestrador**: aplica o Deployment (com o Init Container) e aguarda a conclusão do rollout — o Init Container garante que as migrations rodem antes da API subir.

---

## Alternativas Consideradas

### `MigrateAsync()` no startup da aplicação (status quo da Fase 1)

**Vantagens:** zero configuração extra; já existia no `Program.cs`.  
**Desvantagens:** em Kubernetes com múltiplas réplicas, todos os pods executam `MigrateAsync()` simultaneamente — race condition. Se uma migration falha, o pod entra em `CrashLoopBackOff`, dificultando o diagnóstico. Anti-pattern para produção.

### Job no GitHub Actions (step de CI que conecta diretamente no banco)

**Vantagens:** executa uma única vez por deploy; falha rápida no pipeline antes de qualquer pod ser atualizado.  
**Desvantagens:** exige que o banco esteja acessível fora do cluster (não é o caso com Kind local); adiciona complexidade com secrets de connection string no GitHub; acoplamento entre o runner de CI e a rede do cluster.

---

## Consequências

### Positivas

- Migrations aplicadas **uma vez por pod**, dentro do cluster, sem exposição da connection string para o runner de CI.
- Kubernetes garante que o Init Container conclua com sucesso antes de iniciar o container principal — comportamento declarativo e auditável.
- Funciona corretamente com Kind local e com clusters em nuvem, sem alteração de configuração.
- O `kubectl rollout status` no pipeline já cobre implicitamente o sucesso das migrations (rollout só avança se o Init Container passar).

### Negativas / Riscos

- Init Container roda a cada reinício do pod (ex.: liveness probe, node reschedule) — migrations idempotentes são obrigatórias (EF Core garante isso por padrão).
- A imagem SDK (`mcr.microsoft.com/dotnet/sdk:8.0`) é significativamente maior que a imagem de runtime. O pull ocorre apenas no Init Container, que é descartado após a execução — impacto em tempo de startup, não em tamanho da imagem final.
- Requer que a `db_connection_string` esteja no Secret `mechanics-secrets` — coordenação com o módulo Terraform de database (ADR relacionada: módulo `database` do `infra/`).
