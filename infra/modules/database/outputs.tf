output "db_host" {
  description = "Database service host within the cluster"
  value       = "postgres.${var.namespace}.svc.cluster.local"
}

output "namespace" {
  description = "Kubernetes namespace"
  value       = var.namespace
}
