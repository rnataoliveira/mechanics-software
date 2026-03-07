# ADR-003: Escolha do Banco de Dados — PostgreSQL

**Status:** Aceito
**Data:** 2026-03-06

## Contexto

O enunciado permite livre escolha de banco de dados, mas exige justificativa. O domínio do sistema envolve entidades com relacionamentos fortes (Cliente → Veículo → OS → Itens → Peças), controle transacional (estoque) e histórico de movimentações.

## Decisão

Usar **PostgreSQL 16** como banco de dados principal.

## Justificativa técnica

| Critério | PostgreSQL |
|---|---|
| Relacionamentos | Suporte nativo a FK, JOIN e integridade referencial |
| Transações | ACID completo — essencial para baixa de estoque e mudança de status |
| Validações | CHECK constraints para CPF/CNPJ e status da OS |
| Concorrência | MVCC evita locks desnecessários em leituras simultâneas |
| Ecossistema | Suporte excelente via Prisma, amplamente usado em produção |
| JSON | JSONB nativo para dados semi-estruturados se necessário |

## Modelo de dados (resumo)

```
clientes          (id, tipo_pessoa, documento, nome, email, telefone)
veiculos          (id, placa, marca, modelo, ano, cliente_id)
servicos          (id, nome, descricao, preco_base, tempo_previsto_min)
pecas             (id, codigo, nome, preco_unitario, quantidade_estoque)
ordens_servico    (id, cliente_id, veiculo_id, status, valor_total, criado_em)
itens_os_servico  (id, os_id, servico_id, quantidade, valor_unitario)
itens_os_peca     (id, os_id, peca_id, quantidade, valor_unitario)
orcamentos        (id, os_id, valor_servicos, valor_pecas, total, status)
movimentacoes_estoque (id, peca_id, tipo, quantidade, referencia, criado_em)
usuarios          (id, email, senha_hash, role)
```

## Alternativas consideradas

| Banco | Motivo da não escolha |
|---|---|
| MySQL | PostgreSQL tem melhor suporte a tipos avançados; sintaxe mais expressiva |
| MongoDB | Domínio relacional — dados estruturados com FK; ACID é requisito |
| SQLite | Não adequado para produção com concorrência; apenas para testes |
