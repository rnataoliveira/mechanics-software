output "kubeconfig_path" {
  description = "Path to the generated kubeconfig file"
  value       = module.kubernetes.kubeconfig_path
}

output "db_host" {
  description = "Database service host within the cluster"
  value       = module.database.db_host
}
