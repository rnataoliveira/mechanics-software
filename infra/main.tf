module "kubernetes" {
  source       = "./modules/kubernetes"
  cluster_name = var.cluster_name
  aws_region   = var.aws_region
}
