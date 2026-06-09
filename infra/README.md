# Infra — Terraform

Infrastructure as Code para o **Mechanics Software**, provisionando um cluster Kubernetes local (Kind) e os recursos de banco de dados PostgreSQL.

## Pré-requisitos

| Ferramenta | Versão mínima | Instalação |
|---|---|---|
| Terraform | >= 1.7 | https://developer.hashicorp.com/terraform/install |
| Kind | >= 0.23 | `brew install kind` |
| kubectl | >= 1.29 | `brew install kubectl` |
| Docker | >= 24 | https://docs.docker.com/get-docker/ |

## Recursos criados

| Módulo | Recurso | Descrição |
|---|---|---|
| `kubernetes` | Kind Cluster | 1 control-plane + 2 workers |
| `database` | Namespace `mechanics-software` | Namespace dedicado no cluster |
| `database` | PVC `postgres-pvc` | Volume persistente de 1Gi (ReadWriteOnce) |
| `database` | Secret `mechanics-secrets` | Credenciais do banco e JWT secret |

## Passo a passo

### 1. Inicializar os providers

```bash
cd infra
terraform init
```

### 2. Visualizar o plano de execução

```bash
terraform plan \
  -var="db_password=postgres" \
  -var="jwt_secret=dev-secret-change-in-production-32chars"
```

### 3. Aplicar a infraestrutura

```bash
terraform apply -auto-approve \
  -var="db_password=postgres" \
  -var="jwt_secret=dev-secret-change-in-production-32chars"
```

### 4. Configurar o kubeconfig

```bash
export KUBECONFIG=$(terraform output -raw kubeconfig_path)
kubectl get nodes
```

Saída esperada:

```
NAME                              STATUS   ROLES           AGE   VERSION
mechanics-software-control-plane  Ready    control-plane   ...   v1.x.x
mechanics-software-worker         Ready    <none>          ...   v1.x.x
mechanics-software-worker2        Ready    <none>          ...   v1.x.x
```

### 5. Verificar os recursos do banco

```bash
kubectl get namespace mechanics-software
kubectl get pvc -n mechanics-software
kubectl get secret mechanics-secrets -n mechanics-software
```

## Variáveis

| Variável | Obrigatória | Default | Descrição |
|---|---|---|---|
| `db_password` | Sim | — | Senha do PostgreSQL |
| `jwt_secret` | Sim | — | Chave JWT (mínimo 32 caracteres) |
| `cluster_name` | Não | `mechanics-software` | Nome do cluster Kind |
| `db_name` | Não | `mechanics_software` | Nome do banco de dados |
| `db_user` | Não | `postgres` | Usuário do banco |
| `smtp_host` | Não | `""` | Host SMTP para notificações |
| `smtp_port` | Não | `587` | Porta SMTP |
| `smtp_user` | Não | `""` | Usuário SMTP |
| `smtp_pass` | Não | `""` | Senha SMTP |

## Destruir o ambiente

```bash
terraform destroy -auto-approve \
  -var="db_password=postgres" \
  -var="jwt_secret=dev-secret-change-in-production-32chars"
```

Isso remove o cluster Kind e todos os recursos provisionados.
