# Linguagem Ubíqua — Mechanics Software

> Versão em português brasileiro da linguagem ubíqua do domínio.
> Os termos em inglês são mantidos no código, banco de dados e endpoints REST — conforme regras de nomenclatura ao final deste documento.

---

## Termos do Domínio

| Termo | Definição |
|---|---|
| **Ordem de Serviço (OS)** | Documento central que registra todo o atendimento: cliente, veículo, serviços, peças, orçamento e status |
| **Cliente** | Pessoa física (CPF) ou jurídica (CNPJ) que traz um veículo para atendimento |
| **Veículo** | Automóvel identificado por placa, marca, modelo e ano; sempre vinculado a um cliente |
| **Serviço** | Trabalho técnico a ser executado (ex.: troca de óleo, alinhamento, balanceamento) |
| **Peça** | Item físico ou insumo utilizado durante a execução dos serviços (ex.: filtros, óleo, pastilhas, fluidos), sujeito a controle de estoque. Insumos consumíveis são modelados como Peças com um atributo `categoria` — não existe entidade separada de Insumo. |
| **Orçamento** | Custo total calculado automaticamente a partir dos serviços e peças disponíveis; enviado ao cliente para aprovação. Entidade filha da OS — não possui ciclo de vida independente. |
| **Aprovação** | Autorização formal do cliente para prosseguir com os serviços |
| **Diagnóstico** | Avaliação técnica do veículo pelo mecânico antes do início da execução. Deve ocorrer antes da composição de itens na OS. |
| **Execução** | Fase em que os serviços aprovados são realizados |
| **Entrega** | Devolução do veículo ao cliente após a conclusão de todos os serviços |
| **Estoque** | Controle de quantidade disponível para cada peça |
| **Reserva** | Bloqueio temporário de quantidade em estoque para uma OS específica. Apenas peças com disponibilidade `DISPONÍVEL` geram reserva. |
| **Movimentação de Estoque** | Registro de cada alteração de estoque (entrada, saída, reserva ou liberação) para uma peça |
| **Item de Serviço** | Um serviço adicionado à OS, com quantidade e snapshot do preço no momento da composição |
| **Item de Peça** | Uma peça adicionada à OS, com quantidade, snapshot do preço e disponibilidade no momento da composição |
| **Disponibilidade** | Atributo do Item de Peça: `DISPONÍVEL` (estoque suficiente, reserva realizada) ou `INDISPONÍVEL` (estoque insuficiente, atendente alertado, excluído do total do orçamento) |
| **Atendente** | Colaborador responsável por criar a OS, cadastrar itens, enviar o orçamento e registrar a entrega |
| **Mecânico** | Técnico responsável por realizar o diagnóstico e executar os serviços |
| **Administrador** | Usuário com acesso completo ao sistema, incluindo CRUDs e relatórios |

---

## Status da Ordem de Serviço

| Status (código) | Descrição | Disparado por |
|---|---|---|
| `RECEIVED` | OS criada, veículo recebido na oficina | Sistema (na criação da OS) |
| `IN_DIAGNOSIS` | Mecânico está avaliando o veículo | Mecânico |
| `AWAITING_APPROVAL` | Orçamento gerado e enviado ao cliente | Sistema (no envio do orçamento) |
| `IN_EXECUTION` | Cliente aprovou; serviços em andamento | Sistema (na aprovação) |
| `COMPLETED` | Todos os serviços finalizados | Mecânico |
| `DELIVERED` | Veículo devolvido ao cliente | Atendente |
| `CANCELLED` | Cliente rejeitou o orçamento | Sistema (na rejeição) |

---

## Transições de Status Válidas

```
RECEIVED → IN_DIAGNOSIS
IN_DIAGNOSIS → AWAITING_APPROVAL
AWAITING_APPROVAL → IN_EXECUTION     (aprovação)
AWAITING_APPROVAL → CANCELLED        (rejeição)
IN_EXECUTION → COMPLETED
COMPLETED → DELIVERED
```

Qualquer outra transição deve lançar `InvalidStatusTransitionException`.

---

## Regras de Nomenclatura no Código

- Classes do domínio usam os termos em **inglês** desta linguagem ubíqua
- Tabelas do banco de dados seguem os termos em `snake_case` (inglês)
- Endpoints REST usam os termos em inglês
- Comentários e documentação interna usam os termos em inglês
- Esta versão PT-BR serve como referência de comunicação com o negócio e para o vídeo de entrega
