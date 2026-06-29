# Infra — Terraform (AWS EKS)

Infrastructure as Code para o **Mechanics Software**, provisionando um cluster Kubernetes na AWS (EKS) com VPC dedicada.

## O que é criado

| Recurso | Descrição |
|---------|-----------|
| VPC `10.0.0.0/16` | 3 subnets públicas + 3 privadas, NAT Gateway, DNS habilitado |
| EKS Cluster `mechanics-software` | Kubernetes 1.30, endpoint público |
| Node Group `default` | `t3.small`, 1–3 nós, desired 2 |
| Add-ons | `coredns`, `kube-proxy`, `vpc-cni`, `aws-ebs-csi-driver` |

---

## Pré-requisitos

| Ferramenta | Versão mínima | Instalação |
|------------|--------------|------------|
| Terraform | >= 1.7 | https://developer.hashicorp.com/terraform/install |
| AWS CLI | >= 2.0 | https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html |
| kubectl | >= 1.29 | `brew install kubectl` |

---

## Configurar em uma nova conta AWS

Siga este passo a passo sempre que precisar usar uma conta AWS diferente.

### 1. Criar credenciais na conta AWS

No console AWS da nova conta:

1. Acesse **IAM → Users → Create user**
2. Nome: `mechanics-software-deploy` (ou qualquer nome)
3. Em **Permissions**, selecione **Attach policies directly** e adicione `AdministratorAccess`
4. Após criar, acesse o usuário → **Security credentials → Create access key**
5. Selecione **CLI** como use case
6. Anote o **Access Key ID** e **Secret Access Key** — você só vê a Secret uma vez

### 2. Configurar o AWS CLI localmente

```bash
aws configure
```

Preencha quando solicitado:

```
AWS Access Key ID:     <Access Key ID da nova conta>
AWS Secret Access Key: <Secret Access Key da nova conta>
Default region name:   us-east-1
Default output format: json
```

Verifique se está funcionando:

```bash
aws sts get-caller-identity
```

Saída esperada (com os dados da nova conta):
```json
{
    "UserId": "AIDA...",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/mechanics-software-deploy"
}
```

### 3. Inicializar o Terraform

```bash
cd infra
terraform init
```

> O `terraform.tfstate` é ignorado pelo `.gitignore` — cada ambiente começa do zero, sem conflito com estado anterior.

### 4. Revisar o plano

```bash
terraform plan
```

Revise os recursos que serão criados (VPC, subnets, EKS cluster, node group). Deve mostrar **~54 resources to add**.

### 5. Aplicar a infraestrutura

```bash
terraform apply
```

Digite `yes` quando solicitado. O processo leva **10–15 minutos**.

Ao final, os outputs mostram:

```
cluster_name       = "mechanics-software"
cluster_region     = "us-east-1"
kubeconfig_command = "aws eks update-kubeconfig --name mechanics-software --region us-east-1"
```

### 6. Configurar o kubeconfig

```bash
aws eks update-kubeconfig --name mechanics-software --region us-east-1
```

Verificar que o cluster está saudável:

```bash
kubectl get nodes
```

Saída esperada:
```
NAME                                       STATUS   ROLES    AGE   VERSION
ip-10-0-x-x.us-east-1.compute.internal    Ready    <none>   2m    v1.30.x
ip-10-0-x-x.us-east-1.compute.internal    Ready    <none>   2m    v1.30.x
```

### 7. Fazer deploy da aplicação

```bash
kubectl apply -f ../k8s/
```

Aguardar os pods ficarem prontos:

```bash
kubectl get pods -n mechanics-software --watch
```

Saída esperada:
```
NAME                              READY   STATUS    RESTARTS   AGE
mechanics-api-xxxxxxxxx-xxxxx     1/1     Running   0          2m
postgres-xxxxxxxxx-xxxxx          1/1     Running   0          2m
```

Pegar o endpoint externo da API:

```bash
kubectl get svc mechanics-api-service -n mechanics-software
```

O `EXTERNAL-IP` é a URL pública da API.

---

## Configurar o pipeline CI/CD (GitHub Actions)

Para que o pipeline faça deploy automático na nova conta, atualize os secrets do repositório.

Acesse: **GitHub → Settings → Secrets and variables → Actions**

Atualize os seguintes secrets:

| Secret | Valor |
|--------|-------|
| `AWS_ACCESS_KEY_ID` | Access Key ID criado no passo 1 |
| `AWS_SECRET_ACCESS_KEY` | Secret Access Key criado no passo 1 |
| `AWS_REGION` | `us-east-1` (ou a região escolhida) |
| `KUBE_CONFIG` | Conteúdo do kubeconfig — veja abaixo |

Para obter o `KUBE_CONFIG`:

```bash
cat ~/.kube/config | base64
```

Cole o resultado base64 no secret `KUBE_CONFIG`.

---

## Variáveis

| Variável | Obrigatória | Default | Descrição |
|----------|-------------|---------|-----------|
| `cluster_name` | Não | `mechanics-software` | Nome do cluster EKS |
| `aws_region` | Não | `us-east-1` | Região AWS |

Para sobrescrever:

```bash
terraform apply -var="cluster_name=meu-cluster" -var="aws_region=sa-east-1"
```

---

## Destruir o ambiente

**Importante:** destrua o cluster após gravar o vídeo para evitar cobranças na conta AWS.

```bash
cd infra
terraform destroy
```

Digite `yes` quando solicitado. O processo leva **5–10 minutos**.

Verifique no console AWS que todos os recursos foram removidos (VPC, EKS, EC2).

---

## Estrutura dos arquivos

```
infra/
  main.tf           # módulo kubernetes (chama modules/kubernetes)
  variables.tf      # cluster_name, aws_region
  outputs.tf        # cluster_name, cluster_region, kubeconfig_command
  providers.tf      # AWS provider
  modules/
    kubernetes/
      main.tf       # VPC + EKS cluster + node group
```
