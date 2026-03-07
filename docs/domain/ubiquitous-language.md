# Linguagem Ubiqua — Mechanics Software

## Termos do dominio

| Termo | Definicao |
|---|---|
| **Ordem de Servico (OS)** | Documento central que registra todo o atendimento: cliente, veiculo, servicos, pecas, orcamento e status |
| **Cliente** | Pessoa fisica (CPF) ou juridica (CNPJ) que traz o veiculo para atendimento |
| **Veiculo** | Automovel identificado por placa, marca, modelo e ano; sempre vinculado a um cliente |
| **Servico** | Trabalho tecnico a ser executado (ex: troca de oleo, alinhamento, balanceamento) |
| **Peca** | Item fisico utilizado na execucao de um servico, com controle de estoque |
| **Insumo** | Material consumivel utilizado durante os servicos (ex: oleo, fluidos) |
| **Orcamento** | Valor total calculado automaticamente com base nos servicos e pecas da OS; enviado ao cliente para aprovacao |
| **Aprovacao** | Autorizacao formal do cliente para que os servicos sejam executados |
| **Diagnostico** | Avaliacao tecnica do veiculo realizada antes da execucao dos servicos |
| **Execucao** | Fase de realizacao dos servicos aprovados pelo cliente |
| **Entrega** | Devolucao do veiculo ao cliente apos conclusao de todos os servicos |
| **Estoque** | Controle de quantidade disponivel de pecas e insumos |
| **Reserva** | Bloqueio temporario de quantidade em estoque para uma OS especifica |
| **Movimentacao de Estoque** | Registro de toda alteracao (entrada ou saida) no estoque de uma peca |
| **Item de OS** | Servico ou peca associado a uma Ordem de Servico |
| **Atendente** | Funcionario responsavel por criar a OS, registrar servicos e comunicar com o cliente |
| **Mecanico** | Funcionario responsavel por executar o diagnostico e os servicos |
| **Administrador** | Usuario com acesso completo ao sistema, incluindo CRUDs e relatorios |

## Status da Ordem de Servico

| Status | Descricao | Quem aciona |
|---|---|---|
| `RECEBIDA` | OS criada, veiculo na oficina | Sistema (ao criar OS) |
| `EM_DIAGNOSTICO` | Mecanico avaliando o veiculo | Mecanico |
| `AGUARDANDO_APROVACAO` | Orcamento gerado e enviado ao cliente | Sistema (ao enviar orcamento) |
| `EM_EXECUCAO` | Cliente aprovou; servicos em andamento | Sistema (apos aprovacao) |
| `FINALIZADA` | Todos os servicos concluidos | Mecanico |
| `ENTREGUE` | Veiculo devolvido ao cliente | Atendente |
| `CANCELADA` | Cliente rejeitou orcamento | Sistema (apos rejeicao) |

## Transicoes validas de status

```
RECEBIDA --> EM_DIAGNOSTICO
EM_DIAGNOSTICO --> AGUARDANDO_APROVACAO
AGUARDANDO_APROVACAO --> EM_EXECUCAO     (aprovacao)
AGUARDANDO_APROVACAO --> CANCELADA       (rejeicao)
EM_EXECUCAO --> FINALIZADA
FINALIZADA --> ENTREGUE
```

Qualquer outra transicao e invalida e deve lancar excecao de dominio.

## Regras de nomenclatura no codigo

- Classes de dominio usam os termos exatos desta linguagem ubiqua
- Nomes de tabelas no banco refletem os termos (em snake_case)
- Endpoints REST usam os termos em ingles para padronizacao de API publica
- Comentarios e documentacao interna usam os termos em portugues
