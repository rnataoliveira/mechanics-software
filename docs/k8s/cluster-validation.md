# K8s — Validação local do cluster completo (F2-13)

Guia passo a passo para subir e validar o cluster localmente com **Kind**.

## Pré-requisitos

| Ferramenta | Versão mínima | Instalação |
|---|---|---|
| Docker | 24+ | https://docs.docker.com/get-docker/ |
| kind | 0.23+ | `brew install kind` |
| kubectl | 1.29+ | `brew install kubectl` |

## 1. Criar o cluster Kind

```bash
kind create cluster --name mechanics-software
```

Confirmar que o cluster está acessível:

```bash
kubectl cluster-info --context kind-mechanics-software
```

## 2. Aplicar todos os manifestos

A ordem importa — namespace e recursos de suporte primeiro.

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/pvc.yaml
kubectl apply -f k8s/deployment-db.yaml
kubectl apply -f k8s/service-db.yaml
kubectl apply -f k8s/deployment-api.yaml
kubectl apply -f k8s/service-api.yaml
kubectl apply -f k8s/hpa.yaml
```

Ou em dois passos (namespace precisa existir antes dos demais recursos):

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
```

## 3. Aguardar todos os pods ficarem `Running`

```bash
kubectl get pods -n mechanics-software -w
```

Saída esperada:

```
NAME                                     READY   STATUS    RESTARTS   AGE
mechanics-software-api-xxxxxxxxx-xxxxx   1/1     Running   0          60s
mechanics-software-api-xxxxxxxxx-xxxxx   1/1     Running   0          60s
mechanics-software-db-xxxxxxxxx-xxxxx    1/1     Running   0          45s
```

## 4. Verificar o ConfigMap e o Secret

```bash
kubectl get configmap mechanics-config -n mechanics-software -o yaml
kubectl get secret mechanics-secrets -n mechanics-software
```

## 5. Verificar o HPA

```bash
kubectl get hpa -n mechanics-software
```

Saída esperada:

```
NAME                     REFERENCE                           TARGETS   MINPODS   MAXPODS   REPLICAS
mechanics-software-api   Deployment/mechanics-software-api   0%/70%    2         10        2
```

## 6. Testar a API via port-forward

```bash
kubectl port-forward svc/mechanics-software-api 8080:8080 -n mechanics-software
```

Em outro terminal:

```bash
# Health (sem autenticação)
curl http://localhost:8080/health
# Esperado: HTTP 200 — "Healthy"

# Rota autenticada (sem token → 401 é o comportamento correto)
curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/service-orders
# Esperado: 401
```

## 7. Verificar o PVC

```bash
kubectl get pvc -n mechanics-software
```

Saída esperada:

```
NAME           STATUS   VOLUME   CAPACITY   ACCESS MODES   STORAGECLASS   AGE
postgres-pvc   Bound    ...      1Gi        RWO            standard       2m
```

## 8. Destruir o cluster

```bash
kind delete cluster --name mechanics-software
```

## Checklist completo (F2-13)

- [ ] `kubectl apply -f k8s/` aplicado sem erros
- [ ] Todos os pods em `Running`
- [ ] `GET /health` retorna `200`
- [ ] `GET /api/service-orders` sem token retorna `401`
- [ ] HPA listado com `MINPODS: 2`, `MAXPODS: 10`, `TARGET: 70%`
- [ ] PVC em estado `Bound`
- [ ] Screenshot ou log do cluster saudável salvo em `docs/k8s/`
