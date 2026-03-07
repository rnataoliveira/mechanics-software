# Bounded Contexts

## Mapa de Contextos

```
+-------------------+     +----------------------+
|   Customers       |     |   Vehicles           |
|                   |     |                      |
|  - CRUD cliente   |<--->|  - CRUD veiculo      |
|  - Validar        |     |  - Vincular cliente  |
|    CPF/CNPJ       |     |  - Validar placa     |
+-------------------+     +----------------------+
          |                         |
          v                         v
+----------------------------------------------+
|           Service Orders                     |
|                                              |
|  - Criar OS                                 |
|  - Adicionar servicos e pecas               |
|  - Gerar orcamento                          |
|  - Enviar para aprovacao                    |
|  - Gerenciar ciclo de vida (status)         |
|  - Consulta de status pelo cliente          |
+----------------------------------------------+
                    |
                    v
          +-------------------+
          |   Inventory       |
          |                   |
          |  - CRUD pecas     |
          |  - Controle       |
          |    estoque        |
          |  - Reserva        |
          |  - Baixa          |
          +-------------------+

         +-------------------+
         |   Auth            |
         |                   |
         |  - Login JWT      |
         |  - Guards         |
         +-------------------+
                 |
           (protege todos
            os contextos
            admin)
```

## Descricao dos Contextos

### Customers
- **Nucleo:** `Cliente`
- **Operacoes:** CRUD, busca por CPF/CNPJ
- **Validacoes:** algoritmo CPF, algoritmo CNPJ
- **Expoe:** `clienteId` para outros contextos

### Vehicles
- **Nucleo:** `Veiculo`
- **Operacoes:** CRUD, busca por placa
- **Dependencia upstream:** Customers (cliente deve existir)
- **Validacoes:** formato de placa Mercosul e antigo

### Service Orders
- **Nucleo:** `OrdemDeServico` + `Orcamento`
- **Operacoes:** criacao, gerenciamento de itens, state machine, aprovacao
- **Dependencias upstream:** Customers, Vehicles, Inventory
- **Expoe:** endpoint publico de consulta de status (sem JWT)

### Inventory
- **Nucleo:** `Peca` + `MovimentacaoDeEstoque`
- **Operacoes:** CRUD pecas, controle de estoque, reserva, baixa
- **Chamado por:** Service Orders (ao adicionar peca a OS e ao confirmar uso)

### Auth
- **Nucleo:** `Usuario` + JWT
- **Operacoes:** login, emissao de token
- **Protege:** todas as rotas administrativas dos demais contextos
- **Rota publica:** `GET /service-orders/:id/status` (consulta pelo cliente)

## Relacoes entre contextos

| Relacao | Tipo | Descricao |
|---|---|---|
| Vehicles → Customers | Conformista | Vehicle usa clienteId; nao valida regras de Cliente |
| Service Orders → Customers | Anti-corruption | OS valida existencia do cliente antes de criar |
| Service Orders → Vehicles | Anti-corruption | OS valida existencia do veiculo antes de criar |
| Service Orders → Inventory | Customer/Supplier | OS solicita reserva; Inventory e o fornecedor |
| Auth → todos | Open Host Service | JWT Guard aplicado via decorator nos modulos |
