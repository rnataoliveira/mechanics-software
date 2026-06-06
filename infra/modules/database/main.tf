resource "kubernetes_namespace" "mechanics" {
  metadata {
    name = var.namespace
  }
}

resource "kubernetes_persistent_volume_claim" "postgres" {
  metadata {
    name      = "postgres-pvc"
    namespace = kubernetes_namespace.mechanics.metadata[0].name
  }

  spec {
    access_modes = ["ReadWriteOnce"]

    resources {
      requests = {
        storage = "1Gi"
      }
    }
  }
}

resource "kubernetes_secret" "mechanics_secrets" {
  metadata {
    name      = "mechanics-secrets"
    namespace = kubernetes_namespace.mechanics.metadata[0].name
  }

  data = {
    db_password = var.db_password
    jwt_secret  = var.jwt_secret
  }
}
