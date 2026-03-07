# Event Storming — Mechanics Software

## Legenda

| Simbolo | Tipo | Descricao |
|---|---|---|
| `[EVT]` | Evento de Dominio | Algo que aconteceu no sistema (laranja) |
| `[CMD]` | Comando | Intencao/acao que dispara um evento (azul) |
| `[AGG]` | Agregado | Entidade que executa o comando (amarelo) |
| `[POL]` | Politica / Regra | Reacao automatica a um evento (roxo) |
| `[ACT]` | Ator | Quem dispara o comando (bege) |
| `[HOT]` | Hot Spot | Duvida ou problema em aberto (vermelho) |

---

## Fluxo 1 — Criacao e Acompanhamento da Ordem de Servico

### Passo 1 — Identificacao do cliente

```
[ACT] Cliente
  |
  v
[CMD] Identificar cliente por CPF/CNPJ
  |
  v
[AGG] Cliente
  |
  +-- encontrado --> [EVT] Cliente Identificado
  |
  +-- nao encontrado --> [CMD] Cadastrar Cliente
                              |
                              v
                         [POL] CPF/CNPJ deve ser valido (algoritmo de digito verificador)
                         [POL] Email deve ter formato valido
                              |
                              v
                         [EVT] Cliente Cadastrado
```

---

### Passo 2 — Identificacao do veiculo

```
[ACT] Atendente
  |
  v
[CMD] Localizar Veiculo por placa
  |
  v
[AGG] Veiculo
  |
  +-- encontrado --> [EVT] Veiculo Localizado
  |
  +-- nao encontrado --> [CMD] Cadastrar Veiculo
                              |
                              v
                         [POL] Placa deve ser valida (Mercosul ABC1D23 ou antigo ABC-1234)
                         [POL] Veiculo deve estar vinculado a um cliente existente
                              |
                              v
                         [EVT] Veiculo Cadastrado
```

---

### Passo 3 — Abertura da Ordem de Servico

```
[ACT] Atendente
  |
  v
[CMD] Abrir Ordem de Servico
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] Status inicial deve ser RECEBIDA
[POL] OS deve ter cliente e veiculo validos
  |
  v
[EVT] Ordem de Servico Criada (status: RECEBIDA)
```

---

### Passo 4 — Composicao da OS

```
[ACT] Atendente / Mecanico
  |
  +-- [CMD] Adicionar Servico a OS
  |         |
  |         v
  |    [AGG] OrdemDeServico
  |         |
  |         v
  |    [POL] OS deve estar em status RECEBIDA ou EM_DIAGNOSTICO
  |         |
  |         v
  |    [EVT] Servico Adicionado a OS
  |
  +-- [CMD] Adicionar Peca a OS
            |
            v
       [AGG] OrdemDeServico
            |
            v
       [POL] OS deve estar em status RECEBIDA ou EM_DIAGNOSTICO
       [POL] Verificar disponibilidade em estoque
            |
            +-- disponivel --> [EVT] Peca Adicionada a OS
            |                  [EVT] Peca Reservada no Estoque
            |
            +-- indisponivel --> [HOT] Estoque insuficiente
                                  Como lidar? Avisar atendente? Bloquear adicao?
```

---

### Passo 5 — Geracao e envio do orcamento

```
[ACT] Sistema / Atendente
  |
  v
[CMD] Gerar Orcamento
  |
  v
[AGG] Orcamento
  |
  v
[POL] Valor total = soma(valor * qtd de cada servico) + soma(valor * qtd de cada peca)
[POL] OS deve ter ao menos um servico
  |
  v
[EVT] Orcamento Gerado

  |
  v
[CMD] Enviar Orcamento ao Cliente
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] Status muda para AGUARDANDO_APROVACAO automaticamente
  |
  v
[EVT] Orcamento Enviado ao Cliente
[EVT] Status da OS atualizado para AGUARDANDO_APROVACAO
```

---

### Passo 6 — Aprovacao ou rejeicao

```
[ACT] Cliente
  |
  +-- [CMD] Aprovar Orcamento
  |         |
  |         v
  |    [AGG] OrdemDeServico
  |         |
  |         v
  |    [POL] Status muda para EM_EXECUCAO
  |         |
  |         v
  |    [EVT] Orcamento Aprovado
  |    [EVT] Status da OS atualizado para EM_EXECUCAO
  |
  +-- [CMD] Rejeitar Orcamento
            |
            v
       [AGG] OrdemDeServico
            |
            v
       [POL] Status muda para CANCELADA
       [POL] Reservas de estoque devem ser liberadas
            |
            v
       [EVT] Orcamento Rejeitado
       [EVT] Status da OS atualizado para CANCELADA
       [EVT] Reservas de Peca Liberadas
```

---

### Passo 7 — Diagnostico

```
[ACT] Mecanico
  |
  v
[CMD] Iniciar Diagnostico
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] Transicao valida: RECEBIDA --> EM_DIAGNOSTICO
  |
  v
[EVT] Diagnostico Iniciado
[EVT] Status da OS atualizado para EM_DIAGNOSTICO

[HOT] O diagnostico pode descobrir novos servicos/pecas.
      Isso exige voltar ao Passo 4 antes de gerar o orcamento.
```

---

### Passo 8 — Execucao

```
[ACT] Mecanico
  |
  v
[CMD] Executar Servicos
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] OS deve estar em status EM_EXECUCAO
[POL] Ao usar uma peca, baixar do estoque
  |
  v
[EVT] Servico Executado
[EVT] Peca Utilizada na OS
[EVT] Peca Baixada do Estoque
[EVT] Movimentacao de Estoque Registrada
```

