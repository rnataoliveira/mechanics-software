output "kubeconfig_path" {
  description = "Path to the generated kubeconfig file"
  value       = local.kubeconfig_path
  depends_on  = [null_resource.kind_cluster]
}
