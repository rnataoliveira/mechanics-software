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

## Dados pré-carregados

Na primeira inicialização do container, o `DatabaseSeeder` cria automaticamente:

| Entidade | Nome | ID fixo |
|----------|------|---------|
| Cliente | Carlos Silva | `a1000000-0000-0000-0000-000000000001` |
| Veículo | Toyota Corolla ABC1234 | `b1000000-0000-0000-0000-000000000001` |
| Serviço | Troca de Óleo | `c1000000-0000-0000-0000-000000000001` |
| Peça | Óleo Motor 5W30 | `d1000000-0000-0000-0000-000000000001` |

Também são criados mais 2 clientes, 2 veículos, 2 serviços e 2 peças para demonstrar que o sistema já possui dados cadastrados.

## Fluxo executado (14 passos)

| # | Método | Endpoint | O que demonstra |
|---|--------|----------|-----------------|
| 1 | POST | `/api/auth/login` | Autenticação, captura JWT |
| 2 | — | *(Authorize)* | Preenche Bearer token no Swagger |
| 3 | POST | `/api/service-orders` | Abertura de OS com cliente e veículo pré-cadastrados |
| 4 | POST | `/api/service-orders/{id}/start-diagnosis` | Início do diagnóstico |
| 5 | POST | `/api/service-orders/{id}/services` | Adição de serviço à OS |
| 6 | POST | `/api/service-orders/{id}/parts` | Adição de peça à OS |
| 7 | POST | `/api/service-orders/{id}/budget` | Geração do orçamento |
| 8 | POST | `/api/service-orders/{id}/send-budget` | Envio do orçamento ao cliente |
| 9 | POST | `/api/service-orders/{id}/approve` | Aprovação pelo cliente |
| 10 | POST | `/api/service-orders/{id}/start-execution` | Início da execução |
| 11 | POST | `/api/service-orders/{id}/complete` | Conclusão do serviço |
| 12 | POST | `/api/service-orders/{id}/deliver` | Entrega do veículo |
| 13 | GET  | `/api/service-orders/{id}` | Estado final da OS |
| 14 | GET  | `/api/service-orders/metrics/average-execution-time` | Métrica de tempo médio |

## Credenciais padrão

```
Email:  admin@mechanics.local
Senha:  Admin@123
```

Definidas em `DatabaseSeeder.cs` e sobrescrevíveis pelas variáveis de ambiente `SEED_ADMIN_EMAIL` e `SEED_ADMIN_PASSWORD`.

## Arquitetura do script

A interação com o Swagger UI é **visual** (para o vídeo). As chamadas reais à API são feitas via `page.request` do Playwright com o JWT token, o que garante que o fluxo de dados seja 100% confiável independente do estado interno do React no Swagger UI.

Para endpoints com path param (`{id}`), o Playwright usa `pressSequentially` para digitar o UUID no campo — o que aciona os eventos de teclado que o React espera, tornando o UUID visível no vídeo. O botão Execute é ignorado nesses endpoints (um overlay exibe a resposta real da API no canto inferior direito da tela).
