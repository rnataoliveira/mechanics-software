variable "cluster_name" {
  description = "Kind cluster name"
  type        = string
  default     = "mechanics-software"
}

variable "db_name" {
  description = "PostgreSQL database name"
  type        = string
  default     = "mechanics_software"
}

variable "db_user" {
  description = "PostgreSQL user"
  type        = string
  default     = "postgres"
}

variable "db_password" {
  description = "PostgreSQL password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret (minimum 32 characters)"
  type        = string
  sensitive   = true
}

variable "smtp_host" {
  description = "SMTP server host"
  type        = string
  default     = ""
}

variable "smtp_port" {
  description = "SMTP server port"
  type        = number
  default     = 587
}

variable "smtp_user" {
  description = "SMTP username"
  type        = string
  default     = ""
}

variable "smtp_pass" {
  description = "SMTP password"
  type        = string
  sensitive   = true
  default     = ""
}
