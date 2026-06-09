locals {
  kubeconfig_path = "${path.root}/.kubeconfig-${var.cluster_name}"
}

resource "null_resource" "kind_cluster" {
  triggers = {
    cluster_name = var.cluster_name
  }

  provisioner "local-exec" {
    command = "kind create cluster --name ${var.cluster_name} --config ${path.module}/kind-config.yaml --kubeconfig ${local.kubeconfig_path}"
  }

  provisioner "local-exec" {
    when    = destroy
    command = "kind delete cluster --name ${self.triggers.cluster_name}"
  }
}
