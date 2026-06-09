variable "db_password" {
  description = "PostgreSQL password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret"
  type        = string
  sensitive   = true
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mechanics-software"
}
