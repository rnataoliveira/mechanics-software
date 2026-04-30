# Swagger Demo — Gravação Automática do Fluxo

Script Playwright que abre o Swagger UI, executa o fluxo completo de Ordem de Serviço e grava um vídeo `.webm` da sessão.

## Pré-requisitos

- Node.js 18+
- Docker + Docker Compose

## Como rodar

```bash
cd scripts/swagger-demo
npm install
npx playwright install chromium   # apenas na primeira vez
npm run demo
```

O script sobe o Docker automaticamente se a API ainda não estiver no ar.  
O vídeo é salvo em `test-results/` ao final da execução.

Para abrir o relatório HTML com o vídeo incorporado:

```bash
npx playwright show-report
```

## Fluxo executado (18 passos)

| # | Método | Endpoint | O que demonstra |
|---|--------|----------|-----------------|
| 1 | POST | `/api/auth/login` | Autenticação, captura JWT |
| 2 | — | *(Authorize)* | Preenche Bearer token no Swagger |
| 3 | POST | `/api/customers` | Cadastro de cliente (CPF aleatório) |
| 4 | POST | `/api/vehicles` | Cadastro de veículo do cliente |
| 5 | POST | `/api/services` | Criação de serviço no catálogo |
| 6 | POST | `/api/parts` | Cadastro de peça no estoque |
| 7 | POST | `/api/service-orders` | Abertura de Ordem de Serviço |
| 8 | POST | `/api/service-orders/{id}/start-diagnosis` | Início do diagnóstico |
| 9 | POST | `/api/service-orders/{id}/services` | Adição de serviço à OS |
| 10 | POST | `/api/service-orders/{id}/parts` | Adição de peça à OS |
| 11 | POST | `/api/service-orders/{id}/budget` | Geração do orçamento |
| 12 | POST | `/api/service-orders/{id}/send-budget` | Envio do orçamento ao cliente |
| 13 | POST | `/api/service-orders/{id}/approve` | Aprovação pelo cliente |
| 14 | POST | `/api/service-orders/{id}/start-execution` | Início da execução |
| 15 | POST | `/api/service-orders/{id}/complete` | Conclusão do serviço |
| 16 | POST | `/api/service-orders/{id}/deliver` | Entrega do veículo |
| 17 | GET  | `/api/service-orders/{id}` | Estado final da OS |
| 18 | GET  | `/api/service-orders/metrics/average-execution-time` | Métrica de tempo médio |

## Dados gerados por execução

Cada execução gera dados únicos para evitar conflitos:

| Campo | Estratégia |
|-------|-----------|
| CPF do cliente | Gerado matematicamente válido e aleatório |
| Placa do veículo | Formato legado brasileiro aleatório (ex: `KTR8521`) |
| Código da peça | Prefixo fixo + timestamp (ex: `OL-5W30-1746012345`) |

## Credenciais padrão

```
Email:  admin@mechanics.local
Senha:  Admin@123
```

Definidas em `DatabaseSeeder.cs` e sobrescrevíveis pelas variáveis de ambiente `SEED_ADMIN_EMAIL` e `SEED_ADMIN_PASSWORD`.

## Arquitetura do script

A interação com o Swagger UI é **visual** (para o vídeo). As chamadas reais à API são feitas via `page.request` do Playwright com o JWT token, o que garante que o fluxo de dados seja 100% confiável independente do estado interno do React no Swagger UI.
