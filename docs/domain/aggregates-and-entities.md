# Aggregates, Entidades e Value Objects

## Aggregate Roots

### OrdemDeServico (Aggregate Root principal)

O agregado mais complexo do sistema. Encapsula todas as regras de negocio do ciclo de vida da OS.

**Responsabilidades:**
- Gerenciar adicao de servicos e pecas
- Calcular e gerar orcamento
- Controlar transicoes de status (state machine)
- Garantir invariantes de negocio

**Entidades filhas:**
- `ItemServico` — servico adicionado a OS
- `ItemPeca` — peca adicionada a OS
- `Orcamento` — valor calculado para aprovacao

**Regras de invariante:**
- Nao e possivel adicionar itens a uma OS com status diferente de `RECEBIDA` ou `EM_DIAGNOSTICO`
- Nao e possivel iniciar execucao sem orcamento aprovado
- O status so pode avancar conforme a state machine definida

---

### Cliente (Aggregate Root)

**Atributos:**
- `id` — UUID
- `tipoPessoa` — `PF` | `PJ`
- `documento` — Value Object `CpfCnpj`
- `nome` — string
- `email` — Value Object `Email`
- `telefone` — string

**Regras:**
- `documento` e unico por cliente
- `tipoPessoa` determina o formato de validacao do documento

---

### Veiculo (Aggregate Root)

**Atributos:**
- `id` — UUID
- `placa` — Value Object `PlacaVeiculo`
- `marca` — string
- `modelo` — string
- `ano` — number
- `clienteId` — referencia ao Cliente

**Regras:**
- `placa` e unica no sistema
- `ano` nao pode ser maior que ano atual + 1

---

### Peca (Aggregate Root — contexto Inventory)

**Atributos:**
- `id` — UUID
- `codigo` — string unico
- `nome` — string
- `descricao` — string
- `precoUnitario` — Value Object `Dinheiro`
- `quantidadeEstoque` — number

**Entidades filhas:**
- `MovimentacaoDeEstoque`

**Regras:**
- Estoque nao pode ficar negativo
- Toda alteracao de quantidade gera `MovimentacaoDeEstoque`

---

### Servico (Aggregate Root — catalogo)

**Atributos:**
- `id` — UUID
- `nome` — string
- `descricao` — string
- `precoBase` — Value Object `Dinheiro`
- `tempoPrevistoMinutos` — number

---

## Value Objects

### CpfCnpj

```
tipo: 'CPF' | 'CNPJ'
valor: string (apenas digitos)

Validacoes:
- CPF: 11 digitos, algoritmo de digito verificador
- CNPJ: 14 digitos, algoritmo de digito verificador
```

### PlacaVeiculo

```
valor: string

Formatos aceitos:
- Mercosul: ABC1D23 (3 letras, 1 digito, 1 letra, 2 digitos)
- Antigo:   ABC1234 (3 letras, 4 digitos)
```

### Dinheiro

```
valor: number (em centavos — inteiro)
moeda: 'BRL'

Operacoes:
- somar(outro: Dinheiro): Dinheiro
- multiplicar(fator: number): Dinheiro
- toReal(): string  ("R$ 150,00")
```

Usar centavos evita erros de ponto flutuante.

### Email

```
valor: string
Validacao: formato RFC 5322 simplificado
```

### StatusOrdemServico

```
Enum com transicoes validas encapsuladas.
Lanca DomainException para transicoes invalidas.
```

---

## Entidades (sem ser Aggregate Root)

### ItemServico
```
id
ordemServicoId
servicoId
quantidade
valorUnitario (snapshot do preco no momento da adicao)
```

### ItemPeca
```
id
ordemServicoId
pecaId
quantidade
valorUnitario (snapshot do preco no momento da adicao)
```

### Orcamento
```
id
ordemServicoId
valorServicos: Dinheiro
valorPecas: Dinheiro
total: Dinheiro
status: 'PENDENTE' | 'APROVADO' | 'REJEITADO'
geradoEm: Date
```

### MovimentacaoDeEstoque
```
id
pecaId
tipo: 'ENTRADA' | 'SAIDA' | 'RESERVA' | 'LIBERACAO'
quantidade: number
referencia: string (ex: numero da OS)
criadoEm: Date
```
