module "kubernetes" {
  source       = "./modules/kubernetes"
  cluster_name = var.cluster_name
}

module "database" {
  source      = "./modules/database"
  db_password = var.db_password
  jwt_secret  = var.jwt_secret
  namespace   = "mechanics-software"

  depends_on = [module.kubernetes]
}