---

### Passo 9 — Finalizacao e entrega

```
[ACT] Mecanico
  |
  v
[CMD] Finalizar Ordem de Servico
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] Transicao valida: EM_EXECUCAO --> FINALIZADA
  |
  v
[EVT] Ordem de Servico Finalizada
[EVT] Status da OS atualizado para FINALIZADA

  |
  v
[ACT] Atendente
  |
  v
[CMD] Registrar Entrega do Veiculo
  |
  v
[AGG] OrdemDeServico
  |
  v
[POL] Transicao valida: FINALIZADA --> ENTREGUE
  |
  v
[EVT] Veiculo Entregue ao Cliente
[EVT] Status da OS atualizado para ENTREGUE
```

---

## Fluxo 2 — Gestao de Pecas e Estoque

### Cadastro de pecas

```
[ACT] Administrador
  |
  v
[CMD] Cadastrar Peca
  |
  v
[AGG] Peca
  |
  v
[POL] Codigo da peca deve ser unico
[POL] Preco unitario deve ser positivo
[POL] Quantidade inicial de estoque >= 0
  |
  v
[EVT] Peca Cadastrada

  |
  v
[CMD] Atualizar dados da Peca
  |
  v
[EVT] Peca Atualizada
```

---

### Movimentacao de estoque

```
[ACT] Administrador / Sistema
  |
  +-- [CMD] Repor Estoque
  |         |
  |         v
  |    [AGG] Peca
  |         |
  |         v
  |    [POL] Quantidade deve ser positiva
  |         |
  |         v
  |    [EVT] Estoque Reposto
  |    [EVT] Movimentacao de Estoque Registrada (tipo: ENTRADA)
  |
  +-- [CMD] Reservar Peca para OS
  |         |
  |         v
  |    [AGG] Peca
  |         |
  |         v
  |    [POL] Saldo disponivel deve ser >= quantidade solicitada
  |         |
  |         +-- ok --> [EVT] Peca Reservada para OS
  |         |          [EVT] Movimentacao de Estoque Registrada (tipo: RESERVA)
  |         |
  |         +-- insuficiente --> [EVT] Estoque Insuficiente Identificado
  |                               [HOT] Bloquear adicao a OS ou permitir com aviso?
  |
  +-- [CMD] Confirmar Uso de Peca (apos execucao)
  |         |
  |         v
  |    [AGG] Peca
  |         |
  |         v
  |    [POL] Reserva deve existir para a OS
  |    [POL] Estoque nao pode ficar negativo
  |         |
  |         v
  |    [EVT] Uso de Peca Confirmado
  |    [EVT] Peca Baixada do Estoque
  |    [EVT] Movimentacao de Estoque Registrada (tipo: SAIDA)
  |
  +-- [CMD] Liberar Reserva de Peca (quando OS e cancelada)
            |
            v
       [AGG] Peca
            |
            v
       [POL] Reserva deve existir para a OS
            |
            v
       [EVT] Reserva de Peca Liberada
       [EVT] Movimentacao de Estoque Registrada (tipo: LIBERACAO)
```

---

## State Machine — Status da Ordem de Servico

```
                  RECEBIDA
                     |
                     v
              EM_DIAGNOSTICO
                     |
                     v
          AGUARDANDO_APROVACAO
               /           \
     (aprova) /             \ (rejeita)
             v               v
        EM_EXECUCAO       CANCELADA
             |
             v
         FINALIZADA
             |
             v
          ENTREGUE
```

### Transicoes validas

| De | Para | Gatilho |
|---|---|---|
| `RECEBIDA` | `EM_DIAGNOSTICO` | Mecanico inicia diagnostico |
| `EM_DIAGNOSTICO` | `AGUARDANDO_APROVACAO` | Orcamento enviado ao cliente |
| `AGUARDANDO_APROVACAO` | `EM_EXECUCAO` | Cliente aprova orcamento |
| `AGUARDANDO_APROVACAO` | `CANCELADA` | Cliente rejeita orcamento |
| `EM_EXECUCAO` | `FINALIZADA` | Mecanico finaliza todos os servicos |
| `FINALIZADA` | `ENTREGUE` | Atendente registra retirada do veiculo |

Qualquer outra transicao deve lancar `InvalidStatusTransitionException`.

---

## Hot Spots (pontos em aberto)

| # | Descricao | Impacto |
|---|---|---|
| 1 | Diagnostico pode descobrir novos servicos — como reabrir composicao da OS? | Medio |
| 2 | Estoque insuficiente ao adicionar peca — bloquear ou avisar? | Alto |
| 3 | Cliente pode consultar status sem autenticacao — como identificar o cliente? | Medio |
| 4 | Cancelamento parcial de itens da OS antes da aprovacao | Baixo |

---

## Atores do sistema

| Ator | Descricao | Permissoes |
|---|---|---|
| **Cliente** | Dono do veiculo | Consultar status da OS (publico), aprovar/rejeitar orcamento |
| **Atendente** | Funcionario do balcao | Criar OS, registrar itens, enviar orcamento, registrar entrega |
| **Mecanico** | Tecnico da oficina | Iniciar diagnostico, iniciar execucao, finalizar OS |
| **Administrador** | Gestor do sistema | Acesso total: CRUDs, relatorios, usuarios |
| **Sistema** | Automacoes internas | Gerar orcamento, mudar status, registrar movimentacoes |
